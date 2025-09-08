using UnityEngine;

// AttackState.cs - CombatManager対応版
public class AttackState : AIStateBase
{
    private CombatManager combat;
    private Transform currentTarget;
    private AIState previousState;
    private bool isChasing = false;

    public AttackState(UnitController unit) : base(unit)
    {
        combat = unit.GetComponent<CombatManager>();
    }

    public override void Enter()
    {
        Debug.Log($"{unit.gameObject.name}: 攻撃状態開始");

        RememberPreviousState();

        currentTarget = combat?.CurrentTarget;

        if (currentTarget == null)
        {
            currentTarget = combat?.FindEnemyInAttackStartRange();
            if (combat != null) combat.CurrentTarget = currentTarget;
        }

        if (currentTarget != null)
        {
            Debug.Log($"{unit.gameObject.name}: ターゲット設定 - {currentTarget.name}");
        }
        else
        {
            ReturnToPreviousState();
            return;
        }

        unit.SetColor(Color.red);
        isChasing = false;
    }

    public override void Update()
    {
        if (combat == null || combat.IsDead) return;

        if (!IsTargetValid(currentTarget))
        {
            ReturnToPreviousState();
            return;
        }

        float distanceToTarget = Vector3.Distance(unit.Position, currentTarget.position);

        if (distanceToTarget > combat.AttackChaseRange)
        {
            Debug.Log($"{unit.gameObject.name}: ターゲットが攻撃可能範囲外に逃走");
            ReturnToPreviousState();
            return;
        }

        if (distanceToTarget <= combat.AttackStartRange)
        {
            HandleCloseRangeAttack();
        }
        else
        {
            HandleChaseAndAttack();
        }
    }

    public override void Exit()
    {
        Debug.Log($"{unit.gameObject.name}: 攻撃状態終了");

        combat?.SetNormalSpeed();

        if (combat != null) combat.CurrentTarget = null;
        currentTarget = null;
        isChasing = false;
    }

    private void HandleCloseRangeAttack()
    {
        if (isChasing)
        {
            unit.StopMoving();
            combat.SetCombatSpeed();
            isChasing = false;
            Debug.Log($"{unit.gameObject.name}: 追跡停止、戦闘速度に変更");
        }

        Vector3 directionToTarget = (currentTarget.position - unit.Position).normalized;
        if (directionToTarget != Vector3.zero)
        {
            unit.transform.rotation = Quaternion.LookRotation(directionToTarget);
        }

        combat.TryAttack(currentTarget);
    }

    private void HandleChaseAndAttack()
    {
        if (!isChasing)
        {
            combat.SetNormalSpeed();
            isChasing = true;
            Debug.Log($"{unit.gameObject.name}: 追跡開始");
        }

        unit.MoveTo(currentTarget.position);
    }

    private void RememberPreviousState()
    {
        AIState currentAIState = unit.GetStateMachine().GetCurrentState();

        if (currentAIState != AIState.Attack)
        {
            previousState = currentAIState;
        }
        else
        {
            previousState = AIState.Defend;
        }

        Debug.Log($"{unit.gameObject.name}: 前の状態を記憶 - {previousState}");
    }

    private void ReturnToPreviousState()
    {
        Debug.Log($"{unit.gameObject.name}: 前の状態に復帰 - {previousState}");
        unit.GetStateMachine().ChangeState(previousState);
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null) return false;

        CombatManager targetCombat = target.GetComponent<CombatManager>();
        if (targetCombat == null || targetCombat.IsDead) return false;

        return true;
    }
}