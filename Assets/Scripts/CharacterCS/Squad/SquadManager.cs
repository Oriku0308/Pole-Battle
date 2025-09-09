using UnityEngine;
using System.Collections.Generic;

public class SquadManager : MonoBehaviour
{
    [Header("班構成設定")]
    [SerializeField] private SquadData _squadData;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _autoSpawnOnStart = true;

    [Header("自動移動設定")]
    [SerializeField] private Transform _patrolDestination; // パトロール目的地

    private SquadSpawner _spawner;
    private SquadMovementController _movementController;
    private List<UnitController> _spawnedUnits = new List<UnitController>();
    private UnitController _currentLeader;

    // イベント通知用
    public System.Action<UnitController> OnLeaderChanged;
    public System.Action<Vector3> OnSquadMoved;

    void Awake()
    {
        // 各コンポーネントを初期化
        _spawner = new SquadSpawner(this, _squadData);
        _movementController = new SquadMovementController(this);
    }

    void Start()
    {
        // 自動生成が有効なら班員を生成
        if (_autoSpawnOnStart && _squadData != null)
        {
            SpawnSquadMembers();
        }
    }

    void Update()
    {
        // 班員の追従行動を更新
        _movementController.UpdateFollowBehavior();
    }

    // --- 公開メソッド ---

    // 新しい班長を設定
    public void SetLeader(UnitController newLeader)
    {
        if (!IsMember(newLeader))
        {
            Debug.LogWarning($"{newLeader.gameObject.name}: この班のメンバーではありません");
            return;
        }

        if (_currentLeader != null)
        {
            _currentLeader.SetAsLeader(false);
        }

        _currentLeader = newLeader;
        _currentLeader.SetAsLeader(true);

        Debug.Log($"{GetSquadName()}: 新しい班長 - {_currentLeader.gameObject.name}");
        OnLeaderChanged?.Invoke(_currentLeader);
    }

    // 班長を指定位置に移動
    public void MoveLeaderTo(Vector3 targetPosition)
    {
        _movementController.MoveLeaderTo(targetPosition);
    }

    // 班全体を目的地に移動
    public void MoveSquadToDestination()
    {
        _movementController.MoveSquadToDestination();
    }

    // 目的地を設定（Transform） - 修正版
    public void SetDestination(Transform destination)
    {
        _movementController.SetDestination(destination);
    }

    // 目的地を設定（座標） - 修正版
    public void SetDestination(Vector3 destinationPosition)
    {
        _movementController.SetDestination(destinationPosition);
    }

    // --- 新機能：自動移動（パトロール）システム ---

    /// <summary>
    /// パトロールモード開始
    /// 設定された目的地に向かって班全体で移動
    /// </summary>
    public void StartPatrolMode()
    {
        if (_patrolDestination == null)
        {
            Debug.LogWarning($"{GetSquadName()}: パトロール目的地が設定されていません");
            return;
        }

        Debug.Log($"{GetSquadName()}: パトロールモード開始 → {_patrolDestination.name}");

        // 班全体をパトロール状態に設定
        foreach (var unit in _spawnedUnits)
        {
            if (unit != null && !unit.GetComponent<CombatManager>().IsDead)
            {
                // 直接目的地に移動指示（PatrolStateが存在しない場合の対応）
                unit.MoveTo(_patrolDestination.position);

                // AI状態をパトロールに変更
                unit.GetStateMachine().ChangeState(AIState.Patrol);
            }
        }
    }

    /// <summary>
    /// 防衛モード開始
    /// 班全体を現在位置で防衛状態に
    /// </summary>
    public void StartDefendMode()
    {
        Debug.Log($"{GetSquadName()}: 防衛モード開始");

        // 班全体を防衛状態に設定
        foreach (var unit in _spawnedUnits)
        {
            if (unit != null && !unit.GetComponent<CombatManager>().IsDead)
            {
                unit.StopMoving(); // 移動を停止
                unit.GetStateMachine().ChangeState(AIState.Defend);
            }
        }
    }

    /// <summary>
    /// パトロール目的地を動的に設定
    /// </summary>
    public void SetPatrolDestination(Transform destination)
    {
        _patrolDestination = destination;
        Debug.Log($"{GetSquadName()}: パトロール目的地を設定 - {destination.name}");
    }

    /// <summary>
    /// パトロール目的地を座標で設定
    /// </summary>
    public void SetPatrolDestination(Vector3 position)
    {
        GameObject tempDestination = new GameObject($"{GetSquadName()}_PatrolDestination");
        tempDestination.transform.position = position;
        _patrolDestination = tempDestination.transform;

        Debug.Log($"{GetSquadName()}: パトロール目的地を設定 - {position}");
    }

    // 班員を生成
    [ContextMenu("Spawn Squad Members")]
    public void SpawnSquadMembers()
    {
        _spawner.SpawnSquadMembers(_spawnPoint != null ? _spawnPoint.position : transform.position);
    }

    // ユニット死亡時の処理
    public void OnUnitDied(UnitController deadUnit)
    {
        if (!IsMember(deadUnit)) return;

        Debug.Log($"{GetSquadName()}: {deadUnit.gameObject.name}が死亡しました");
        _spawnedUnits.Remove(deadUnit);

        // 死亡したユニットが班長なら新しい班長を選出
        if (_currentLeader == deadUnit)
        {
            _currentLeader = null;
            SelectNewLeader();
        }

        // 全員死亡したら全滅処理
        if (_spawnedUnits.Count == 0)
        {
            OnSquadEliminated();
        }
    }

    // 新しい班長を自動選出
    private void SelectNewLeader()
    {
        if (_spawnedUnits.Count == 0)
        {
            Debug.Log($"{GetSquadName()}: 生存者がいないため班長を選出できません");
            return;
        }

        UnitController newLeader = _spawnedUnits[0];
        SetLeader(newLeader);
        Debug.Log($"{GetSquadName()}: 新しい班長に {newLeader.gameObject.name} を選出");
    }

    // 班全滅時の処理
    private void OnSquadEliminated()
    {
        Debug.Log($"{GetSquadName()}: 班が全滅しました");
    }

    // 生成した班員をクリア
    public void ClearSpawnedUnits()
    {
        foreach (var unit in _spawnedUnits)
        {
            if (unit != null)
            {
                DestroyImmediate(unit.gameObject);
            }
        }
        _spawnedUnits.Clear();
        _currentLeader = null;
    }

    public bool IsMember(UnitController unit) => _spawnedUnits.Contains(unit);
    public string GetSquadName() => _squadData != null ? _squadData.CompositionName : gameObject.name;
    public int GetAliveCount() => _spawnedUnits.Count;
    public bool IsSquadFunctional() => _spawnedUnits.Count > 0 && _currentLeader != null;

    // プロパティ
    public List<UnitController> SpawnedUnits => _spawnedUnits;
    public UnitController CurrentLeader => _currentLeader;
    public SquadData SquadData => _squadData;
    public Transform Transform => transform;
    public Transform PatrolDestination => _patrolDestination; // 新規追加
}