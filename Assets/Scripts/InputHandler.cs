using UnityEngine;
using System;

public class InputHandler : MonoBehaviour
{
    [Header("入力設定")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _selectableLayerMask = -1;

    [Header("キーボード入力設定")]
    [SerializeField] private KeyCode _patrolModeKey = KeyCode.P;
    [SerializeField] private KeyCode _defendModeKey = KeyCode.D;

    // マウス入力イベント
    public Action<UnitController> OnUnitSelected;
    public Action<Vector3> OnMoveCommand;

    // キーボード入力イベント
    public Action OnPatrolModeCommand;
    public Action OnDefendModeCommand;

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
        HandleKeyboardInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ProcessMouseClick();
        }
    }

    private void HandleKeyboardInput()
    {
        // 班長が選択されている場合のみキーボード入力を受け付ける
        if (_selectedLeader == null) return;

        // 遊撃モード指示
        if (Input.GetKeyDown(_patrolModeKey))
        {
            Debug.Log($"InputHandler: 遊撃モード指示 - {_patrolModeKey}キー押下");
            OnPatrolModeCommand?.Invoke();
        }

        // 防衛モード指示
        if (Input.GetKeyDown(_defendModeKey))
        {
            Debug.Log($"InputHandler: 防衛モード指示 - {_defendModeKey}キー押下");
            OnDefendModeCommand?.Invoke();
        }
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

    // キー設定の変更メソッド
    public void SetPatrolModeKey(KeyCode keyCode)
    {
        _patrolModeKey = keyCode;
        Debug.Log($"InputHandler: 遊撃モードキーを {keyCode} に変更");
    }

    public void SetDefendModeKey(KeyCode keyCode)
    {
        _defendModeKey = keyCode;
        Debug.Log($"InputHandler: 防衛モードキーを {keyCode} に変更");
    }

    // 現在の設定を取得
    public KeyCode GetPatrolModeKey() => _patrolModeKey;
    public KeyCode GetDefendModeKey() => _defendModeKey;
}