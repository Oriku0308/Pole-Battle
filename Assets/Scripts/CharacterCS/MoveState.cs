using UnityEngine;

public class MoveState : AIStateBase
{
    public MoveState(UnitController unit) : base(unit) { }

    public override void Enter()
    {
        // 移動開始時の処理
        unit.SetColor(Color.blue); // 視覚的フィードバック
    }

    public override void Update()
    {
        // 目的地に到着したかチェック
        if (!unit.IsMoving())
        {
            unit.GetStateMachine().ChangeState(AIState.Defend);
        }
    }

    public override void Exit()
    {
        // 移動終了時の処理
    }
}