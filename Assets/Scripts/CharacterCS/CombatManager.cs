using UnityEngine;

/// <summary>
/// 攻撃と敵検出の基本機能を提供するコンポーネント
/// ユニットの戦闘に関するすべての処理を管理
/// </summary>
public class CombatManager : MonoBehaviour
{
    [Header("攻撃設定")]
    [SerializeField] private float _attackStartRange = 2f;  // 攻撃開始範囲（狭い方）
    [SerializeField] private float _attackChaseRange = 10f; // 攻撃可能範囲（広い方）
    [SerializeField] private float _attackInterval = 1f;    // 攻撃間隔
    [SerializeField] private float _attackDamage = 20f;     // 攻撃力

    [Header("移動設定")]
    [SerializeField] private float _normalSpeed = 5f;       // 通常移動速度
    [SerializeField] private float _combatSpeed = 3f;       // 戦闘時移動速度

    [Header("HP設定")]
    [SerializeField] private float _maxHP = 100f;           // 最大HP
    [SerializeField] private float _currentHP;              // 現在HP

    // 内部状態
    private bool _isDead = false;                           // 死亡フラグ
    private Transform _currentTarget;                       // 現在のターゲット
    private float _lastAttackTime = 0f;                     // 最後の攻撃時刻

    // コンポーネント参照
    private UnitController _unitController;
    private UnityEngine.AI.NavMeshAgent _agent;

    void Awake()
    {
        // コンポーネント取得と初期化
        _unitController = GetComponent<UnitController>();
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _currentHP = _maxHP;

        // 移動速度を通常速度に設定
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

        // 範囲内のすべてのコライダーをチェック
        foreach (var collider in nearbyColliders)
        {
            if (collider.transform == transform) continue; // 自分自身は除外

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

        // ダメージ処理
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
        if (Time.time - _lastAttackTime < _attackInterval) return false; // クールダウン中

        return IsTargetInAttackStartRange(target);
    }

    // --- ダメージ・HP管理 ---

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHP -= damage;
        _currentHP = Mathf.Max(0, _currentHP); // 0以下にならないよう制限

        Debug.Log($"{gameObject.name}: {damage}ダメージ受ける (HP: {_currentHP}/{_maxHP})");

        // HP0なら死亡処理
        if (_currentHP <= 0)
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

        // 班に死亡を通知
        if (_unitController != null)
        {
            SquadManager squad = _unitController.transform.parent?.GetComponent<SquadManager>();
            squad?.OnUnitDied(_unitController);

            // 移動とAIを停止
            _unitController.StopMoving();
            _unitController.GetStateManager().enabled = false;
        }

        // 見た目を変更
        GetComponent<Renderer>().material.color = Color.gray;
        GetComponent<Collider>().enabled = false;

        Debug.Log($"{gameObject.name}: 死亡");
        Destroy(gameObject, 3f); // 3秒後に削除
    }

    // --- 速度制御 ---

    /// <summary>
    /// 戦闘速度に変更（攻撃時の速度低下）
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

    // --- プロパティ ---
    public Transform CurrentTarget { get => _currentTarget; set => _currentTarget = value; }
    public bool IsDead => _isDead;
    public float AttackStartRange => _attackStartRange;
    public float AttackChaseRange => _attackChaseRange;
}