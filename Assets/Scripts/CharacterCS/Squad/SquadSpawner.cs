using UnityEngine;

public class SquadSpawner
{
    private SquadManager _squadManager;
    private SquadData _squadData;

    public SquadSpawner(SquadManager squadManager, SquadData squadData)
    {
        _squadManager = squadManager;
        _squadData = squadData;
    }

    /// <summary>
    /// 班員を生成
    /// </summary>
    public void SpawnSquadMembers(Vector3 basePosition)
    {
        if (_squadData == null)
        {
            Debug.LogError($"{_squadManager.gameObject.name}: SquadData が設定されていません");
            return;
        }

        // 既存の班員をクリア
        _squadManager.ClearSpawnedUnits();

        // スポーン位置にオフセットを適用
        basePosition += _squadData.SpawnOffset;

        // 各ユニットプレハブを生成
        for (int i = 0; i < _squadData.UnitPrefabs.Count; i++)
        {
            var prefab = _squadData.UnitPrefabs[i];
            if (prefab == null) continue;

            Vector3 spawnPosition = GetSpawnPosition(basePosition, i);
            GameObject unitObj = Object.Instantiate(prefab, spawnPosition, Quaternion.identity, _squadManager.Transform);
            unitObj.name = $"{prefab.name}_{i}";

            // UnitControllerコンポーネントを取得または追加
            UnitController unit = unitObj.GetComponent<UnitController>();
            if (unit == null)
            {
                unit = unitObj.AddComponent<UnitController>();
            }

            // 班との関連付け
            _squadManager.SpawnedUnits.Add(unit);
        }

        Debug.Log($"{_squadData.CompositionName}: 班生成完了 - {_squadManager.SpawnedUnits.Count}人");
    }

    /// <summary>
    /// スポーン位置を計算（円形フォーメーション）
    /// </summary>
    private Vector3 GetSpawnPosition(Vector3 basePosition, int index)
    {
        if (index == 0) return basePosition; // 最初のユニットは基本位置

        // 円形に配置
        float angle = (index - 1) * (360f / _squadData.TotalUnits) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * _squadData.FormationRadius,
            0,
            Mathf.Sin(angle) * _squadData.FormationRadius
        );

        return basePosition + offset;
    }
}