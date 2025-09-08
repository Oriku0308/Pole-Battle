using UnityEngine;

public class PatrolState : AIStateBase
{
    [Header("パトロール設定")]
    [SerializeField] private Transform[] patrolTargets;  // パトロール対象

    private CombatManager combat;
    private int currentTargetIndex = 0;
    private Vector3 currentDestination;
    private float lastEnemyCheckTime = 0f;
    private const float ENEMY_CHECK_INTERVAL = 0.2f;

    public PatrolState(UnitController unit) : base(unit)
    {
        combat = unit.GetComponent<CombatManager>();

        // PatrolTargets コンポーネントから設定を取得
        PatrolTargets patrolComponent = unit.GetComponent<PatrolTargets>();
        if (patrolComponent != null)
        {
            patrolTargets = patrolComponent.Targets;
        }
    }

    public override void Enter()
    {
        Debug.Log($"{unit.gameObject.name}: 遊撃状態開始");
        unit.SetColor(Color.blue); // 遊撃中は青色

        // 通常速度に設定
        combat?.SetNormalSpeed();

        // 最初のパトロール対象に向かう
        SetNextPatrolDestination();
        lastEnemyCheckTime = 0f;
    }

    public override void Update()
    {
        if (combat == null || combat.IsDead) return;

        // 敵検出（最優先）
        CheckForEnemies();

        // パトロール移動処理
        UpdatePatrolMovement();
    }

    public override void Exit()
    {
        Debug.Log($"{unit.gameObject.name}: 遊撃状態終了");
    }

    /// <summary>
    /// 敵検出処理
    /// </summary>
    private void CheckForEnemies()
    {
        if (Time.time - lastEnemyCheckTime > ENEMY_CHECK_INTERVAL)
        {
            // 攻撃可能範囲内の敵を検索（遊撃モードは広範囲で検索）
            Transform enemy = combat.FindEnemyInChaseRange();

            if (enemy != null)
            {
                Debug.Log($"{unit.gameObject.name}: 遊撃中に敵発見！ - {enemy.name}");

                // ターゲットを設定
                combat.CurrentTarget = enemy;

                // 攻撃状態に移行
                unit.GetStateMachine().ChangeState(AIState.Attack);
                return;
            }

            lastEnemyCheckTime = Time.time;
        }
    }

    /// <summary>
    /// パトロール移動更新
    /// </summary>
    private void UpdatePatrolMovement()
    {
        // 移動が完了しているかチェック
        if (!unit.IsMoving())
        {
            // 目標に到達
            HandleDestinationReached();
        }
    }

    /// <summary>
    /// 目標到達時の処理
    /// </summary>
    private void HandleDestinationReached()
    {
        if (patrolTargets == null || patrolTargets.Length == 0)
        {
            // パトロール対象がない場合は防衛モードに移行
            Debug.Log($"{unit.gameObject.name}: パトロール対象なし、防衛モードに移行");
            unit.GetStateMachine().ChangeState(AIState.Defend);
            return;
        }

        // 現在の目標が最終目標の場合
        if (IsAtFinalDestination())
        {
            Debug.Log($"{unit.gameObject.name}: パトロール完了、防衛モードに移行");
            unit.GetStateMachine().ChangeState(AIState.Defend);
        }
        else
        {
            // 次の目標に向かう
            SetNextPatrolDestination();
        }
    }

    /// <summary>
    /// 次のパトロール目標を設定
    /// </summary>
    private void SetNextPatrolDestination()
    {
        if (patrolTargets == null || patrolTargets.Length == 0) return;

        // 現在のインデックスの目標を取得
        Transform target = patrolTargets[currentTargetIndex];

        if (target != null)
        {
            currentDestination = target.position;
            unit.MoveTo(currentDestination);

            Debug.Log($"{unit.gameObject.name}: パトロール目標{currentTargetIndex}に向かう - {target.name}");
        }
        else
        {
            Debug.LogWarning($"{unit.gameObject.name}: パトロール目標{currentTargetIndex}がnullです");
            // 次の目標に進む
            currentTargetIndex++;
        }
    }

    /// <summary>
    /// 最終目標に到達したかチェック
    /// </summary>
    private bool IsAtFinalDestination()
    {
        if (patrolTargets == null || patrolTargets.Length == 0) return true;

        // 最後の目標に到達したか
        if (currentTargetIndex >= patrolTargets.Length - 1)
        {
            return true;
        }

        // 次の目標に進む
        currentTargetIndex++;
        return false;
    }

    /// <summary>
    /// パトロール対象を外部から設定
    /// </summary>
    public void SetPatrolTargets(Transform[] targets)
    {
        patrolTargets = targets;
        currentTargetIndex = 0;
    }
}

/// <summary>
/// パトロール対象を設定するためのコンポーネント
/// </summary>
[System.Serializable]
public class PatrolTargets : MonoBehaviour
{
    [Header("パトロール対象")]
    [SerializeField] private Transform[] _targets;

    public Transform[] Targets => _targets;

    public void SetTargets(Transform[] targets)
    {
        _targets = targets;
    }
}