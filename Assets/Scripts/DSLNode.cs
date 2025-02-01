using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DSLNode
{
    public GridState NodeState;
    
    public Vector3Int Pos;    // 격자 좌표
    public float G;           // 현재 경로 비용
    public float Rhs;         // one-step lookahead 비용
    
    // D* Lite에서 우선순위 큐에 넣기 위해 사용하는 키 (k1, k2)
    public float Key1;
    public float Key2;

    public DSLNode(Vector3Int position, GridState gridState) {
        Pos = position;
        G = Mathf.Infinity;
        Rhs = Mathf.Infinity;
        NodeState = gridState;
    }
}
