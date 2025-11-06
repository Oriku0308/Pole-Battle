using UnityEngine;

/// <summary>
/// 攻撃と敵検出の基本機能を提供するコンポーネント
/// ユニットの戦闘に関するすべての処理を管理
/// </summary>
public class CombatManager : MonoBehaviour
{
    private const float DEATH_DELAY = 3f;
    private const float MIN_HP = 0f;

    [Header("攻撃設定")]
    [SerializeField] private float _attackStartRange = 2f;
    [SerializeField] private float _attackChaseRange = 10f;
    [SerializeField] private float _attackInterval = 1f;
    [SerializeField] private float _attackDamage = 20f;

    [Header("移動設定")]
    [SerializeField] private float _normalSpeed = 5f;
    [SerializeField] private float _combatSpeed = 3f;

    [Header("HP設定")]
    [SerializeField] private float _maxHP = 100f;
    [SerializeField] private float _currentHP;

    // 内部状態
    private bool _isDead = false;
    private Transform _currentTarget;
    private float _lastAttackTime = 0f;

    // コンポーネント参照
    private UnitController _unitController;
    private UnityEngine.AI.NavMeshAgent _agent;
    private Renderer _renderer;
    private Collider _collider;

    void Awake()
    {
        // コンポーネント取得
        _unitController = GetComponent<UnitController>();
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();

        // 初期化
        _currentHP = _maxHP;

        if (_agent != null)
        {
            _agent.speed = _normalSpeed;
        }
    }

    // --- 敵検出システム ---

    /// <summary>
    /// 攻撃開始範囲内の敵を検出
    /// </summary>
    public Transform FindEnemyInAttackStartRange()
    {
        return FindNearestEnemyInRange(_attackStartRange);
    }

    /// <summary>
    /// 指定範囲内の最も近い敵を検索
    /// </summary>
    private Transform FindNearestEnemyInRange(float range)
    {
        if (_isDead) return null;

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, range);
        Transform nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (var collider in nearbyColliders)
        {
            if (collider.transform == transform) continue;

            UnitController otherUnit = collider.GetComponent<UnitController>();
            if (otherUnit == null || !IsEnemy(otherUnit)) continue;

            if (otherUnit.Combat == null || otherUnit.Combat.IsDead) continue;

            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = collider.transform;
            }
        }

        return nearestEnemy;
    }

    /// <summary>
    /// 敵かどうかの判定
    /// </summary>
    private bool IsEnemy(UnitController otherUnit)
    {
        if (_unitController == null) return false;

        Transform mySquad = _unitController.transform.parent;
        Transform otherSquad = otherUnit.transform.parent;

        return mySquad != otherSquad;
    }

    // --- 攻撃システム ---

    /// <summary>
    /// ターゲットが攻撃開始範囲内にいるかチェック
    /// </summary>
    public bool IsTargetInAttackStartRange(Transform target)
    {
        if (target == null) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= _attackStartRange;
    }

    /// <summary>
    /// 攻撃実行
    /// </summary>
    public bool TryAttack(Transform target)
    {
        if (!CanAttack(target)) return false;

        _lastAttackTime = Time.time;

        CombatManager targetCombat = target.GetComponent<CombatManager>();
        if (targetCombat != null)
        {
            targetCombat.TakeDamage(_attackDamage);
        }

        Debug.Log($"{gameObject.name}: {target.name}を攻撃！");
        return true;
    }

    /// <summary>
    /// 攻撃可能かチェック
    /// </summary>
    private bool CanAttack(Transform target)
    {
        if (_isDead || target == null) return false;
        if (IsOnCooldown()) return false;
        return IsTargetInAttackStartRange(target);
    }

    /// <summary>
    /// クールダウン中かチェック
    /// </summary>
    private bool IsOnCooldown()
    {
        return Time.time - _lastAttackTime < _attackInterval;
    }

    // --- ダメージ・HP管理 ---

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHP -= damage;
        _currentHP = Mathf.Max(MIN_HP, _currentHP);

        Debug.Log($"{gameObject.name}: {damage}ダメージ受ける (HP: {_currentHP}/{_maxHP})");

        if (_currentHP <= MIN_HP)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    private void Die()
    {
        _isDead = true;

        if (_unitController != null)
        {
            SquadManager squad = _unitController.transform.parent?.GetComponent<SquadManager>();
            squad?.OnUnitDied(_unitController);

            _unitController.StopMoving();
            _unitController.StateManager.enabled = false;
        }

        if (_renderer != null)
        {
            _renderer.material.color = Color.gray;
        }

        if (_collider != null)
        {
            _collider.enabled = false;
        }

        Debug.Log($"{gameObject.name}: 死亡");
        Destroy(gameObject, DEATH_DELAY);
    }

    // --- 速度制御 ---

    public void SetCombatSpeed() => SetAgentSpeed(_combatSpeed);
    public void SetNormalSpeed() => SetAgentSpeed(_normalSpeed);

    private void SetAgentSpeed(float speed)
    {
        if (_agent != null)
        {
            _agent.speed = speed;
        }
    }

    // --- プロパティ ---
    public Transform CurrentTarget { get => _currentTarget; set => _currentTarget = value; }
    public bool IsDead => _isDead;
    public float AttackStartRange => _attackStartRange;
    public float AttackChaseRange => _attackChaseRange;
}