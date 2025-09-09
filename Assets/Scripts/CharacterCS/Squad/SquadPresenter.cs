using UnityEngine;

/// <summary>
/// MVPパターンのPresenter層
/// InputHandlerからの入力を受け取り、SquadManagerに適切なコマンドを送る中継役
/// </summary>
public class SquadPresenter : MonoBehaviour
{
    [Header("MVP Components")]
    [SerializeField] private InputHandler _inputHandler;   // 入力管理（View層）
    [SerializeField] private SquadManager _squadModel;     // 班管理（Model層）

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
        if (_squadModel == null)
        {
            _squadModel = GetComponent<SquadManager>();
        }

        // イベント購読（InputHandler → Presenter）
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
        // このPresenterが管理する班のメンバーかチェック
        if (_squadModel.IsMember(selectedUnit))
        {
            Debug.Log($"SquadPresenter: 班員選択 - {selectedUnit.gameObject.name}");

            // Model（Squad）にリーダー設定を指示
            _squadModel.SetLeader(selectedUnit);

            // InputHandlerに選択状態を通知
            _inputHandler.SetSelectedLeader(selectedUnit);
        }
        else
        {
            Debug.Log($"SquadPresenter: 他の班のメンバー - {selectedUnit.gameObject.name}");
        }
    }

    /// <summary>
    /// 移動指示処理
    /// </summary>
    private void HandleMoveCommand(Vector3 targetPosition)
    {
        UnitController selectedLeader = _inputHandler.GetSelectedLeader();

        // 選択されたリーダーがこの班のメンバーかチェック
        if (selectedLeader != null && _squadModel.IsMember(selectedLeader) && selectedLeader.IsLeader)
        {
            Debug.Log($"SquadPresenter: 移動指示実行 - {targetPosition}");

            // Model（Squad）に移動指示
            _squadModel.MoveLeaderTo(targetPosition);
        }
    }

    /// <summary>
    /// パトロール指示処理（自動移動機能）
    /// </summary>
    private void HandlePatrolCommand()
    {
        UnitController selectedLeader = _inputHandler.GetSelectedLeader();

        // 選択されたリーダーがこの班のメンバーかチェック
        if (selectedLeader != null && _squadModel.IsMember(selectedLeader))
        {
            Debug.Log($"SquadPresenter: パトロール指示実行 - {selectedLeader.gameObject.name}");

            // Model（Squad）にパトロール指示
            _squadModel.StartPatrolMode();
        }
    }

    /// <summary>
    /// 防衛指示処理
    /// </summary>
    private void HandleDefendCommand()
    {
        UnitController selectedLeader = _inputHandler.GetSelectedLeader();

        // 選択されたリーダーがこの班のメンバーかチェック
        if (selectedLeader != null && _squadModel.IsMember(selectedLeader))
        {
            Debug.Log($"SquadPresenter: 防衛指示実行 - {selectedLeader.gameObject.name}");

            // Model（Squad）に防衛指示
            _squadModel.StartDefendMode();
        }
    }

    /// <summary>
    /// 班長変更時の処理（死亡による自動選出時など）
    /// </summary>
    private void HandleLeaderChanged(UnitController newLeader)
    {
        UnitController currentSelected = _inputHandler.GetSelectedLeader();

        // 現在選択中のユニットがこの班のメンバーでなくなった場合
        // （死亡して班員リストから除外された場合）
        if (currentSelected != null && !_squadModel.IsMember(currentSelected))
        {
            // 新しい班長を選択状態にする
            _inputHandler.SetSelectedLeader(newLeader);
            Debug.Log($"SquadPresenter: 操作対象を新班長に自動切り替え - {newLeader.gameObject.name}");
        }
    }
}