using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Squad : MonoBehaviour
{
    [Header("班構成設定")]
    [SerializeField] private SquadData _squadData;
    [SerializeField] private Transform _patrolTarget;

    // 分離されたコンポーネント
    private SquadSpawner _spawner;
    private SquadFormation _formation;

    // ランタイム情報
    private List<UnitController> _spawnedUnits = new List<UnitController>();
    private UnitController _currentLeader;

    void Awake()
    {
        _spawner = GetComponent<SquadSpawner>();
        _formation = GetComponent<SquadFormation>();
    }

    void Start()
    {
        _spawner.Initialize(this, _squadData);
        _formation.Initialize(this);

        if (_spawner.AutoSpawnOnStart)
        {
            SpawnSquadMembers();
        }
    }

    void Update()
    {
        _formation.UpdateFormation(_currentLeader, GetAliveUnits());
    }

    // --- 基本的な班管理 ---

    public void SetLeader(UnitController newLeader)
    {
        if (!IsMember(newLeader)) return;

        if (_currentLeader != null)
        {
            _currentLeader.SetAsLeader(false);
        }

        _currentLeader = newLeader;
        _currentLeader.SetAsLeader(true);

        Debug.Log($"{GetSquadName()}: 新しい班長 - {_currentLeader.gameObject.name}");
    }

    public void MoveLeaderTo(Vector3 targetPosition)
    {
        if (_currentLeader == null) return;

        _currentLeader.MoveTo(targetPosition);
        _currentLeader.GetStateMachine().ChangeState(AIState.Move);

        foreach (var member in GetAliveUnits())
        {
            if (member != _currentLeader)
            {
                member.StartFollowingLeader();
            }
        }
    }

    public void StartPatrolMode()
    {
        if (_patrolTarget == null) return;

        foreach (var unit in GetAliveUnits())
        {
            unit.GetStateMachine().ChangeState(AIState.Patrol);
        }
    }

    public void StartDefendMode()
    {
        foreach (var unit in GetAliveUnits())
        {
            unit.GetStateMachine().ChangeState(AIState.Defend);
        }
    }

    // --- スポーン関連（Spawnerに委譲） ---

    public void SpawnSquadMembers()
    {
        _spawnedUnits = _spawner.SpawnMembers();
        if (_spawnedUnits.Count > 0)
        {
            SetLeader(_spawnedUnits[0]);
        }
    }

    public bool TryRespawnSquad()
    {
        if (!IsSquadEliminated()) return false;

        _spawnedUnits = _spawner.RespawnMembers();
        if (_spawnedUnits.Count > 0)
        {
            SetLeader(_spawnedUnits[0]);
        }
        return true;
    }

    public void ClearSpawnedUnits()
    {
        _spawner.ClearUnits(_spawnedUnits);
        _spawnedUnits.Clear();
        _currentLeader = null;
    }

    // --- ユーティリティ ---

    public bool IsSquadEliminated()
    {
        return GetAliveUnits().Count == 0;
    }

    public List<UnitController> GetAliveUnits()
    {
        return _spawnedUnits.Where(unit =>
            unit != null &&
            unit.gameObject.activeInHierarchy &&
            !unit.GetComponent<CombatManager>()?.IsDead == true
        ).ToList();
    }

    public bool IsMember(UnitController unit)
    {
        return _spawnedUnits.Contains(unit);
    }

    private string GetSquadName()
    {
        return _squadData != null ? _squadData.CompositionName : gameObject.name;
    }

    // プロパティ
    public List<UnitController> SpawnedUnits => _spawnedUnits;
    public UnitController CurrentLeader => _currentLeader;
    public Transform PatrolTarget => _patrolTarget;
    public Transform[] PatrolTargets => _patrolTarget != null ? new Transform[] { _patrolTarget } : new Transform[0];
}