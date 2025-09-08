using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ユニットプールサービスのインターフェース
/// </summary>
public interface IUnitPoolService
{
    GameObject GetUnit(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent);
    void ReturnUnit(GameObject unit);
    int GetActiveUnitCount();
    int GetPooledUnitCount();
    void ClearPool();
}

/// <summary>
/// ユニットのオブジェクトプールサービス（直接参照版）
/// </summary>
public class UnitPoolService : MonoBehaviour, IUnitPoolService
{
    [Header("プール設定")]
    [SerializeField] private int _poolSize = 50;

    private Queue<GameObject> _unitPool = new Queue<GameObject>();
    private List<GameObject> _activeUnits = new List<GameObject>();

    /// <summary>
    /// プールからユニットを取得
    /// </summary>
    public GameObject GetUnit(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject unit;

        if (_unitPool.Count > 0)
        {
            // プールから取得
            unit = _unitPool.Dequeue();
            unit.transform.position = position;
            unit.transform.rotation = rotation;
            unit.transform.SetParent(parent);
        }
        else
        {
            // 新規作成
            unit = Instantiate(prefab, position, rotation, parent);
        }

        // アクティブ化
        unit.SetActive(true);
        _activeUnits.Add(unit);

        // コンポーネントを初期化
        ResetUnitComponents(unit);

        Debug.Log($"UnitPoolService: ユニット取得 - {unit.name}");
        return unit;
    }

    /// <summary>
    /// ユニットをプールに返却
    /// </summary>
    public void ReturnUnit(GameObject unit)
    {
        if (unit == null) return;

        // アクティブリストから削除
        _activeUnits.Remove(unit);

        // 非アクティブ化
        unit.SetActive(false);
        unit.transform.SetParent(transform);

        // プールに返却
        _unitPool.Enqueue(unit);

        Debug.Log($"UnitPoolService: ユニット返却 - {unit.name}");
    }

    /// <summary>
    /// プールをクリア
    /// </summary>
    public void ClearPool()
    {
        // アクティブなユニットを全て返却
        var activeUnitsCopy = new List<GameObject>(_activeUnits);
        foreach (var unit in activeUnitsCopy)
        {
            if (unit != null)
            {
                ReturnUnit(unit);
            }
        }

        // プール内のユニットを削除
        while (_unitPool.Count > 0)
        {
            var unit = _unitPool.Dequeue();
            if (unit != null)
            {
                Destroy(unit);
            }
        }

        _activeUnits.Clear();
        _unitPool.Clear();

        Debug.Log("UnitPoolService: プールをクリアしました");
    }

    /// <summary>
    /// ユニットコンポーネントを初期化
    /// </summary>
    private void ResetUnitComponents(GameObject unit)
    {
        // CombatManagerをリセット
        CombatManager combat = unit.GetComponent<CombatManager>();
        if (combat != null)
        {
            combat.ResetToInitialState();
        }

        // UnitControllerをリセット
        UnitController controller = unit.GetComponent<UnitController>();
        if (controller != null)
        {
            controller.ResetToInitialState();
        }

        // AIStateMachineをリセット
        AIStateMachine stateMachine = unit.GetComponent<AIStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.enabled = true;
            stateMachine.ResetToInitialState();
        }

        // その他のコンポーネントをリセット
        Collider collider = unit.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    /// <summary>
    /// 現在アクティブなユニット数を取得
    /// </summary>
    public int GetActiveUnitCount()
    {
        return _activeUnits.Count;
    }

    /// <summary>
    /// プール内のユニット数を取得
    /// </summary>
    public int GetPooledUnitCount()
    {
        return _unitPool.Count;
    }

    /// <summary>
    /// デバッグ情報表示
    /// </summary>
    [ContextMenu("Show Pool Status")]
    public void ShowPoolStatus()
    {
        Debug.Log($"UnitPool Status - Active: {GetActiveUnitCount()}, Pooled: {GetPooledUnitCount()}");
    }
}