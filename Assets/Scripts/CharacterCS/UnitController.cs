using UnityEngine;
using UnityEngine.AI;
using System;

public class UnitController : MonoBehaviour
{
    private NavMeshAgent _agent; // NavMesh移動制御
    private AIStateManager _stateManager; // AI状態管理
    private Renderer _unitRenderer; // 見た目（色変更用）
    private bool _isLeader = false; // 班長フラグ
    private bool _isFollowingLeader = false; // 班長追従中フラグ
    private Transform _currentTarget; // 現在のターゲット
    private SquadManager _mySquad; // 所属班の参照
    private Color _originalColor; // 元の色

    private void Awake()
    {
        // コンポーネント取得と初期設定
        _agent = GetComponent<NavMeshAgent>();
        _unitRenderer = GetComponent<Renderer>();

        if (_unitRenderer != null)
        {
            _originalColor = _unitRenderer.material.color;
        }

        // NavMeshAgent設定
        _agent.speed = 5f;
        _agent.stoppingDistance = 0.1f;
    }

    private void Start()
    {
        // AIステートマシンを取得
        _stateManager = GetComponent<AIStateManager>();
    }

    // --- 公開メソッド（Squadから呼ばれる） ---

    // 班長追従を開始
    public void StartFollowingLeader()
    {
        _isFollowingLeader = true;
        Debug.Log($"{gameObject.name}: 班長追従開始");
    }

    // 追従を停止して防衛状態に移行
    public void StopFollowing()
    {
        _isFollowingLeader = false;

        if (_stateManager != null && _stateManager.GetCurrentState() != AIState.Defend)
        {
            _stateManager.ChangeState(AIState.Defend);
        }

        Debug.Log($"{gameObject.name}: 追従停止 - 防衛状態へ");
    }

    // 所属班を設定
    public void SetSquad(SquadManager squad)
    {
        _mySquad = squad;
        Debug.Log($"{gameObject.name}: 班設定完了 - {squad.gameObject.name}");
    }

    // 班長の設定・解除
    public void SetAsLeader(bool leader)
    {
        _isLeader = leader;

        if (leader)
        {
            // 班長に設定時の処理
            _isFollowingLeader = false;
            transform.localScale = Vector3.one * 1.2f; // サイズアップ

            if (_unitRenderer != null)
            {
                _unitRenderer.material.color = Color.yellow; // 黄色に変更
            }

            if (_agent != null)
            {
                _agent.ResetPath();
                _agent.isStopped = false;
            }

            Debug.Log($"{gameObject.name}: 班長に設定");
        }
        else
        {
            // 班長解除時の処理
            transform.localScale = Vector3.one; // サイズを戻す

            if (_unitRenderer != null)
            {
                _unitRenderer.material.color = _originalColor; // 元の色に戻す
            }
        }
    }

    // 移動中かどうかを判定
    public bool IsMoving()
    {
        if (_agent == null) return false;
        if (_agent.pathPending) return true; // 経路計算中

        return _agent.hasPath && _agent.remainingDistance > 0.3f;
    }

    // 移動を停止
    public void StopMoving()
    {
        if (_agent != null)
        {
            _agent.ResetPath();
            _agent.isStopped = false;
        }
    }

    // 指定位置に移動
    public void MoveTo(Vector3 position)
    {
        if (_agent != null)
        {
            _agent.isStopped = false;
            _agent.ResetPath();

            bool success = _agent.SetDestination(position);
            if (!success)
            {
                Debug.LogWarning($"{gameObject.name}: NavMesh経路設定失敗");
            }
        }
    }

    // 色を設定（班長以外）
    public void SetColor(Color color)
    {
        if (_unitRenderer != null && !_isLeader)
        {
            _unitRenderer.material.color = color;
        }
    }

    // プロパティ
    public AIStateManager GetStateManager() => _stateManager;
    public bool IsLeader => _isLeader;
    public bool IsFollowingLeader => _isFollowingLeader;
    public Vector3 Position => transform.position;
}