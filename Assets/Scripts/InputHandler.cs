using UnityEngine;
using System;

public class InputHandler : MonoBehaviour
{
    [Header("入力設定")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _selectableLayerMask = -1;

    public Action<UnitController> OnUnitSelected;
    public Action<Vector3> OnMoveCommand;

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
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ProcessMouseClick();
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
}