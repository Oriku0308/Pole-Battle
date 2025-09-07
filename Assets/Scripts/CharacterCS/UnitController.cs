using UnityEngine;
using UnityEngine.AI;
using System;

public class UnitController : MonoBehaviour
{
    private NavMeshAgent _agent;
    private AIStateMachine _stateMachine;
    private Renderer _unitRenderer;

    [Header("ランタイム情報")]
    [SerializeField] private bool _isLeader = false;
    [SerializeField] private bool _isFollowingLeader = false;

    private Transform _currentTarget;
    private Squad _mySquad;
    private Color _originalColor;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _unitRenderer = GetComponent<Renderer>();

        if (_unitRenderer != null)
        {
            _originalColor = _unitRenderer.material.color;
        }

        _agent.speed = 5f;
        _agent.stoppingDistance = 0.1f;
    }

    private void Start()
    {
        _stateMachine = GetComponent<AIStateMachine>();
    }

    // --- 公開メソッド（Squadから呼ばれる） ---

    public void StartFollowingLeader()
    {
        _isFollowingLeader = true;
        Debug.Log($"{gameObject.name}: 班長追従開始");
    }

    public void StopFollowing()
    {
        _isFollowingLeader = false;

        if (_stateMachine != null && _stateMachine.GetCurrentState() != AIState.Defend)
        {
            _stateMachine.ChangeState(AIState.Defend);
        }

        Debug.Log($"{gameObject.name}: 追従停止 - 防衛状態へ");
    }

    public void SetSquad(Squad squad)
    {
        _mySquad = squad;
        Debug.Log($"{gameObject.name}: 班設定完了 - {squad.gameObject.name}");
    }

    public void SetAsLeader(bool leader)
    {
        _isLeader = leader;

        if (leader)
        {
            _isFollowingLeader = false;
            transform.localScale = Vector3.one * 1.2f;
            if (_unitRenderer != null)
            {
                _unitRenderer.material.color = Color.yellow;
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
            transform.localScale = Vector3.one;
            if (_unitRenderer != null)
            {
                _unitRenderer.material.color = _originalColor;
            }
        }
    }

    public bool IsMoving()
    {
        if (_agent == null) return false;
        if (_agent.pathPending) return true;
        return _agent.hasPath && _agent.remainingDistance > 0.3f;
    }

    public void StopMoving()
    {
        if (_agent != null)
        {
            _agent.ResetPath();
            _agent.isStopped = false;
        }
    }

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

    public void SetColor(Color color)
    {
        if (_unitRenderer != null && !_isLeader)
        {
            _unitRenderer.material.color = color;
        }
    }

    public Transform FindNearestEnemy()
    {
        return null;
    }

    public void SetTarget(Transform target)
    {
        _currentTarget = target;
    }

    // プロパティ
    public AIStateMachine GetStateMachine() => _stateMachine;
    public bool IsLeader => _isLeader;
    public bool IsFollowingLeader => _isFollowingLeader;
    public Vector3 Position => transform.position;
}