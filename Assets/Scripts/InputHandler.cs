using UnityEngine;
using System;

/// <summary>
/// プレイヤーの入力を受け取り、適切なコマンドに変換する入力管理クラス
/// マウス操作とキーボード操作を統合管理
/// </summary>
public class InputHandler : MonoBehaviour
{
    [Header("入力設定")]
    [SerializeField] private Camera _mainCamera;                    // メインカメラ
    [SerializeField] private LayerMask _selectableLayerMask = -1;   // 選択可能オブジェクトのレイヤー

    // イベント通知用デリゲート
    public Action<UnitController> OnUnitSelected;   // ユニット選択時
    public Action<Vector3> OnMoveCommand;           // 移動指示時
    public Action OnPatrolCommand;                  // パトロール開始コマンド
    public Action OnDefendCommand;                  // 防衛開始コマンド

    private UnitController _selectedLeader;         // 現在選択中のリーダー

    private void Start()
    {
        // メインカメラが未設定の場合、自動で取得
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        // 入力処理を毎フレーム実行
        HandleMouseInput();     // マウス入力
        HandleKeyboardInput();  // キーボード入力
    }

    /// <summary>
    /// マウス入力の処理
    /// </summary>
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // 左クリック
        {
            ProcessMouseClick();
        }
    }

    /// <summary>
    /// キーボード入力処理
    /// </summary>
    private void HandleKeyboardInput()
    {
        // Pキー: パトロール開始
        if (Input.GetKeyDown(KeyCode.P))
        {
            HandlePatrolCommand();
        }

        // Dキー: 防衛状態に切り替え
        if (Input.GetKeyDown(KeyCode.D))
        {
            HandleDefendCommand();
        }
    }

    /// <summary>
    /// パトロールコマンド処理
    /// 選択中のリーダーの班全体をパトロール状態に
    /// </summary>
    private void HandlePatrolCommand()
    {
        if (_selectedLeader != null)
        {
            Debug.Log($"InputHandler: パトロール指示 - {_selectedLeader.gameObject.name}");

            // 選択されたリーダーの班をパトロール状態に
            SquadManager squad = _selectedLeader.transform.parent?.GetComponent<SquadManager>();
            if (squad != null)
            {
                SetSquadToPatrolState(squad);
            }
            else
            {
                // 個別ユニットをパトロール状態に
                _selectedLeader.GetStateMachine().ChangeState(AIState.Patrol);
            }

            OnPatrolCommand?.Invoke();
        }
        else
        {
            Debug.Log("InputHandler: リーダーが選択されていません（パトロール）");
        }
    }

    /// <summary>
    /// 防衛コマンド処理
    /// 選択中のリーダーの班全体を防衛状態に
    /// </summary>
    private void HandleDefendCommand()
    {
        if (_selectedLeader != null)
        {
            Debug.Log($"InputHandler: 防衛指示 - {_selectedLeader.gameObject.name}");

            // 選択されたリーダーの班を防衛状態に
            SquadManager squad = _selectedLeader.transform.parent?.GetComponent<SquadManager>();
            if (squad != null)
            {
                SetSquadToDefendState(squad);
            }
            else
            {
                // 個別ユニットを防衛状態に
                _selectedLeader.GetStateMachine().ChangeState(AIState.Defend);
            }

            OnDefendCommand?.Invoke();
        }
        else
        {
            Debug.Log("InputHandler: リーダーが選択されていません（防衛）");
        }
    }

    /// <summary>
    /// 班全体をパトロール状態に設定
    /// </summary>
    private void SetSquadToPatrolState(SquadManager squad)
    {
        foreach (var unit in squad.SpawnedUnits)
        {
            if (unit != null && !unit.GetComponent<CombatManager>().IsDead)
            {
                unit.GetStateMachine().ChangeState(AIState.Patrol);
            }
        }
        Debug.Log($"{squad.name}: 班全体をパトロール状態に設定");
    }

    /// <summary>
    /// 班全体を防衛状態に設定
    /// </summary>
    private void SetSquadToDefendState(SquadManager squad)
    {
        foreach (var unit in squad.SpawnedUnits)
        {
            if (unit != null && !unit.GetComponent<CombatManager>().IsDead)
            {
                unit.GetStateMachine().ChangeState(AIState.Defend);
            }
        }
        Debug.Log($"{squad.name}: 班全体を防衛状態に設定");
    }

    /// <summary>
    /// マウスクリックの処理
    /// ユニット選択または移動指示を判定
    /// </summary>
    private void ProcessMouseClick()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _selectableLayerMask))
        {
            UnitController clickedUnit = hit.collider.GetComponent<UnitController>();

            if (clickedUnit != null)
            {
                // ユニット選択
                HandleUnitSelection(clickedUnit);
            }
            else
            {
                // 地面クリック（移動指示）
                HandleMoveCommand(hit.point);
            }
        }
    }

    /// <summary>
    /// ユニット選択処理
    /// </summary>
    private void HandleUnitSelection(UnitController unit)
    {
        Debug.Log($"InputHandler: ユニット選択 - {unit.gameObject.name}");

        _selectedLeader = unit;
        OnUnitSelected?.Invoke(unit);
    }

    /// <summary>
    /// 移動指示処理
    /// </summary>
    private void HandleMoveCommand(Vector3 position)
    {
        if (_selectedLeader != null)
        {
            Debug.Log($"InputHandler: 移動指示 - {position}");
            OnMoveCommand?.Invoke(position);
        }
        else
        {
            Debug.Log("InputHandler: リーダーが選択されていません");
        }
    }

    // --- 外部インターフェース ---

    /// <summary>
    /// 外部からの選択状態設定
    /// </summary>
    public void SetSelectedLeader(UnitController leader)
    {
        _selectedLeader = leader;
    }

    /// <summary>
    /// 現在選択中のリーダーを取得
    /// </summary>
    public UnitController GetSelectedLeader() => _selectedLeader;
}