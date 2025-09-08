using UnityEngine;

public class AttackState : AIStateBase
{
    private CombatManager combat;
    private Transform currentTarget;
    private AIState previousState; // 前の状態を記憶
    private bool isChasing = false; // 追跡中かどうか

    public AttackState(UnitController unit) : base(unit)
    {
        combat = unit.GetComponent<CombatManager>();
    }

    public override void Enter()
    {
        Debug.Log($"{unit.gameObject.name}: 攻撃状態開始");

        // 前の状態を記憶（攻撃終了時の復帰先）
        RememberPreviousState();

        // 現在のターゲットを取得
        currentTarget = combat?.CurrentTarget;

        if (currentTarget == null)
        {
            // ターゲットがない場合は敵を検索
            currentTarget = combat?.FindEnemyInAttackStartRange();
            if (combat != null) combat.CurrentTarget = currentTarget;
        }

        if (currentTarget != null)
        {
            Debug.Log($"{unit.gameObject.name}: ターゲット設定 - {currentTarget.name}");
        }
        else
        {
            // ターゲットが見つからない場合は前の状態に戻る
            ReturnToPreviousState();
            return;
        }

        // 攻撃状態の色に変更
        unit.SetColor(Color.red);
        isChasing = false;
    }

    public override void Update()
    {
        if (combat == null || combat.IsDead) return;

        // ターゲットの有効性チェック
        if (!IsTargetValid(currentTarget))
        {
            ReturnToPreviousState();
            return;
        }

        // ターゲットとの距離をチェック
        float distanceToTarget = Vector3.Distance(unit.Position, currentTarget.position);

        // 攻撃可能範囲外に出た場合
        if (distanceToTarget > combat.AttackChaseRange)
        {
            Debug.Log($"{unit.gameObject.name}: ターゲットが攻撃可能範囲外に逃走");
            ReturnToPreviousState();
            return;
        }

        // 攻撃開始範囲内の場合
        if (distanceToTarget <= combat.AttackStartRange)
        {
            HandleCloseRangeAttack();
        }
        // 攻撃可能範囲内だが攻撃開始範囲外の場合
        else
        {
            HandleChaseAndAttack();
        }
    }

    public override void Exit()
    {
        Debug.Log($"{unit.gameObject.name}: 攻撃状態終了");

        // 通常速度に戻す
        combat?.SetNormalSpeed();

        // ターゲットをクリア
        if (combat != null) combat.CurrentTarget = null;
        currentTarget = null;
        isChasing = false;
    }

    /// <summary>
    /// 近距離攻撃処理（攻撃開始範囲内）
    /// </summary>
    private void HandleCloseRangeAttack()
    {
        // 追跡を停止し、移動速度を遅くする
        if (isChasing)
        {
            unit.StopMoving();
            combat.SetCombatSpeed();
            isChasing = false;
            Debug.Log($"{unit.gameObject.name}: 追跡停止、戦闘速度に変更");
        }

        // ターゲットの方向を向く
        Vector3 directionToTarget = (currentTarget.position - unit.Position).normalized;
        if (directionToTarget != Vector3.zero)
        {
            unit.transform.rotation = Quaternion.LookRotation(directionToTarget);
        }

        // 攻撃実行
        combat.TryAttack(currentTarget);
    }

    /// <summary>
    /// 追跡攻撃処理（攻撃可能範囲内、攻撃開始範囲外）
    /// </summary>
    private void HandleChaseAndAttack()
    {
        // 追跡開始
        if (!isChasing)
        {
            combat.SetNormalSpeed(); // 追跡時は通常速度
            isChasing = true;
            Debug.Log($"{unit.gameObject.name}: 追跡開始");
        }

        // ターゲットに向かって移動
        unit.MoveTo(currentTarget.position);
    }

    /// <summary>
    /// 前の状態を記憶
    /// </summary>
    private void RememberPreviousState()
    {
        AIState currentAIState = unit.GetStateMachine().GetCurrentState();

        // 現在の状態が攻撃状態でない場合のみ記憶
        if (currentAIState != AIState.Attack)
        {
            previousState = currentAIState;
        }
        else
        {
            // 既に攻撃状態の場合はデフォルトで防衛状態に設定
            previousState = AIState.Defend;
        }

        Debug.Log($"{unit.gameObject.name}: 前の状態を記憶 - {previousState}");
    }

    /// <summary>
    /// 前の状態に戻る
    /// </summary>
    private void ReturnToPreviousState()
    {
        Debug.Log($"{unit.gameObject.name}: 前の状態に復帰 - {previousState}");
        unit.GetStateMachine().ChangeState(previousState);
    }

    /// <summary>
    /// ターゲットが有効かチェック
    /// </summary>
    private bool IsTargetValid(Transform target)
    {
        if (target == null) return false;

        CombatManager targetCombat = target.GetComponent<CombatManager>();
        if (targetCombat == null || targetCombat.IsDead) return false;

        return true;
    }
}