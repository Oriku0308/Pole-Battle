using UnityEngine;
using System.Collections.Generic;

public class Squad : MonoBehaviour
{
    [Header("班構成設定")]
    [SerializeField] private SquadData _squadData;

    [Header("スポーン設定")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _autoSpawnOnStart = true;

    [Header("追従設定")]
    [SerializeField] private float _followUpdateInterval = 0.3f;
    [SerializeField] private float _leaderDistance = 2f;
    [SerializeField] private float _memberAvoidanceDistance = 1.5f;

    [Header("ランタイム情報")]
    [SerializeField] private List<UnitController> _spawnedUnits = new List<UnitController>();

    private UnitController _currentLeader;
    private float _lastFollowUpdateTime = 0f;

    // イベント通知用
    public System.Action<UnitController> OnLeaderChanged;
    public System.Action<Vector3> OnSquadMoved;

    void Start()
    {
        if (_autoSpawnOnStart && _squadData != null)
        {
            SpawnSquadMembers();
        }
    }

    void Update()
    {
        UpdateFollowBehavior();
    }

    // --- 公開メソッド（Presenterから呼ばれる） ---

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

        // イベント通知
        OnLeaderChanged?.Invoke(_currentLeader);
    }

    public void MoveLeaderTo(Vector3 targetPosition)
    {
        if (_currentLeader == null)
        {
            Debug.LogWarning($"{GetSquadName()}: 班長が設定されていません");
            return;
        }

        Debug.Log($"{GetSquadName()}: 班長移動指示 - {targetPosition}");

        StartCoroutine(ExecuteMoveCommand(targetPosition));
    }

    // --- 内部処理メソッド ---

    private System.Collections.IEnumerator ExecuteMoveCommand(Vector3 targetPosition)
    {
        _currentLeader.StopMoving();
        yield return null;

        _currentLeader.MoveTo(targetPosition);
        _currentLeader.GetStateMachine().ChangeState(AIState.Move);

        foreach (var member in _spawnedUnits)
        {
            if (member != _currentLeader)
            {
                member.StartFollowingLeader();
            }
        }

        // イベント通知
        OnSquadMoved?.Invoke(targetPosition);
    }

    private void UpdateFollowBehavior()
    {
        if (_currentLeader == null) return;

        if (Time.time - _lastFollowUpdateTime > _followUpdateInterval)
        {
            foreach (var member in _spawnedUnits)
            {
                if (member != _currentLeader && member.IsFollowingLeader)
                {
                    UpdateMemberFollow(member);
                }
            }
            _lastFollowUpdateTime = Time.time;
        }
    }

    private void UpdateMemberFollow(UnitController member)
    {
        if (_currentLeader.IsMoving())
        {
            Vector3 followPosition = GetNaturalFollowPosition(member, _currentLeader.Position);
            member.MoveTo(followPosition);

            if (member.GetStateMachine().GetCurrentState() != AIState.Move)
            {
                member.GetStateMachine().ChangeState(AIState.Move);
            }
        }
        else
        {
            HandleMemberWhenLeaderStopped(member);
        }
    }

    private void HandleMemberWhenLeaderStopped(UnitController member)
    {
        Vector3 idealPosition = GetNaturalFollowPosition(member, _currentLeader.Position);
        float distanceToIdeal = Vector3.Distance(member.Position, idealPosition);

        if (distanceToIdeal > 0.8f)
        {
            member.MoveTo(idealPosition);

            if (member.GetStateMachine().GetCurrentState() != AIState.Move)
            {
                member.GetStateMachine().ChangeState(AIState.Move);
            }
        }
        else
        {
            member.StopFollowing();
        }
    }

    private Vector3 GetNaturalFollowPosition(UnitController member, Vector3 leaderPosition)
    {
        Vector3 baseDirection = GetBaseDirectionFromLeader(member, leaderPosition);
        Vector3 targetPosition = leaderPosition + baseDirection * _leaderDistance;
        targetPosition = AvoidOtherMembers(member, targetPosition, leaderPosition);
        targetPosition.y = member.Position.y;
        return targetPosition;
    }

    private Vector3 GetBaseDirectionFromLeader(UnitController member, Vector3 leaderPosition)
    {
        Vector3 currentDirection = (member.Position - leaderPosition).normalized;

        if (currentDirection.magnitude < 0.1f)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            currentDirection = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle));
        }

        return currentDirection;
    }

    private Vector3 AvoidOtherMembers(UnitController member, Vector3 proposedPosition, Vector3 leaderPosition)
    {
        Vector3 finalPosition = proposedPosition;

        foreach (var otherMember in _spawnedUnits)
        {
            if (otherMember == member || otherMember == _currentLeader) continue;

            float distanceToMember = Vector3.Distance(finalPosition, otherMember.Position);

            if (distanceToMember < _memberAvoidanceDistance)
            {
                Vector3 avoidDirection = (finalPosition - otherMember.Position).normalized;
                finalPosition = otherMember.Position + avoidDirection * _memberAvoidanceDistance;

                float distanceToLeader = Vector3.Distance(finalPosition, leaderPosition);
                if (distanceToLeader > _leaderDistance * 1.5f)
                {
                    Vector3 toLeader = (leaderPosition - finalPosition).normalized;
                    finalPosition = leaderPosition - toLeader * (_leaderDistance * 1.2f);
                }
            }
        }

        return finalPosition;
    }

    // --- ユーティリティメソッド ---

    [ContextMenu("Spawn Squad Members")]
    public void SpawnSquadMembers()
    {
        if (_squadData == null)
        {
            Debug.LogError($"{gameObject.name}: SquadData が設定されていません");
            return;
        }

        ClearSpawnedUnits();

        Vector3 basePosition = _spawnPoint != null ? _spawnPoint.position : transform.position;
        basePosition += _squadData.SpawnOffset;

        for (int i = 0; i < _squadData.UnitPrefabs.Count; i++)
        {
            var prefab = _squadData.UnitPrefabs[i];
            if (prefab == null) continue;

            Vector3 spawnPosition = GetSpawnPosition(basePosition, i);
            GameObject unitObj = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
            unitObj.name = $"{prefab.name}_{i}";

            UnitController unit = unitObj.GetComponent<UnitController>();
            if (unit == null)
            {
                unit = unitObj.AddComponent<UnitController>();
            }

            unit.SetSquad(this);
            _spawnedUnits.Add(unit);
        }

        Debug.Log($"{_squadData.CompositionName}: 班生成完了 - {_spawnedUnits.Count}人");
    }

    private Vector3 GetSpawnPosition(Vector3 basePosition, int index)
    {
        if (index == 0) return basePosition;

        float angle = (index - 1) * (360f / _squadData.TotalUnits) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * _squadData.FormationRadius,
            0,
            Mathf.Sin(angle) * _squadData.FormationRadius
        );

        return basePosition + offset;
    }

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
    public float FollowDistance => _squadData != null ? _squadData.FormationRadius : 3f;
}