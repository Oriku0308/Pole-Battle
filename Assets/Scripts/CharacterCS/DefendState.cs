using UnityEngine;

public class DefendState : AIStateBase
{
    private float alertTimer = 0f;

    public DefendState(UnitController unit) : base(unit) { }

    public override void Enter()
    {
        Debug.Log($"{unit.gameObject.name}: 防衛状態開始");
        unit.SetColor(Color.green); // 防衛中は緑色
        unit.StopMoving(); // 移動停止
        alertTimer = 0f;
    }

    public override void Update()
    {
        alertTimer += Time.deltaTime;

        // 簡単な警戒アニメーション（色の変化）
        float alpha = 0.7f + 0.3f * Mathf.Sin(alertTimer * 2f);
        Color alertColor = Color.green;
        alertColor.a = alpha;
        unit.SetColor(alertColor);

        // TODO: 敵検出ロジック（Step 3で実装）
        // Transform enemy = unit.FindNearestEnemy();
        // if (enemy != null) {
        //     unit.SetTarget(enemy);
        //     unit.GetStateMachine().ChangeState(AIState.Attack);
        // }
    }

    public override void Exit()
    {
        Debug.Log($"{unit.gameObject.name}: 防衛状態終了");
    }
}