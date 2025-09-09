using UnityEngine;

public class SquadPresenter : MonoBehaviour
{
    [Header("MVP Components")]
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private SquadManager _squadModel;

    void Start()
    {
        InitializePresenter();
    }

    private void InitializePresenter()
    {
        if (_inputHandler == null)
        {
            _inputHandler = FindAnyObjectByType<InputHandler>();
        }

        if (_squadModel == null)
        {
            _squadModel = GetComponent<SquadManager>();
        }

        // イベント購読
        _inputHandler.OnUnitSelected += HandleUnitSelection;
        _inputHandler.OnMoveCommand += HandleMoveCommand;

        Debug.Log($"SquadPresenter: 初期化完了 - {gameObject.name}");
    }

    void OnDestroy()
    {
        // イベント購読解除
        if (_inputHandler != null)
        {
            _inputHandler.OnUnitSelected -= HandleUnitSelection;
            _inputHandler.OnMoveCommand -= HandleMoveCommand;
        }
    }

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
}