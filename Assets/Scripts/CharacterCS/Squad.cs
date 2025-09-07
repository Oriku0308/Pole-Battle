using UnityEngine;
using System.Collections.Generic;

public class Squad : MonoBehaviour
{
    [Header("班構成")]
    [SerializeField] private List<UnitController> members = new List<UnitController>();

    [Header("フォーメーション")]
    [SerializeField] private float formationRadius = 2f;

    private UnitController currentLeader;

    void Start()
    {
        // 全メンバーにこの班を設定
        foreach (var member in members)
        {
            member.SetSquad(this);
        }

        Debug.Log($"{gameObject.name}: 班初期化完了 - メンバー数: {members.Count}");
    }

    // クリックされたキャラクターをリーダーに設定
    public void SetLeader(UnitController newLeader)
    {
        // 前のリーダーのリーダー状態を解除
        if (currentLeader != null)
        {
            currentLeader.SetAsLeader(false);
        }

        // 新しいリーダーを設定
        currentLeader = newLeader;
        currentLeader.SetAsLeader(true);

        Debug.Log($"{gameObject.name}: 新しい班長 - {currentLeader.gameObject.name}");
    }

    // 班全体に移動指示
    public void MoveTo(Vector3 targetPosition)
    {
        if (currentLeader == null)
        {
            Debug.LogWarning($"{gameObject.name}: 班長が設定されていません");
            return;
        }

        Debug.Log($"{gameObject.name}: 班移動指示 - {targetPosition}");

        // 班長が先頭で移動
        currentLeader.MoveTo(targetPosition);
        currentLeader.GetStateMachine().ChangeState(AIState.Move);

        // 班員はフォーメーション位置に移動
        foreach (var member in members)
        {
            if (member != currentLeader)
            {
                Vector3 formationPos = GetFormationPosition(targetPosition, member);
                member.MoveTo(formationPos);
                member.GetStateMachine().ChangeState(AIState.Move);
            }
        }
    }

    private Vector3 GetFormationPosition(Vector3 leaderPos, UnitController member)
    {
        int memberIndex = members.IndexOf(member);
        if (memberIndex <= 0) return leaderPos; // 班長の場合

        float angle = (memberIndex - 1) * (360f / (members.Count - 1)) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * formationRadius,
            0,
            Mathf.Sin(angle) * formationRadius
        );
        return leaderPos + offset;
    }

    // このキャラが班のメンバーかチェック
    public bool IsMember(UnitController unit)
    {
        return members.Contains(unit);
    }

    public List<UnitController> Members => members;
    public UnitController CurrentLeader => currentLeader;
}