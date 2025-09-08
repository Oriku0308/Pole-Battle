using UnityEngine;
using System.Collections.Generic;

public class SquadFormation : MonoBehaviour
{
    [Header("追従設定")]
    [SerializeField] private float _followUpdateInterval = 0.3f;
    [SerializeField] private float _leaderDistance = 2f;
    [SerializeField] private float _memberAvoidanceDistance = 1.5f;

    private Squad _squad;
    private float _lastFollowUpdateTime = 0f;

    public void Initialize(Squad squad)
    {
        _squad = squad;
    }

    /// <summary>
    /// フォーメーション更新
    /// </summary>
    public void UpdateFormation(UnitController leader, List<UnitController> aliveUnits)
    {
        if (leader == null || leader.GetComponent<CombatManager>()?.IsDead == true)
        {
            SelectNewLeader(aliveUnits);
            return;
        }

        if (Time.time - _lastFollowUpdateTime > _followUpdateInterval)
        {
            foreach (var member in aliveUnits)
            {
                if (member != leader && member.IsFollowingLeader)
                {
                    UpdateMemberFollow(member, leader);
                }
            }
            _lastFollowUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// 新しい班長を選出
    /// </summary>
    private void SelectNewLeader(List<UnitController> aliveUnits)
    {
        if (aliveUnits.Count > 0)
        {
            _squad.SetLeader(aliveUnits[0]);
        }
    }

    /// <summary>
    /// メンバーの追従更新
    /// </summary>
    private void UpdateMemberFollow(UnitController member, UnitController leader)
    {
        if (leader.IsMoving())
        {
            Vector3 followPosition = GetFollowPosition(member, leader.Position);
            member.MoveTo(followPosition);

            if (member.GetStateMachine().GetCurrentState() != AIState.Move)
            {
                member.GetStateMachine().ChangeState(AIState.Move);
            }
        }
        else
        {
            HandleMemberWhenLeaderStopped(member, leader);
        }
    }

    /// <summary>
    /// 班長停止時のメンバー処理
    /// </summary>
    private void HandleMemberWhenLeaderStopped(UnitController member, UnitController leader)
    {
        Vector3 idealPosition = GetFollowPosition(member, leader.Position);
        float distanceToIdeal = Vector3.Distance(member.Position, idealPosition);

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

    /// <summary>
    /// 追従位置を計算
    /// </summary>
    private Vector3 GetFollowPosition(UnitController member, Vector3 leaderPosition)
    {
        Vector3 direction = (member.Position - leaderPosition).normalized;

        // 方向がゼロの場合はランダム方向
        if (direction.magnitude < 0.1f)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            direction = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle));
        }

        Vector3 targetPosition = leaderPosition + direction * _leaderDistance;
        targetPosition.y = member.Position.y;

        return targetPosition;
    }
}