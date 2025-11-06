using UnityEngine;

public class SquadController : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private SquadManager _squadManager;

    void Start()
    {
        InitializePresenter();
    }

    /// <summary>
    /// Presenterの初期化
    /// コンポーネント取得とイベント購読
    /// </summary>
    private void InitializePresenter()
    {
        // コンポーネント自動取得
        if (_inputHandler == null)
        {
            _inputHandler = FindAnyObjectByType<InputHandler>();
        }
        if (_squadManager == null)
        {
            _squadManager = GetComponent<SquadManager>();
        }

        _inputHandler.OnUnitSelected += HandleUnitSelection;
        _inputHandler.OnMoveCommand += HandleMoveCommand;
        _inputHandler.OnPatrolCommand += HandlePatrolCommand;  // パトロール指示追加
        _inputHandler.OnDefendCommand += HandleDefendCommand;  // 防衛指示追加

        Debug.Log($"SquadPresenter: 初期化完了 - {gameObject.name}");
    }

    void OnDestroy()
    {
        // イベント購読解除（メモリリーク防止）
        if (_inputHandler != null)
        {
            _inputHandler.OnUnitSelected -= HandleUnitSelection;
            _inputHandler.OnMoveCommand -= HandleMoveCommand;
            _inputHandler.OnPatrolCommand -= HandlePatrolCommand;
            _inputHandler.OnDefendCommand -= HandleDefendCommand;
        }
    }

    /// <summary>
    /// ユニット選択処理
    /// </summary>
    private void HandleUnitSelection(UnitController selectedUnit)
    {
        // 管理する班のメンバーかチェック
        if (_squadManager.IsMember(selectedUnit))
        {
            Debug.Log($"SquadController: 班員選択 - {selectedUnit.gameObject.name}");

            // Squadにリーダー設定を指示
            _squadManager.SetLeader(selectedUnit);

            // InputHandlerに選択状態を通知
            _inputHandler.SetSelectedLeader(selectedUnit);
        }
        else
        {
            Debug.Log($"SquadController: 他の班のメンバー - {selectedUnit.gameObject.name}");
        }
    }

    /// <summary>
    /// 移動指示処理
    /// </summary>
    private void HandleMoveCommand(Vector3 targetPosition)
    {
        UnitController selectedLeader = _inputHandler.GetSelectedLeader();

        // 選択されたリーダーがこの班のメンバーかチェック
        if (selectedLeader != null && _squadManager.IsMember(selectedLeader) && selectedLeader.IsLeader)
        {
            Debug.Log($"SquadController: 移動指示実行 - {targetPosition}");

            // Squadに移動指示
            _squadManager.MoveLeaderTo(targetPosition);
        }
    }

    /// <summary>
    /// パトロール指示処理（自動移動機能）
    /// </summary>
    private void HandlePatrolCommand()
    {
        UnitController selectedLeader = _inputHandler.GetSelectedLeader();

        // 選択されたリーダーがこの班のメンバーかチェック
        if (selectedLeader != null && _squadManager.IsMember(selectedLeader))
        {
            Debug.Log($"SquadController: パトロール指示実行 - {selectedLeader.gameObject.name}");

            // Squadにパトロール指示
            _squadManager.StartPatrolMode();
        }
    }

    /// <summary>
    /// 防衛指示処理
    /// </summary>
    private void HandleDefendCommand()
    {
        UnitController selectedLeader = _inputHandler.GetSelectedLeader();

        // 選択されたリーダーがこの班のメンバーかチェック
        if (selectedLeader != null && _squadManager.IsMember(selectedLeader))
        {
            Debug.Log($"SquadController: 防衛指示実行 - {selectedLeader.gameObject.name}");

            // Squadに防衛指示
            _squadManager.StartDefendMode();
        }
    }
}