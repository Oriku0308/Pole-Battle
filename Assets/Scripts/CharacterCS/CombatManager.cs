using UnityEngine;

/// <summary>
/// 攻撃と敵検出の基本機能を提供するコンポーネント
/// </summary>
public class CombatManager : MonoBehaviour
{
    [Header("攻撃設定")]
    [SerializeField] private float _attackStartRange = 2f;      // 攻撃開始範囲（狭い方）
    [SerializeField] private float _attackChaseRange = 10f;     // 攻撃可能範囲（広い方）
    [SerializeField] private float _attackInterval = 1f;       // 攻撃間隔
    [SerializeField] private float _attackDamage = 20f;        // 攻撃力

    [Header("移動設定")]
    [SerializeField] private float _normalSpeed = 5f;          // 通常移動速度
    [SerializeField] private float _combatSpeed = 3f;          // 戦闘時移動速度

    [Header("HP設定")]
    [SerializeField] private float _maxHP = 100f;              // 最大HP
    [SerializeField] private float _currentHP;                // 現在HP

    private bool _isDead = false;
    private Transform _currentTarget;        // 現在のターゲット

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
    /// 敵かどうかの判定（簡易版）
    /// </summary>
    private bool IsEnemy(UnitController otherUnit)
    {
        if (_unitController == null) return false;

        // 簡易判定：親オブジェクト（Squad）が異なれば敵
        Transform mySquad = _unitController.transform.parent;
        Transform otherSquad = otherUnit.transform.parent;

        return mySquad != otherSquad;
    }

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
    /// ターゲットが攻撃可能範囲内にいるかチェック
    /// </summary>
    public bool IsTargetInChaseRange(Transform target)
    {
        if (target == null) return false;
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= _attackChaseRange;
    }

    /// <summary>
    /// 攻撃実行
    /// </summary>
    public bool TryAttack(Transform target)
    {
        if (!CanAttack(target)) return false;

        _lastAttackTime = Time.time;

        // ダメージ処理
        CombatManager targetCombat = target.GetComponent<CombatManager>();
        if (targetCombat != null)
        {
            targetCombat.TakeDamage(_attackDamage);
        }

        // 攻撃エフェクト
        StartCoroutine(AttackFlashEffect());

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

        StartCoroutine(DamageFlashEffect());

        Debug.Log($"{gameObject.name}: {damage}ダメージ受ける (HP: {_currentHP}/{_maxHP})");

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
        if (_agent != null)
        {
            _agent.speed = _combatSpeed;
        }
    }

    /// <summary>
    /// 通常速度に変更
    /// </summary>
    public void SetNormalSpeed()
    {
        if (_agent != null)
        {
            _agent.speed = _normalSpeed;
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
            _unitController.StopMoving();
            _unitController.GetStateMachine().enabled = false;
        }

        GetComponent<Renderer>().material.color = Color.gray;
        GetComponent<Collider>().enabled = false;

        Debug.Log($"{gameObject.name}: 死亡");
        Destroy(gameObject, 3f);
    }

    /// <summary>
    /// 攻撃エフェクト
    /// </summary>
    private System.Collections.IEnumerator AttackFlashEffect()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// ダメージエフェクト
    /// </summary>
    private System.Collections.IEnumerator DamageFlashEffect()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = originalColor;
        }
    }

    // ギズモ表示
    void OnDrawGizmosSelected()
    {
        // 攻撃開始範囲（赤）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackStartRange);

        // 攻撃可能範囲（黄）
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