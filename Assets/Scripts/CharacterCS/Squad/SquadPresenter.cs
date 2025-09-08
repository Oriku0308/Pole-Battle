using UnityEngine;

public class SquadPresenter : MonoBehaviour
{
    [Header("MVP Components")]
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private Squad _squadModel;

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
            _squadModel = GetComponent<Squad>();
        }

        // マウス入力イベント購読
        _inputHandler.OnUnitSelected += HandleUnitSelection;
        _inputHandler.OnMoveCommand += HandleMoveCommand;

        // キーボード入力イベント購読
        _inputHandler.OnPatrolModeCommand += HandlePatrolCommand;
        _inputHandler.OnDefendModeCommand += HandleDefendCommand;

        Debug.Log($"SquadPresenter: 初期化完了 - {gameObject.name}");
    }

    void OnDestroy()
    {
        // イベント購読解除
        if (_inputHandler != null)
        {
            _inputHandler.OnUnitSelected -= HandleUnitSelection;
            _inputHandler.OnMoveCommand -= HandleMoveCommand;
            _inputHandler.OnPatrolModeCommand -= HandlePatrolCommand;
            _inputHandler.OnDefendModeCommand -= HandleDefendCommand;
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

            // 選択したユニットの情報をデバッグ表示
            DisplayUnitInfo(selectedUnit);
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

    private void HandlePatrolCommand()
    {
        UnitController selectedLeader = _inputHandler.GetSelectedLeader();

        // 選択されたリーダーがこの班のメンバーかチェック
        if (selectedLeader != null && _squadModel.IsMember(selectedLeader) && selectedLeader.IsLeader)
        {
            Debug.Log($"SquadPresenter: 遊撃モード指示 - {selectedLeader.gameObject.name}");

            // Squad単位で遊撃モード開始
            _squadModel.StartPatrolMode();
        }
        else
        {
            Debug.Log($"SquadPresenter: 遊撃指示無効 - 班長が選択されていません");
        }
    }

    private void HandleDefendCommand()
    {
        UnitController selectedLeader = _inputHandler.GetSelectedLeader();

        // 選択されたリーダーがこの班のメンバーかチェック
        if (selectedLeader != null && _squadModel.IsMember(selectedLeader) && selectedLeader.IsLeader)
        {
            Debug.Log($"SquadPresenter: 防衛モード指示 - {selectedLeader.gameObject.name}");

            // Squad単位で防衛モード開始
            _squadModel.StartDefendMode();
        }
        else
        {
            Debug.Log($"SquadPresenter: 防衛指示無効 - 班長が選択されていません");
        }
    }

    private void DisplayUnitInfo(UnitController unit)
    {
        CombatManager combat = unit.GetComponent<CombatManager>();
        if (combat != null)
        {
            Debug.Log($"ユニット情報 - {unit.gameObject.name}: HP:{combat.CurrentHP}/{combat.MaxHP}, 状態:{unit.GetStateMachine().GetCurrentState()}");
        }
    }

    // 外部から状態変更（UI用）
    public void OnPatrolButtonClick()
    {
        HandlePatrolCommand();
    }

    public void OnDefendButtonClick()
    {
        HandleDefendCommand();
    }

    // 統計情報取得
    public int GetAliveUnitCount()
    {
        int count = 0;
        foreach (var unit in _squadModel.SpawnedUnits)
        {
            if (unit != null)
            {
                CombatManager combat = unit.GetComponent<CombatManager>();
                if (combat != null && !combat.IsDead)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public float GetSquadAverageHP()
    {
        float totalHP = 0f;
        float totalMaxHP = 0f;
        int aliveCount = 0;

        foreach (var unit in _squadModel.SpawnedUnits)
        {
            if (unit != null)
            {
                CombatManager combat = unit.GetComponent<CombatManager>();
                if (combat != null && !combat.IsDead)
                {
                    totalHP += combat.CurrentHP;
                    totalMaxHP += combat.MaxHP;
                    aliveCount++;
                }
            }
        }

        return aliveCount > 0 ? (totalHP / totalMaxHP) : 0f;
    }

    // プロパティ
    public Squad SquadModel => _squadModel;
    public InputHandler InputHandler => _inputHandler;
}