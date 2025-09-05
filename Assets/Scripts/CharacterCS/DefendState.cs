using UnityEngine;

public class DefendState : AIStateBase
{
    public DefendState(UnitController unit) : base(unit) { }

    public override void Enter()
    {
        unit.SetColor(Color.green); // 防衛状態は緑
        unit.StopMoving();
    }

    public override void Update()
    {
        // 敵を探索
        Transform enemy = unit.FindNearestEnemy();
        if (enemy != null)
        {
            unit.SetTarget(enemy);
            unit.GetStateMachine().ChangeState(AIState.Attack);
        }
    }

    public override void Exit()
    {
        // 防衛終了時の処理
    }
}