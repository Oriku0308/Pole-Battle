using UnityEngine;

/// <summary>
/// 目的地移動ステート - 指定された目的地に向かって移動する
/// </summary>
public class PatrolState : AIStateBase
{
    [Header("移動設定")]
    [SerializeField] private float _arrivalThreshold = 1f; // 到着判定の距離

    private CombatManager _combatManager;

    private Vector3 _targetDestination; // 目的地の座標
    private bool _hasDestination = false; // 目的地が設定されているか
    private bool _isMovingToDestination = false; // 目的地に向かって移動中かどうか
    private float _lastEnemyCheckTime = 0f;
    private const float ENEMY_CHECK_INTERVAL = 0.2f;

    public PatrolState(UnitController unitController) : base(unitController)
    {
        _combatManager = unit.GetComponent<CombatManager>();
    }

    public override void Enter()
    {
        Debug.Log($"{unit.gameObject.name}: 目的地移動開始");
        unit.SetColor(Color.blue); // 移動中は青色

        // 通常速度に設定
        _combatManager?.SetNormalSpeed();

        // 目的地が設定されていない場合
        if (!_hasDestination)
        {
            Debug.LogWarning($"{unit.gameObject.name}: 目的地が設定されていません");
            return;
        }

        // 目的地に向かって移動開始
        StartMovingToDestination();
        _lastEnemyCheckTime = 0f;
    }

    public override void Update()
    {
        if (_combatManager == null || _combatManager.IsDead) return;

        // 戦闘中は敵を優先
        CheckForEnemies();

        // 目的地が設定されていない場合は待機
        if (!_hasDestination)
        {
            unit.GetStateMachine().ChangeState(AIState.Defend);
            return;
        }

        // 目的地に向かって移動中の処理
        if (_isMovingToDestination)
        {
            CheckArrivalAtDestination();
        }
    }

    public override void Exit()
    {
        Debug.Log($"{unit.gameObject.name}: 目的地移動終了");
        unit.StopMoving();
    }

    /// <summary>
    /// 敵検出処理
    /// </summary>
    private void CheckForEnemies()
    {
        if (Time.time - _lastEnemyCheckTime > ENEMY_CHECK_INTERVAL)
        {
            // 攻撃開始範囲内の敵を検索
            Transform enemy = _combatManager.FindEnemyInAttackStartRange();
            if (enemy != null)
            {
                Debug.Log($"{unit.gameObject.name}: 移動中に敵発見！ - {enemy.name}");

                // ターゲットを設定
                _combatManager.CurrentTarget = enemy;

                // 攻撃状態に移行
                unit.GetStateMachine().ChangeState(AIState.Attack);
                return;
            }

            _lastEnemyCheckTime = Time.time;
        }
    }

    /// <summary>
    /// 目的地を設定する（Squad.csから呼ばれる）
    /// </summary>
    public void SetDestination(Vector3 destination)
    {
        _targetDestination = destination;
        _hasDestination = true;

        Debug.Log($"{unit.gameObject.name}: 目的地設定 - {destination}");

        // 既に移動ステートの場合は即座に新しい目的地に向かう
        if (unit.GetStateMachine().GetCurrentState() == AIState.Patrol)
        {
            StartMovingToDestination();
        }
    }

    /// <summary>
    /// 目的地をクリアする
    /// </summary>
    public void ClearDestination()
    {
        _hasDestination = false;
        _isMovingToDestination = false;
        unit.StopMoving();

        Debug.Log($"{unit.gameObject.name}: 目的地クリア");
    }

    /// <summary>
    /// 目的地に向かって移動開始
    /// </summary>
    private void StartMovingToDestination()
    {
        if (!_hasDestination) return;

        unit.MoveTo(_targetDestination);
        _isMovingToDestination = true;

        Debug.Log($"{unit.gameObject.name}: 目的地への移動開始 - {_targetDestination}");
    }

    /// <summary>
    /// 目的地到着チェック
    /// </summary>
    private void CheckArrivalAtDestination()
    {
        if (!unit.IsMoving())
        {
            OnArriveAtDestination();
        }
    }

    /// <summary>
    /// 目的地到着時の処理
    /// </summary>
    private void OnArriveAtDestination()
    {
        _isMovingToDestination = false;
        unit.StopMoving();

        Debug.Log($"{unit.gameObject.name}: 目的地に到着");

        // 目的地到着後は防衛状態に移行
        unit.GetStateMachine().ChangeState(AIState.Defend);
    }

    /// <summary>
    /// 状態遷移可否の判定
    /// </summary>
    public override bool CanTransitionTo(AIState newState)
    {
        // 攻撃状態への遷移は常に許可（敵発見時）
        if (newState == AIState.Attack)
        {
            return true;
        }

        // 防衛状態への遷移は許可
        if (newState == AIState.Defend)
        {
            return true;
        }

        // その他の状態への遷移も基本的に許可
        return true;
    }

    // プロパティ
    public bool HasDestination => _hasDestination;
    public Vector3 TargetDestination => _targetDestination;
    public bool IsMovingToDestination => _isMovingToDestination;
}