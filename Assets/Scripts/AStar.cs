using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    // 그래프 정보와 시작/목표 노드
    private Dictionary<DSLNode, List<DSLNode>> _graph;
    private DSLNode _start, _goal;
    
    // A*에서 사용될 open set (우선순위 큐)
    private PriorityQueue<DSLNode> _openList;
    
    // 경로 복원을 위한 딕셔너리 (각 노드의 부모 노드)
    private Dictionary<DSLNode, DSLNode> _cameFrom;

    // 생성자: 그래프, 시작, 목표 노드를 받아 초기화
    public AStar(Dictionary<DSLNode, List<DSLNode>> graph, DSLNode start, DSLNode goal)
    {
        _graph = graph;
        _start = start;
        _goal = goal;
        // 우선순위 큐는 f 값 (G + h)을 기준으로 비교하도록 함.
        _openList = new PriorityQueue<DSLNode>(Comparer<DSLNode>.Create(CompareNodes));
        _cameFrom = new Dictionary<DSLNode, DSLNode>();
    }

    // 휴리스틱 함수: 맨해튼 거리 사용 (필요에 따라 다른 휴리스틱 사용 가능)
    private float Heuristic(DSLNode node)
    {
        return Mathf.Abs(node.Pos.x - _goal.Pos.x) + Mathf.Abs(node.Pos.y - _goal.Pos.y);
    }

    // f 값 = g + h 를 계산
    private float f(DSLNode node)
    {
        return node.G + Heuristic(node);
    }

    // 우선순위 큐에서 노드 비교에 사용할 함수 (f 값 기준)
    private int CompareNodes(DSLNode a, DSLNode b)
    {
        float fA = f(a);
        float fB = f(b);
        if (fA < fB)
            return -1;
        if (fA > fB)
            return 1;
        return 0;
    }

    // 두 노드 간 이동 비용 (벽이면 무한대, 아니면 1로 가정)
    private float Cost(DSLNode a, DSLNode b)
    {
        if (a.NodeState == GridState.Wall || b.NodeState == GridState.Wall)
            return Mathf.Infinity;
        return 1f;
    }

    // A* 알고리즘을 실행하여 경로(좌표 목록)를 반환
    public List<Vector3Int> ComputePath()
    {
        // 모든 노드의 G 값을 무한대로 초기화
        foreach (var node in _graph.Keys)
        {
            node.G = Mathf.Infinity;
        }
        _start.G = 0f;

        _openList.Enqueue(_start);

        while (_openList.Count > 0)
        {
            DSLNode current = _openList.Dequeue();

            // 목표 노드에 도달하면 경로 복원 후 반환
            if (current.Equals(_goal))
            {
                return ReconstructPath(current);
            }

            // 인접 노드(이웃)를 순회
            foreach (DSLNode neighbor in _graph[current])
            {
                float tentativeG = current.G + Cost(current, neighbor);
                if (tentativeG < neighbor.G)
                {
                    neighbor.G = tentativeG;
                    // 경로 복원을 위해 현재 노드를 기록
                    _cameFrom[neighbor] = current;
                    
                    // openList에 중복 삽입 여부를 체크하지 않는 간단한 구현이므로, 그냥 삽입합니다.
                    _openList.Enqueue(neighbor);
                }
            }
        }
        // openList가 비었는데도 목표에 도달하지 못한 경우 빈 경로 반환
        return new List<Vector3Int>();
    }

    // 경로 복원: 목표 노드에서 시작 노드까지 거슬러 올라가며 좌표 목록 생성
    private List<Vector3Int> ReconstructPath(DSLNode current)
    {
        List<Vector3Int> totalPath = new List<Vector3Int>();
        totalPath.Add(current.Pos);
        while (_cameFrom.ContainsKey(current))
        {
            current = _cameFrom[current];
            totalPath.Add(current.Pos);
        }
        totalPath.Reverse();
        return totalPath;
    }
}
