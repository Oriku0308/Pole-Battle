using UnityEngine;
using System.Collections;

[System.Serializable]
public class SquadMovementController
{
    // 定数
    private const float LEADER_DISTANCE_MULTIPLIER = 1.5f;
    private const float ADJUSTED_LEADER_DISTANCE = 1.2f;
    private const float IDEAL_POSITION_THRESHOLD = 0.8f;
    private const float MIN_DIRECTION_MAGNITUDE = 0.1f;
    private const float FULL_CIRCLE_DEGREES = 360f;

    [Header("追従設定")]
    [SerializeField] private float _followUpdateInterval = 0.3f;
    [SerializeField] private float _leaderDistance = 2f;
    [SerializeField] private float _memberAvoidanceDistance = 1.5f;

    [Header("目的地設定")]
    [SerializeField] private Transform _destinationPoint;

    private SquadManager _squadManager;
    private float _lastFollowUpdateTime = 0f;

    public SquadMovementController(SquadManager squadManager)
    {
        _squadManager = squadManager;
    }

    /// <summary>
    /// 班長を指定位置に移動
    /// </summary>
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

    /// <summary>
    /// 移動コマンドを実行
    /// </summary>
    private IEnumerator ExecuteMoveCommand(Vector3 targetPosition)
    {
        _squadManager.CurrentLeader.StopMoving();
        yield return null;

        _squadManager.CurrentLeader.MoveTo(targetPosition);
        ChangeStateIfDifferent(_squadManager.CurrentLeader, AIState.Move);

        // 他の班員に追従開始を指示
        foreach (var member in _squadManager.SpawnedUnits)
        {
            if (member != _squadManager.CurrentLeader && member != null)
            {
                member.StartFollowingLeader();
            }
        }

        _squadManager.OnSquadMoved?.Invoke(targetPosition);
    }

    /// <summary>
    /// 追従行動を更新
    /// </summary>
    public void UpdateFollowBehavior()
    {
        if (_squadManager.CurrentLeader == null) return;

        if (Time.time - _lastFollowUpdateTime > _followUpdateInterval)
        {
            foreach (var member in _squadManager.SpawnedUnits)
            {
                if (member != _squadManager.CurrentLeader && member != null && member.IsFollowingLeader)
                {
                    UpdateMemberFollow(member);
                }
            }
            _lastFollowUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// 班員の追従を更新
    /// </summary>
    private void UpdateMemberFollow(UnitController member)
    {
        if (IsLeaderMoving())
        {
            Vector3 followPosition = GetFollowPosition(member, _squadManager.CurrentLeader.Position);
            member.MoveTo(followPosition);
            ChangeStateIfDifferent(member, AIState.Move);
        }
        else
        {
            UpdateMemberIdlePosition(member);
        }
    }

    /// <summary>
    /// 班長が移動中かチェック
    /// </summary>
    private bool IsLeaderMoving()
    {
        AIState leaderState = _squadManager.CurrentLeader.StateManager.GetCurrentState();
        return leaderState == AIState.Patrol ||
               leaderState == AIState.Move ||
               _squadManager.CurrentLeader.IsMoving();
    }

    /// <summary>
    /// 班長停止時の班員処理
    /// </summary>
    private void UpdateMemberIdlePosition(UnitController member)
    {
        Vector3 idealPosition = GetFollowPosition(member, _squadManager.CurrentLeader.Position);
        float distanceToIdeal = Vector3.Distance(member.Position, idealPosition);

        if (distanceToIdeal > IDEAL_POSITION_THRESHOLD)
        {
            member.MoveTo(idealPosition);
            ChangeStateIfDifferent(member, AIState.Move);
        }
        else
        {
            member.StopFollowing();
        }
    }

    /// <summary>
    /// 指定状態でない場合のみ状態を変更
    /// </summary>
    private void ChangeStateIfDifferent(UnitController unit, AIState newState)
    {
        if (unit.StateManager.GetCurrentState() != newState)
        {
            unit.StateManager.ChangeState(newState);
        }
    }

    /// <summary>
    /// 追従位置を計算
    /// </summary>
    private Vector3 GetFollowPosition(UnitController member, Vector3 leaderPosition)
    {
        Vector3 baseDirection = GetDirectionFromLeader(member, leaderPosition);
        Vector3 targetPosition = leaderPosition + baseDirection * _leaderDistance;
        targetPosition = AvoidOtherMembers(member, targetPosition, leaderPosition);
        targetPosition.y = member.Position.y;
        return targetPosition;
    }

    /// <summary>
    /// 班長からの基本方向を取得
    /// </summary>
    private Vector3 GetDirectionFromLeader(UnitController member, Vector3 leaderPosition)
    {
        Vector3 currentDirection = (member.Position - leaderPosition).normalized;

        if (currentDirection.magnitude < MIN_DIRECTION_MAGNITUDE)
        {
            float randomAngle = Random.Range(0f, FULL_CIRCLE_DEGREES) * Mathf.Deg2Rad;
            currentDirection = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle));
        }

        return currentDirection;
    }

    /// <summary>
    /// 他の班員との衝突回避
    /// </summary>
    private Vector3 AvoidOtherMembers(UnitController member, Vector3 proposedPosition, Vector3 leaderPosition)
    {
        Vector3 finalPosition = proposedPosition;

        foreach (var otherMember in _squadManager.SpawnedUnits)
        {
            if (otherMember == member || otherMember == _squadManager.CurrentLeader || otherMember == null) continue;

            float distanceToMember = Vector3.Distance(finalPosition, otherMember.Position);

            if (distanceToMember < _memberAvoidanceDistance)
            {
                Vector3 avoidDirection = (finalPosition - otherMember.Position).normalized;
                finalPosition = otherMember.Position + avoidDirection * _memberAvoidanceDistance;

                float distanceToLeader = Vector3.Distance(finalPosition, leaderPosition);
                if (distanceToLeader > _leaderDistance * LEADER_DISTANCE_MULTIPLIER)
                {
                    Vector3 toLeader = (leaderPosition - finalPosition).normalized;
                    finalPosition = leaderPosition - toLeader * (_leaderDistance * ADJUSTED_LEADER_DISTANCE);
                }
            }
        }

        return finalPosition;
    }
}