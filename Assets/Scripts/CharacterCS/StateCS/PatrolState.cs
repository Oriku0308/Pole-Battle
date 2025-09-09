using UnityEngine;

/// <summary>
/// パトロール状態の具象クラス
/// 設定された目的地に向かって移動する
/// </summary>
public class PatrolState : AIStateBase
{
    private Vector3 _destination;           // 目的地
    private bool _hasDestination = false;   // 目的地が設定されているか
    private CombatManager _combatManager;   // 戦闘管理

    public PatrolState(UnitController unitController) : base(unitController)
    {
        _combatManager = unit.GetComponent<CombatManager>();
    }

    /// <summary>
    /// パトロール状態開始時の処理
    /// </summary>
    public override void Enter()
    {
        Debug.Log($"{unit.gameObject.name}: パトロール状態開始");

        // 戦闘速度に設定
        if (_combatManager != null)
        {
            _combatManager.SetCombatSpeed();
        }

        // 目的地が設定されていれば移動開始
        if (_hasDestination)
        {
            unit.MoveTo(_destination);
        }
    }

    /// <summary>
    /// パトロール状態の更新処理
    /// 班長専用：目的地への移動と敵探索
    /// </summary>
    public override void Update()
    {
        // 敵を探索
        Transform enemy = SearchForEnemies();
        if (enemy != null)
        {
            // 敵発見：攻撃状態に遷移
            _combatManager.CurrentTarget = enemy;
            unit.GetStateMachine().ChangeState(AIState.Attack);
            return;
        }

        // 目的地への移動チェック（班長のみ）
        if (_hasDestination && unit.IsLeader)
        {
            CheckDestinationReached();
        }
    }

    /// <summary>
    /// パトロール状態終了時の処理
    /// </summary>
    public override void Exit()
    {
        Debug.Log($"{unit.gameObject.name}: パトロール状態終了");

        // 通常速度に戻す
        if (_combatManager != null)
        {
            _combatManager.SetNormalSpeed();
        }
    }

    /// <summary>
    /// 目的地を設定
    /// </summary>
    public void SetDestination(Vector3 destination)
    {
        _destination = destination;
        _hasDestination = true;

        Debug.Log($"{unit.gameObject.name}: パトロール目的地設定 - {destination}");

        // パトロール状態中なら即座に移動開始
        if (unit.GetStateMachine().GetCurrentState() == AIState.Patrol)
        {
            unit.MoveTo(_destination);
        }
    }

    /// <summary>
    /// 目的地をクリア
    /// </summary>
    public void ClearDestination()
    {
        _hasDestination = false;
        Debug.Log($"{unit.gameObject.name}: パトロール目的地クリア");
    }

    /// <summary>
    /// 敵を探索
    /// </summary>
    private Transform SearchForEnemies()
    {
        if (_combatManager == null) return null;

        // 攻撃開始範囲内の敵を探索
        return _combatManager.FindEnemyInAttackStartRange();
    }

    /// <summary>
    /// 目的地到達チェック
    /// </summary>
    private void CheckDestinationReached()
    {
        if (!unit.IsMoving())
        {
            float distanceToDestination = Vector3.Distance(unit.Position, _destination);

            if (distanceToDestination <= 2f) // 目的地到達判定
            {
                Debug.Log($"{unit.gameObject.name}: パトロール目的地到達");

                // 防衛状態に遷移
                unit.GetStateMachine().ChangeState(AIState.Defend);
            }
            else
            {
                // まだ到達していない場合は再度移動
                unit.MoveTo(_destination);
            }
        }
    }

    // プロパティ
    public bool HasDestination => _hasDestination;
    public Vector3 Destination => _destination;
}