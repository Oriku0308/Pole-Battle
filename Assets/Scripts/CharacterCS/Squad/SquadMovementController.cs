using UnityEngine;
using System.Collections;

[System.Serializable]
public class SquadMovementController
{
    [Header("追従設定")]
    [SerializeField] private float _followUpdateInterval = 0.3f; // 追従更新間隔
    [SerializeField] private float _leaderDistance = 2f; // 班長からの距離
    [SerializeField] private float _memberAvoidanceDistance = 1.5f; // 班員同士の回避距離

    [Header("目的地設定")]
    [SerializeField] private Transform _destinationPoint; // 目的地

    private SquadManager _squadManager;
    private float _lastFollowUpdateTime = 0f; // 最後の追従更新時間

    public SquadMovementController(SquadManager squadManager)
    {
        _squadManager = squadManager;
    }

    // 班長を指定位置に移動
    public void MoveLeaderTo(Vector3 targetPosition)
    {
        if (_squadManager.CurrentLeader == null)
        {
            Debug.LogWarning($"{_squadManager.GetSquadName()}: 班長が設定されていません");
            return;
        }

        Debug.Log($"{_squadManager.GetSquadName()}: 班長移動指示 - {targetPosition}");
        _squadManager.StartCoroutine(ExecuteMoveCommand(targetPosition));
    }

    // 移動コマンドを実行
    private IEnumerator ExecuteMoveCommand(Vector3 targetPosition)
    {
        _squadManager.CurrentLeader.StopMoving();
        yield return null;

        _squadManager.CurrentLeader.MoveTo(targetPosition);
        _squadManager.CurrentLeader.GetStateMachine().ChangeState(AIState.Move);

        // 他の班員に追従開始を指示
        foreach (var member in _squadManager.SpawnedUnits)
        {
            if (member != _squadManager.CurrentLeader)
            {
                member.StartFollowingLeader();
            }
        }

        _squadManager.OnSquadMoved?.Invoke(targetPosition);
    }

    // 追従行動を更新
    public void UpdateFollowBehavior()
    {
        if (_squadManager.CurrentLeader == null) return;

        // 指定間隔で追従位置を更新
        if (Time.time - _lastFollowUpdateTime > _followUpdateInterval)
        {
            foreach (var member in _squadManager.SpawnedUnits)
            {
                if (member != _squadManager.CurrentLeader && member.IsFollowingLeader)
                {
                    UpdateMemberFollow(member);
                }
            }
            _lastFollowUpdateTime = Time.time;
        }
    }

    // 班員の追従を更新
    private void UpdateMemberFollow(UnitController member)
    {
        if (_squadManager.CurrentLeader.IsMoving())
        {
            // 班長が移動中なら追従
            Vector3 followPosition = GetNaturalFollowPosition(member, _squadManager.CurrentLeader.Position);
            member.MoveTo(followPosition);

            if (member.GetStateMachine().GetCurrentState() != AIState.Move)
            {
                member.GetStateMachine().ChangeState(AIState.Move);
            }
        }
        else
        {
            // 班長が停止中の処理
            HandleMemberWhenLeaderStopped(member);
        }
    }

    // 班長停止時の班員処理
    private void HandleMemberWhenLeaderStopped(UnitController member)
    {
        Vector3 idealPosition = GetNaturalFollowPosition(member, _squadManager.CurrentLeader.Position);
        float distanceToIdeal = Vector3.Distance(member.Position, idealPosition);

        // 理想位置から離れすぎている場合は移動
        if (distanceToIdeal > 0.8f)
        {
            member.MoveTo(idealPosition);

            if (member.GetStateMachine().GetCurrentState() != AIState.Move)
            {
                member.GetStateMachine().ChangeState(AIState.Move);
            }
        }
        else
        {
            member.StopFollowing();
        }
    }

    // 自然な追従位置を計算
    private Vector3 GetNaturalFollowPosition(UnitController member, Vector3 leaderPosition)
    {
        Vector3 baseDirection = GetBaseDirectionFromLeader(member, leaderPosition);
        Vector3 targetPosition = leaderPosition + baseDirection * _leaderDistance;
        targetPosition = AvoidOtherMembers(member, targetPosition, leaderPosition);
        targetPosition.y = member.Position.y; // Y座標は固定
        return targetPosition;
    }

    // 班長からの基本方向を取得
    private Vector3 GetBaseDirectionFromLeader(UnitController member, Vector3 leaderPosition)
    {
        Vector3 currentDirection = (member.Position - leaderPosition).normalized;

        // 方向がない場合はランダムに設定
        if (currentDirection.magnitude < 0.1f)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            currentDirection = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle));
        }

        return currentDirection;
    }

    // 他の班員との衝突回避
    private Vector3 AvoidOtherMembers(UnitController member, Vector3 proposedPosition, Vector3 leaderPosition)
    {
        Vector3 finalPosition = proposedPosition;

        foreach (var otherMember in _squadManager.SpawnedUnits)
        {
            if (otherMember == member || otherMember == _squadManager.CurrentLeader) continue;

            float distanceToMember = Vector3.Distance(finalPosition, otherMember.Position);

            // 他の班員と近すぎる場合は回避
            if (distanceToMember < _memberAvoidanceDistance)
            {
                Vector3 avoidDirection = (finalPosition - otherMember.Position).normalized;
                finalPosition = otherMember.Position + avoidDirection * _memberAvoidanceDistance;

                // 班長から離れすぎないよう調整
                float distanceToLeader = Vector3.Distance(finalPosition, leaderPosition);
                if (distanceToLeader > _leaderDistance * 1.5f)
                {
                    Vector3 toLeader = (leaderPosition - finalPosition).normalized;
                    finalPosition = leaderPosition - toLeader * (_leaderDistance * 1.2f);
                }
            }
        }

        return finalPosition;
    }
}