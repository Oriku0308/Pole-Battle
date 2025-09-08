using UnityEngine;
using System;

public class InputHandler : MonoBehaviour
{
    [Header("入力設定")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _selectableLayerMask = -1;

    public Action<UnitController> OnUnitSelected;
    public Action<Vector3> OnMoveCommand;
    public Action OnPatrolCommand;      // パトロール開始コマンド
    public Action OnDefendCommand;      // 防衛開始コマンド

    private UnitController _selectedLeader;

    private void Start()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();  // キーボード入力処理を追加
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
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
    /// </summary>
    private void HandlePatrolCommand()
    {
        if (_selectedLeader != null)
        {
            Debug.Log($"InputHandler: パトロール指示 - {_selectedLeader.gameObject.name}");

            // 選択されたリーダーの班をパトロール状態に
            Squad squad = _selectedLeader.transform.parent?.GetComponent<Squad>();
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
    /// </summary>
    private void HandleDefendCommand()
    {
        if (_selectedLeader != null)
        {
            Debug.Log($"InputHandler: 防衛指示 - {_selectedLeader.gameObject.name}");

            // 選択されたリーダーの班を防衛状態に
            Squad squad = _selectedLeader.transform.parent?.GetComponent<Squad>();
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
    private void SetSquadToPatrolState(Squad squad)
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
    private void SetSquadToDefendState(Squad squad)
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

    private void HandleUnitSelection(UnitController unit)
    {
        Debug.Log($"InputHandler: ユニット選択 - {unit.gameObject.name}");

        _selectedLeader = unit;
        OnUnitSelected?.Invoke(unit);
    }

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

    // 外部からの選択状態設定
    public void SetSelectedLeader(UnitController leader)
    {
        _selectedLeader = leader;
    }

    public UnitController GetSelectedLeader() => _selectedLeader;
}