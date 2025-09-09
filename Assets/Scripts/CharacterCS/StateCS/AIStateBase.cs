using UnityEngine;

/// <summary>
/// AI状態の基底クラス
/// 全てのAI状態（Move, Defend, Attack, Patrol）が継承する抽象クラス
/// </summary>
public abstract class AIStateBase
{
    protected UnitController unit; // 制御対象のユニット

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="unitController">制御対象のユニット</param>
    public AIStateBase(UnitController unitController)
    {
        unit = unitController;
    }

    // --- 各状態で必須実装メソッド ---

    /// <summary>
    /// 状態開始時の処理
    /// この状態に遷移した時に一度だけ実行される
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// 毎フレーム実行される処理
    /// この状態の主要なロジックを実装
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// 状態終了時の処理
    /// 他の状態に遷移する前に一度だけ実行される
    /// </summary>
    public abstract void Exit();

    // --- オプション実装メソッド ---

    /// <summary>
    /// 指定した状態への遷移が可能かチェック
    /// デフォルトでは全ての遷移を許可
    /// 必要に応じて各状態でオーバーライド
    /// </summary>
    /// <param name="newState">遷移先の状態</param>
    /// <returns>遷移可能な場合true</returns>
    public virtual bool CanTransitionTo(AIState newState)
    {
        return true;
    }
}