using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// サービスロケーターパターンでサービスを管理
/// </summary>
public static class ServiceLocator
{
    private static Dictionary<System.Type, object> _services = new Dictionary<System.Type, object>();

    /// <summary>
    /// サービスを登録
    /// </summary>
    public static void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"ServiceLocator: {type.Name} は既に登録されています。上書きします。");
            _services[type] = service;
        }
        else
        {
            _services.Add(type, service);
            Debug.Log($"ServiceLocator: {type.Name} を登録しました。");
        }
    }

    /// <summary>
    /// サービスを取得
    /// </summary>
    public static T Get<T>() where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out object service))
        {
            return service as T;
        }

        Debug.LogError($"ServiceLocator: {type.Name} が見つかりません。登録されているか確認してください。");
        return null;
    }

    /// <summary>
    /// サービスが登録されているかチェック
    /// </summary>
    public static bool IsRegistered<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }

    /// <summary>
    /// サービスの登録を解除
    /// </summary>
    public static void Unregister<T>() where T : class
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            _services.Remove(type);
            Debug.Log($"ServiceLocator: {type.Name} の登録を解除しました。");
        }
        else
        {
            Debug.LogWarning($"ServiceLocator: {type.Name} は登録されていません。");
        }
    }

    /// <summary>
    /// 全サービスをクリア
    /// </summary>
    public static void Clear()
    {
        _services.Clear();
        Debug.Log("ServiceLocator: 全サービスをクリアしました。");
    }

    /// <summary>
    /// 登録されているサービス一覧をデバッグ表示
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DebugPrintServices()
    {
        Debug.Log("=== ServiceLocator 登録サービス一覧 ===");
        foreach (var kvp in _services)
        {
            Debug.Log($"- {kvp.Key.Name}: {kvp.Value}");
        }
        Debug.Log("=====================================");
    }
}