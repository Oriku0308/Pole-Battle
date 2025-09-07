using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Squad Data", menuName = "Squad System/Squad Data")]
public class SquadData : ScriptableObject
{
    [Header("班構成")]
    [SerializeField] private string _compositionName;
    [SerializeField] private List<GameObject> _unitPrefabs = new List<GameObject>();

    [Header("班設定")]
    [SerializeField] private float _formationRadius = 2f;
    [SerializeField] private Vector3 _spawnOffset = Vector3.zero;

    // プロパティ
    public string CompositionName => _compositionName;
    public List<GameObject> UnitPrefabs => _unitPrefabs;
    public float FormationRadius => _formationRadius;
    public Vector3 SpawnOffset => _spawnOffset;
    public int TotalUnits => _unitPrefabs.Count;
}