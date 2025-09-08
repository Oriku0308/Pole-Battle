using UnityEngine;

/// <summary>
/// 攻撃と敵検出の基本機能（モック用簡素版）
/// </summary>
public class CombatManager : MonoBehaviour
{
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

    // ランタイム情報
    private float _currentHP;
    private bool _isDead = false;
    private Transform _currentTarget;
    private float _lastAttackTime = 0f;

    private UnitController _unitController;
    private UnityEngine.AI.NavMeshAgent _agent;

    void Awake()
    {
        _unitController = GetComponent<UnitController>();
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _currentHP = _maxHP;

        if (_agent != null)
        {
            _agent.speed = _normalSpeed;
        }
    }

    /// <summary>
    /// 初期状態にリセット（モック用）
    /// </summary>
    public void ResetToInitialState()
    {
        _currentHP = _maxHP;
        _isDead = false;
        _currentTarget = null;
        _lastAttackTime = 0f;

        if (_agent != null)
        {
            _agent.speed = _normalSpeed;
            _agent.enabled = true;
        }

        // コライダー有効化
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        // 色をリセット
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.white; // デフォルト色
        }

        Debug.Log($"{gameObject.name}: CombatManager初期状態リセット");
    }

    /// <summary>
    /// 攻撃開始範囲内の敵を検出
    /// </summary>
    public Transform FindEnemyInAttackStartRange()
    {
        return FindNearestEnemyInRange(_attackStartRange);
    }

    /// <summary>
    /// 攻撃可能範囲内の敵を検出
    /// </summary>
    public Transform FindEnemyInChaseRange()
    {
        return FindNearestEnemyInRange(_attackChaseRange);
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
            if (otherUnit != null && IsEnemy(otherUnit))
            {
                CombatManager enemyCombat = collider.GetComponent<CombatManager>();
                if (enemyCombat != null && !enemyCombat.IsDead)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = collider.transform;
                    }
                }
            }
        }

        return nearestEnemy;
    }

    /// <summary>
    /// 敵判定（親が違えば敵）
    /// </summary>
    private bool IsEnemy(UnitController otherUnit)
    {
        if (_unitController == null) return false;
        return _unitController.transform.parent != otherUnit.transform.parent;
    }

    /// <summary>
    /// 攻撃開始範囲内かチェック
    /// </summary>
    public bool IsTargetInAttackStartRange(Transform target)
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.position) <= _attackStartRange;
    }

    /// <summary>
    /// 攻撃可能範囲内かチェック
    /// </summary>
    public bool IsTargetInChaseRange(Transform target)
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.position) <= _attackChaseRange;
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
        if (Time.time - _lastAttackTime < _attackInterval) return false;
        return IsTargetInAttackStartRange(target);
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHP -= damage;
        _currentHP = Mathf.Max(0, _currentHP);

        Debug.Log($"{gameObject.name}: {damage}ダメージ (HP: {_currentHP}/{_maxHP})");

        if (_currentHP <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 戦闘速度に変更
    /// </summary>
    public void SetCombatSpeed()
    {
        if (_agent != null) _agent.speed = _combatSpeed;
    }

    /// <summary>
    /// 通常速度に変更
    /// </summary>
    public void SetNormalSpeed()
    {
        if (_agent != null) _agent.speed = _normalSpeed;
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    private void Die()
    {
        _isDead = true;

        if (_unitController != null)
        {
            _unitController.StopMoving();
            _unitController.GetStateMachine().enabled = false;
        }

        GetComponent<Renderer>().material.color = Color.gray;
        GetComponent<Collider>().enabled = false;

        Debug.Log($"{gameObject.name}: 死亡");
        Destroy(gameObject, 3f);
    }

    // ギズモ表示
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackStartRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _attackChaseRange);
    }

    // プロパティ
    public Transform CurrentTarget { get => _currentTarget; set => _currentTarget = value; }
    public bool IsDead => _isDead;
    public float CurrentHP => _currentHP;
    public float MaxHP => _maxHP;
    public float AttackStartRange => _attackStartRange;
    public float AttackChaseRange => _attackChaseRange;
}