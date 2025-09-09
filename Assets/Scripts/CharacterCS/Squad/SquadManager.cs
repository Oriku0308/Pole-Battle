using UnityEngine;
using System.Collections.Generic;

public class SquadManager : MonoBehaviour
{
    [Header("班構成設定")]
    [SerializeField] private SquadData _squadData;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _autoSpawnOnStart = true;

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

    // 班員を生成
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

    // プロパティ
    public List<UnitController> SpawnedUnits => _spawnedUnits;
    public UnitController CurrentLeader => _currentLeader;
    public Transform Transform => transform;
}