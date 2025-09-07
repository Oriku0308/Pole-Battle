using UnityEngine;

public abstract class AIStateBase
{
    protected UnitController unit;

    public AIStateBase(UnitController unitController)
    {
        unit = unitController;
    }

    // 各状態で必須実装
    public abstract void Enter();      // 状態開始時
    public abstract void Update();     // 毎フレーム実行
    public abstract void Exit();       // 状態終了時

    // オプション実装
    public virtual bool CanTransitionTo(AIState newState)
    {
        return true;
    }
}