using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AI状態の定義
/// </summary>
public enum AIState
{
    Move,    // 移動状態
    Defend,  // 防衛状態
    Attack,  // 攻撃状態
    Patrol   // 遊撃状態
}

/// <summary>
/// AIステートマシン
/// ユニットの行動状態を管理し、状態遷移を制御する
/// </summary>
public class AIStateMachine : MonoBehaviour
{
    private Dictionary<AIState, AIStateBase> states; // 状態オブジェクトの辞書
    private AIState currentState; // 現在の状態
    private AIStateBase currentStateObj; // 現在の状態オブジェクト
    private UnitController unit; // 制御対象のユニット

    void Start()
    {
        // コンポーネント取得と初期化
        unit = GetComponent<UnitController>();
        InitializeStates();
        ChangeState(AIState.Defend); // 初期状態を防衛に設定
    }

    /// <summary>
    /// 全ての状態オブジェクトを初期化
    /// </summary>
    void InitializeStates()
    {
        states = new Dictionary<AIState, AIStateBase>();

        // 各状態のインスタンスを作成
        states[AIState.Move] = new MoveState(unit);
        states[AIState.Defend] = new DefendState(unit);
        states[AIState.Attack] = new AttackState(unit);
        states[AIState.Patrol] = new PatrolState(unit);
    }

    /// <summary>
    /// 状態を変更
    /// 現在の状態を終了し、新しい状態を開始する
    /// </summary>
    public void ChangeState(AIState newState)
    {
        // 現在の状態を終了
        if (currentStateObj != null)
        {
            currentStateObj.Exit();
        }

        // 新しい状態に遷移
        currentState = newState;
        currentStateObj = states[newState];
        currentStateObj.Enter();

        Debug.Log($"{gameObject.name}: {newState}状態に変更");
    }

    void Update()
    {
        // 現在の状態の更新処理を実行
        currentStateObj?.Update();
    }

    /// <summary>
    /// 現在のステートを返す
    /// </summary>
    public AIState GetCurrentState()
    {
        return currentState;
    }
}