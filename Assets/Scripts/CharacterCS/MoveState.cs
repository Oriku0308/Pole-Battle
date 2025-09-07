using UnityEngine;

public class MoveState : AIStateBase
{
    public MoveState(UnitController unit) : base(unit) { }

    public override void Enter()
    {
        Debug.Log($"{unit.gameObject.name}: 移動状態開始");
        unit.SetColor(Color.blue); // 移動中は青色
    }

    public override void Update()
    {
        // 目的地に到着したかチェック
        if (!unit.IsMoving())
        {
            Debug.Log($"{unit.gameObject.name}: 目的地到着、防衛状態へ");
            unit.GetStateMachine().ChangeState(AIState.Defend);
        }
    }

    public override void Exit()
    {
        Debug.Log($"{unit.gameObject.name}: 移動状態終了");
    }
}