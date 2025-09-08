using UnityEngine;

public class PatrolState : AIStateBase
{
    private CombatManager _combat;
    private Transform _patrolTarget;  // 単一の目標地点
    private float _lastEnemyCheckTime = 0f;
    private const float ENEMY_CHECK_INTERVAL = 0.2f;

    public PatrolState(UnitController unit) : base(unit)
    {
        _combat = unit.GetComponent<CombatManager>();
    }

    public override void Enter()
    {
        Debug.Log($"{unit.gameObject.name}: 遊撃状態開始");
        unit.SetColor(Color.blue);

        _combat?.SetNormalSpeed();

        // Squadから遊撃対象を取得
        Squad squad = unit.MySquad;

        if (squad != null && squad.PatrolTargets != null && squad.PatrolTargets.Length > 0)
        {
            // 最初の目標を遊撃対象として設定
            _patrolTarget = squad.PatrolTargets[0];
        }

        if (_patrolTarget != null)
        {
            // 目標地点にまっすぐ向かう
            unit.MoveTo(_patrolTarget.position);
            Debug.Log($"{unit.gameObject.name}: 遊撃対象に向かう - {_patrolTarget.name}");
        }
        else
        {
            Debug.LogWarning($"{unit.gameObject.name}: 遊撃対象が設定されていません、防衛モードに移行");
            unit.GetStateMachine().ChangeState(AIState.Defend);
            return;
        }

        _lastEnemyCheckTime = 0f;
    }

    public override void Update()
    {
        if (_combat == null || _combat.IsDead) return;

        // 敵検出（最優先）
        CheckForEnemies();

        // 目標地点への移動確認
        CheckPatrolMovement();
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
        if (Time.time - _lastEnemyCheckTime > ENEMY_CHECK_INTERVAL)
        {
            // 攻撃可能範囲内の敵を検索
            Transform enemy = _combat.FindEnemyInChaseRange();

            if (enemy != null)
            {
                Debug.Log($"{unit.gameObject.name}: 遊撃中に敵発見！ - {enemy.name}");

                // ターゲットを設定
                _combat.CurrentTarget = enemy;

                // 攻撃状態に移行
                unit.GetStateMachine().ChangeState(AIState.Attack);
                return;
            }

            _lastEnemyCheckTime = Time.time;
        }
    }

    /// <summary>
    /// 遊撃移動の確認
    /// </summary>
    private void CheckPatrolMovement()
    {
        if (_patrolTarget == null)
        {
            Debug.LogWarning($"{unit.gameObject.name}: 遊撃対象を見失いました、防衛モードに移行");
            unit.GetStateMachine().ChangeState(AIState.Defend);
            return;
        }

        // 移動が完了しているかチェック
        if (!unit.IsMoving())
        {
            // 目標地点に到達
            Debug.Log($"{unit.gameObject.name}: 遊撃対象に到達、防衛モードに移行");
            unit.GetStateMachine().ChangeState(AIState.Defend);
        }
    }

    /// <summary>
    /// 外部から遊撃対象を設定
    /// </summary>
    public void SetPatrolTarget(Transform target)
    {
        _patrolTarget = target;
        if (target != null)
        {
            Debug.Log($"{unit.gameObject.name}: 遊撃対象を設定 - {target.name}");
        }
    }

    /// <summary>
    /// 遊撃を再開（攻撃モードから復帰時）
    /// </summary>
    public void ResumePatrol()
    {
        if (_patrolTarget != null)
        {
            // 目標地点への移動を再開
            unit.MoveTo(_patrolTarget.position);
            Debug.Log($"{unit.gameObject.name}: 遊撃再開 - {_patrolTarget.name}");
        }
        else
        {
            // 目標がない場合は防衛モードに移行
            unit.GetStateMachine().ChangeState(AIState.Defend);
        }
    }
}