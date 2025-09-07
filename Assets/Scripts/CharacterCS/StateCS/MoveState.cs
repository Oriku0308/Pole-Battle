using UnityEngine;

public class MoveState : AIStateBase
{
    public MoveState(UnitController unit) : base(unit) { }

    public override void Enter()
    {
        Debug.Log($"{unit.gameObject.name}: 移動状態開始");
        unit.SetColor(Color.blue);
    }

    public override void Update()
    {
        // 班員の場合は、追従中なら移動状態を維持
        bool isFollowing = unit.GetType().GetField("_isFollowingLeader",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance)?.GetValue(unit) as bool? ?? false;

        if (isFollowing)
        {
            // 追従中は移動状態を維持（DefendStateに移行しない）
            return;
        }

        // 班長の場合、または追従していない場合の通常処理
        if (!unit.IsMoving())
        {
            Debug.Log($"{unit.gameObject.name}: 移動完了、防衛状態へ");
            unit.StopFollowing();
            unit.GetStateMachine().ChangeState(AIState.Defend);
        }
    }

    public override void Exit()
    {
        Debug.Log($"{unit.gameObject.name}: 移動状態終了");
    }
}