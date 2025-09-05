using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    private NavMeshAgent agent;
    private AIStateMachine stateMachine;
    private Renderer unitRenderer;

    [Header("AI Parameters")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public int hp = 100;

    private Transform currentTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        stateMachine = GetComponent<AIStateMachine>();
        unitRenderer = GetComponent<Renderer>();

        // 基本設定
        agent.speed = 5f;
        agent.stoppingDistance = 0.1f;
    }

    // ステートマシンとの連携メソッド
    public AIStateMachine GetStateMachine() => stateMachine;
    public bool IsMoving() => agent.remainingDistance > 0.1f;
    public void StopMoving() => agent.ResetPath();
    public void MoveTo(Vector3 position) => agent.SetDestination(position);

    // 視覚的フィードバック
    public void SetColor(Color color)
    {
        unitRenderer.material.color = color;
    }

    // 敵検出（仮実装）
    public Transform FindNearestEnemy()
    {
        // 簡単な敵検出ロジック
        // 後で詳細実装
        return null;
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }
}