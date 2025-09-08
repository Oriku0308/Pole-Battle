using UnityEngine;
using System.Collections.Generic;

public class SquadSpawner : MonoBehaviour
{
    [Header("スポーン設定")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _autoSpawnOnStart = true;

    private Squad _squad;
    private SquadData _squadData;
    private List<GameObject> _originalPrefabs = new List<GameObject>();

    public void Initialize(Squad squad, SquadData squadData)
    {
        _squad = squad;
        _squadData = squadData;
    }

    /// <summary>
    /// 班員をスポーン
    /// </summary>
    public List<UnitController> SpawnMembers()
    {
        if (_squadData == null)
        {
            Debug.LogError("SquadData が設定されていません");
            return new List<UnitController>();
        }

        // プレハブを記録（復活用）
        _originalPrefabs.Clear();
        _originalPrefabs.AddRange(_squadData.UnitPrefabs);

        Vector3 basePosition = _spawnPoint != null ? _spawnPoint.position : transform.position;
        basePosition += _squadData.SpawnOffset;

        List<UnitController> spawnedUnits = new List<UnitController>();

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

            unit.SetSquad(_squad);
            spawnedUnits.Add(unit);
        }

        Debug.Log($"班生成完了 - {spawnedUnits.Count}人");
        return spawnedUnits;
    }

    /// <summary>
    /// 班員を復活
    /// </summary>
    public List<UnitController> RespawnMembers()
    {
        if (_originalPrefabs.Count == 0)
        {
            Debug.LogError("復活用プレハブが記録されていません");
            return new List<UnitController>();
        }

        Vector3 basePosition = _spawnPoint != null ? _spawnPoint.position : transform.position;
        basePosition += _squadData.SpawnOffset;

        List<UnitController> respawnedUnits = new List<UnitController>();

        for (int i = 0; i < _originalPrefabs.Count; i++)
        {
            var prefab = _originalPrefabs[i];
            if (prefab == null) continue;

            Vector3 spawnPosition = GetSpawnPosition(basePosition, i);
            GameObject unitObj = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
            unitObj.name = $"{prefab.name}_{i}";

            UnitController unit = unitObj.GetComponent<UnitController>();
            if (unit != null)
            {
                unit.SetSquad(_squad);
                respawnedUnits.Add(unit);
            }
        }

        Debug.Log($"班復活完了 - {respawnedUnits.Count}人");
        return respawnedUnits;
    }

    /// <summary>
    /// ユニットをクリア
    /// </summary>
    public void ClearUnits(List<UnitController> units)
    {
        foreach (var unit in units)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }
        }
    }

    /// <summary>
    /// スポーン位置を計算
    /// </summary>
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

    // プロパティ
    public bool AutoSpawnOnStart => _autoSpawnOnStart;
}