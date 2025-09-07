using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    private NavMeshAgent agent;
    private AIStateMachine stateMachine;
    private Renderer unitRenderer;

    [Header("AI Parameters")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int hp = 100;

    private Transform currentTarget;
    private Squad mySquad;
    private bool isLeader = false;
    private Color originalColor;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        unitRenderer = GetComponent<Renderer>();
        originalColor = unitRenderer.material.color;

        agent.speed = 5f;
        agent.stoppingDistance = 0.1f;
    }

    void Start()
    {
        stateMachine = GetComponent<AIStateMachine>();
    }

    void Update()
    {
        // マウスクリックでキャラクター選択
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // このキャラクターがクリックされた場合
                if (hit.collider.gameObject == gameObject && mySquad != null)
                {
                    mySquad.SetLeader(this);
                }
                // リーダーの場合は移動指示
                else if (isLeader && mySquad != null)
                {
                    mySquad.MoveTo(hit.point);
                }
            }
        }
    }

    public void SetSquad(Squad squad)
    {
        mySquad = squad;
        Debug.Log($"{gameObject.name}: 班設定完了 - {squad.gameObject.name}");
    }

    public void SetAsLeader(bool leader)
    {
        isLeader = leader;

        if (leader)
        {
            // 班長の視覚的区別
            transform.localScale = Vector3.one * 1.2f;
            unitRenderer.material.color = Color.yellow; // 班長は黄色
            Debug.Log($"{gameObject.name}: 班長に設定");
        }
        else
        {
            // 通常メンバーに戻す
            transform.localScale = Vector3.one;
            unitRenderer.material.color = originalColor;
        }
    }

    public AIStateMachine GetStateMachine() => stateMachine;

    public bool IsMoving()
    {
        if (agent == null) return false;
        if (agent.pathPending) return true;
        return agent.hasPath && agent.remainingDistance > 0.5f;
    }

    public void StopMoving()
    {
        if (agent != null)
        {
            agent.ResetPath();
        }
    }

    public void MoveTo(Vector3 position)
    {
        if (agent != null)
        {
            agent.SetDestination(position);
        }
    }

    public void SetColor(Color color)
    {
        if (unitRenderer != null && !isLeader)
        { // 班長の色は変更しない
            unitRenderer.material.color = color;
        }
    }

    public Transform FindNearestEnemy()
    {
        return null;
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }
}