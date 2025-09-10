using UnityEngine;

public class DefendState : AIStateBase
{
    private float alertTimer = 0f;
    private float lastEnemyCheckTime = 0f;
    private const float ENEMY_CHECK_INTERVAL = 0.3f; // 敵検出の間隔
    private CombatManager combat;

    public DefendState(UnitController unit) : base(unit)
    {
        combat = unit.GetComponent<CombatManager>();
    }

    public override void Enter()
    {
        Debug.Log($"{unit.gameObject.name}: 防衛状態開始");
        unit.SetColor(Color.green); // 防衛中は緑色
        unit.StopMoving(); // 移動停止
        alertTimer = 0f;
        lastEnemyCheckTime = 0f;

        // 通常速度に戻す
        combat?.SetNormalSpeed();
    }

    public override void Update()
    {
        if (combat == null || combat.IsDead) return;

        UpdateAlertAnimation();
        CheckForEnemies();
    }

    public override void Exit()
    {
        Debug.Log($"{unit.gameObject.name}: 防衛状態終了");
    }

    /// <summary>
    /// 警戒アニメーション更新
    /// </summary>
    private void UpdateAlertAnimation()
    {
        alertTimer += Time.deltaTime;

        // 簡単な警戒アニメーション（色の変化）
        float alpha = 0.7f + 0.3f * Mathf.Sin(alertTimer * 2f);
        Color alertColor = Color.green;
        alertColor.a = alpha;
        unit.SetColor(alertColor);
    }

    /// <summary>
    /// 敵検出処理
    /// </summary>
    private void CheckForEnemies()
    {
        // 一定間隔で敵をチェック
        if (Time.time - lastEnemyCheckTime > ENEMY_CHECK_INTERVAL)
        {
            // 攻撃開始範囲内の敵を検索
            Transform enemy = combat.FindEnemyInAttackStartRange();

            if (enemy != null)
            {
                Debug.Log($"{unit.gameObject.name}: 敵発見！ - {enemy.name}");

                // ターゲットを設定
                combat.CurrentTarget = enemy;

                // 攻撃状態に移行
                unit.GetStateManager().ChangeState(AIState.Attack);
                return;
            }

            lastEnemyCheckTime = Time.time;
        }
    }
}