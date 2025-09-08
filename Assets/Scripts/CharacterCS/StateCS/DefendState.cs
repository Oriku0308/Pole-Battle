using UnityEngine;
public class DefendState : AIStateBase
{
    private float alertTimer = 0f;
    private float lastEnemyCheckTime = 0f;
    private const float ENEMY_CHECK_INTERVAL = 0.3f;
    private CombatManager combat;

    public DefendState(UnitController unit) : base(unit)
    {
        combat = unit.GetComponent<CombatManager>();
    }

    public override void Enter()
    {
        Debug.Log($"{unit.gameObject.name}: 防衛状態開始");
        unit.SetColor(Color.green);
        unit.StopMoving();
        alertTimer = 0f;
        lastEnemyCheckTime = 0f;

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

    private void UpdateAlertAnimation()
    {
        alertTimer += Time.deltaTime;

        float alpha = 0.7f + 0.3f * Mathf.Sin(alertTimer * 2f);
        Color alertColor = Color.green;
        alertColor.a = alpha;
        unit.SetColor(alertColor);
    }

    private void CheckForEnemies()
    {
        if (Time.time - lastEnemyCheckTime > ENEMY_CHECK_INTERVAL)
        {
            Transform enemy = combat.FindEnemyInAttackStartRange();

            if (enemy != null)
            {
                Debug.Log($"{unit.gameObject.name}: 敵発見！ - {enemy.name}");

                combat.CurrentTarget = enemy;
                unit.GetStateMachine().ChangeState(AIState.Attack);
                return;
            }

            lastEnemyCheckTime = Time.time;
        }
    }
}