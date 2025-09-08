using UnityEngine;
using System.Collections.Generic;

public enum AIState
{
    Move,
    Defend,
    Attack,
    Patrol
}

public class AIStateMachine : MonoBehaviour
{
    private Dictionary<AIState, AIStateBase> states;
    private AIState currentState;
    private AIStateBase currentStateObj;
    private UnitController unit;

    void Start()
    {
        unit = GetComponent<UnitController>();
        InitializeStates();
        ChangeState(AIState.Defend); // 初期状態
    }

    void InitializeStates()
    {
        states = new Dictionary<AIState, AIStateBase>();
        states[AIState.Move] = new MoveState(unit);
        states[AIState.Defend] = new DefendState(unit);
        states[AIState.Attack] = new AttackState(unit);
        states[AIState.Patrol] = new PatrolState(unit);
    }

    public void ChangeState(AIState newState)
    {
        if (currentStateObj != null)
        {
            currentStateObj.Exit();
        }
        currentState = newState;
        currentStateObj = states[newState];
        currentStateObj.Enter();
        Debug.Log($"{gameObject.name}: {newState}状態に変更");
    }

    void Update()
    {
        currentStateObj?.Update();
    }

    public AIState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// 初期状態にリセット（モック用）
    /// </summary>
    public void ResetToInitialState()
    {
        // 防衛状態に戻す
        ChangeState(AIState.Defend);

        Debug.Log($"{gameObject.name}: AIStateMachine初期状態リセット");
    }
}