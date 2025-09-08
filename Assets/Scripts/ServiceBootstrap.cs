using UnityEngine;

/// <summary>
/// サービスの初期化を管理するブートストラップ
/// </summary>
public class ServiceBootstrap : MonoBehaviour
{
//    [Header("サービス設定")]
//    [SerializeField] private UnitPoolService _unitPoolService;

//    [Header("初期化順序")]
//    [SerializeField] private int _initializationOrder = -100;

//    void Awake()
//    {
//        // 実行順序を設定（他のスクリプトより先に実行）
//        var script = MonoScript.FromMonoBehaviour(this);
//        if (script != null)
//        {
//            MonoImporter.SetExecutionOrder(script, _initializationOrder);
//        }

//        InitializeServices();
//    }

//    /// <summary>
//    /// 全サービスを初期化
//    /// </summary>
//    private void InitializeServices()
//    {
//        Debug.Log("ServiceBootstrap: サービス初期化開始");

//        // UnitPoolServiceを初期化
//        InitializeUnitPoolService();

//        // 他のサービスがあればここに追加
//        // InitializeAudioService();
//        // InitializeUIService();

//        Debug.Log("ServiceBootstrap: サービス初期化完了");

//        // デバッグ用：登録されたサービス一覧を表示
//#if UNITY_EDITOR
//        ServiceLocator.DebugPrintServices();
//#endif
//    }

//    /// <summary>
//    /// UnitPoolServiceを初期化
//    /// </summary>
//    private void InitializeUnitPoolService()
//    {
//        if (_unitPoolService == null)
//        {
//            // UnitPoolServiceが設定されていない場合は自動で探す
//            _unitPoolService = FindAnyObjectByType<UnitPoolService>();

//            if (_unitPoolService == null)
//            {
//                // 見つからない場合は新しく作成
//                GameObject poolObject = new GameObject("UnitPoolService");
//                _unitPoolService = poolObject.AddComponent<UnitPoolService>();

//                Debug.Log("ServiceBootstrap: UnitPoolServiceを自動作成しました");
//            }
//        }

//        // サービスロケーターに登録（UnitPoolServiceのAwakeで自動登録されるが、明示的に確認）
//        if (!ServiceLocator.IsRegistered<IUnitPoolService>())
//        {
//            ServiceLocator.Register<IUnitPoolService>(_unitPoolService);
//            Debug.Log("ServiceBootstrap: UnitPoolServiceを手動登録しました");
//        }
//    }

//    /// <summary>
//    /// アプリケーション終了時のクリーンアップ
//    /// </summary>
//    void OnApplicationQuit()
//    {
//        CleanupServices();
//    }

//    /// <summary>
//    /// サービスのクリーンアップ
//    /// </summary>
//    private void CleanupServices()
//    {
//        Debug.Log("ServiceBootstrap: サービスクリーンアップ開始");

//        // プールをクリア
//        var poolService = ServiceLocator.Get<IUnitPoolService>();
//        if (poolService != null)
//        {
//            poolService.ClearPool();
//        }

//        // サービスロケーターをクリア
//        ServiceLocator.Clear();

//        Debug.Log("ServiceBootstrap: サービスクリーンアップ完了");
//    }

//    /// <summary>
//    /// エディタ用：サービス状態をデバッグ表示
//    /// </summary>
//    [ContextMenu("Debug Print Services")]
//    public void DebugPrintServices()
//    {
//        ServiceLocator.DebugPrintServices();

//        var poolService = ServiceLocator.Get<IUnitPoolService>();
//        if (poolService != null)
//        {
//            Debug.Log($"UnitPool - Active: {poolService.GetActiveUnitCount()}, Pooled: {poolService.GetPooledUnitCount()}");
//        }
//    }
}