using System.Collections.Generic;
using UnityEngine;

public class DStarLite {
    // 간선 정보를 담은 그래프: 각 노드와 그 노드와 연결된 이웃 노드 리스트
    public Dictionary<DSLNode, List<DSLNode>> Graph;
    
    // 우선순위 큐(Open List)
    private PriorityQueue<DSLNode> mOpenList;
    
    // 시작 노드와 목표 노드
    private DSLNode mStart, mGoal;
    
    // 환경 변화에 따른 누적 오프셋 (D*Lite에서 사용)
    private float mK_m = 0f;

    public DStarLite(Dictionary<DSLNode, List<DSLNode>> graph, DSLNode start, DSLNode goal) {
        this.Graph = graph;
        this.mStart = start;
        this.mGoal = goal;
        // 우선순위 큐 생성: CompareNodes 메서드를 IComparer로 변환
        mOpenList = new PriorityQueue<DSLNode>(Comparer<DSLNode>.Create(compareNodes));
        initialize();
    }

    // 맨해튼 거리 휴리스틱
    private float heuristic(DSLNode a, DSLNode b) {
        return Mathf.Abs(a.Pos.x - b.Pos.x) + Mathf.Abs(a.Pos.y - b.Pos.y);
    }

    // D*Lite의 키 계산: Key(u) = (min(g(u), rhs(u)) + h(start,u) + k_m, min(g(u), rhs(u)))
    private (float, float) calculateKey(DSLNode u) {
        float minVal = Mathf.Min(u.G, u.Rhs);
        return (minVal + heuristic(mStart, u) + mK_m, minVal);
    }

    // 우선순위 큐에서 노드 비교에 사용할 함수
    private int compareNodes(DSLNode a, DSLNode b) {
        if (a.Key1 < b.Key1) return -1;
        if (a.Key1 > b.Key1) return 1;
        if (a.Key2 < b.Key2) return -1;
        if (a.Key2 > b.Key2) return 1;
        return 0;
    }

    // 초기화: 모든 노드의 g, rhs 값을 무한대로 설정하고, 목표 노드의 rhs를 0으로 세팅
    private void initialize() {
        foreach (DSLNode node in Graph.Keys) {
            node.G = Mathf.Infinity;
            node.Rhs = Mathf.Infinity;
        }
        mGoal.Rhs = 0f;
        var key = calculateKey(mGoal);
        mGoal.Key1 = key.Item1;
        mGoal.Key2 = key.Item2;
        mOpenList.Enqueue(mGoal);
    }

    // 두 노드 간 이동 비용: 장애물이면 무한대, 아니면 1
    private float cost(DSLNode a, DSLNode b) {
        if (a.NodeState == GridState.Wall || b.NodeState == GridState.Wall) return Mathf.Infinity;
        return 1f;
    }

    // 현재 노드의 후행자(성공자) 반환  
    // 여기서는 그래프가 양방향이라고 가정하여, Graph에 저장된 이웃 노드를 그대로 반환
    private List<DSLNode> getSuccessors(DSLNode u) {
        if (Graph.ContainsKey(u))
            return Graph[u];
        else 
            return new List<DSLNode>();
    }

    // 양방향 그래프에서는 predecessors도 동일하게 처리 가능
    private List<DSLNode> getPredecessors(DSLNode u) {
        return getSuccessors(u);
    }

    // u의 값이 변경되었을 때, g와 rhs 값을 재계산하고 openList를 갱신
    private void updateVertex(DSLNode u) {
        if (!u.Equals(mGoal)) {
            float minRhs = Mathf.Infinity;
            foreach (DSLNode s in getPredecessors(u)) {
                float tentative = cost(s, u) + s.G;
                if (tentative < minRhs)
                    minRhs = tentative;
            }
            u.Rhs = minRhs;
        }
        // u가 openList에 있다면 제거
        mOpenList.Remove(u);
        if (u.G != u.Rhs) {
            var key = calculateKey(u);
            u.Key1 = key.Item1;
            u.Key2 = key.Item2;
            mOpenList.Enqueue(u);
        }
    }

    public float GetShortestPathCost()
    {
        return mStart.G;
    }
    
    // D*Lite의 주 알고리즘 루프
    public void ComputeShortestPath() {
        while (mOpenList.Count > 0 &&
               (compareNodes(mOpenList.Peek(), mStart) < 0 || mStart.Rhs != mStart.G)) {
            DSLNode u = mOpenList.Dequeue();
            var oldKey = (key1: u.Key1, key2: u.Key2);
            var newKey = calculateKey(u);
            // 키 값이 바뀌었으면 다시 삽입
            if (oldKey.Item1 < newKey.Item1 || (oldKey.Item1 == newKey.Item1 && oldKey.Item2 < newKey.Item2)) {
                u.Key1 = newKey.Item1;
                u.Key2 = newKey.Item2;
                mOpenList.Enqueue(u);
            }
            // g 값을 rhs 값으로 낮추고, 주변 노드 업데이트
            else if (u.G > u.Rhs) {
                u.G = u.Rhs;
                foreach (DSLNode pred in getPredecessors(u))
                    updateVertex(pred);
            }
            else {
                u.G = Mathf.Infinity;
                updateVertex(u);
                foreach (DSLNode pred in getPredecessors(u))
                    updateVertex(pred);
            }
        }
    }

    // start에서 goal까지의 경로(좌표 목록)를 반환
    public List<Vector3Int> GetPath() {
        List<Vector3Int> path = new List<Vector3Int>();
        DSLNode current = mStart;
        if (mStart.G == Mathf.Infinity)
            return path; // 경로 없음

        path.Add(current.Pos);
        while (!current.Equals(mGoal)) {
            List<DSLNode> successors = getSuccessors(current);
            float minVal = Mathf.Infinity;
            DSLNode next = null;
            foreach (DSLNode s in successors) {
                float cost = this.cost(current, s);
                float val = cost + s.G;
                if (val < minVal) {
                    minVal = val;
                    next = s;
                }
            }
            if (next == null)
                break;
            path.Add(next.Pos);
            current = next;
        }
        return path;
    }

    // 장애물 업데이트: 해당 노드의 장애물 여부를 변경하고, 관련 노드들을 업데이트 후 경로 재계산
    public void UpdateObstacle(DSLNode node, GridState state) {
        node.NodeState = state;
        updateVertex(node);
        foreach (DSLNode neighbor in getSuccessors(node))
            updateVertex(neighbor);
        ComputeShortestPath();
    }

    // 시작 노드 이동 시 갱신
    public void MoveStart(DSLNode newStart) {
        mStart = newStart;
        ComputeShortestPath();
    }

    // 목표 노드 이동 시 갱신 (필요에 따라 초기화)
    public void MoveGoal(DSLNode newGoal) {
        mGoal = newGoal;
        initialize();
        ComputeShortestPath();
    }
}
