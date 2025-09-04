using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 基本設定
        agent.speed = 5f;
        agent.stoppingDistance = 0.1f;
    }

    // テスト用：クリックで移動
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                MoveTo(hit.point);
            }
        }
    }

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }
}