using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

public enum GridState
{
    Path,
    Source,
    Wall,
    Dest,

    Length
}

public class TileManager : MonoBehaviour
{
    public Tuple<int, int> TileMapSize;


    public GridState mouseGridState;
    [SerializeField] GridState mDefaultGridState;
    [SerializeField] Tilemap mTilemap;
    [SerializeField] Tilemap mPreviewTilemap;
    [SerializeField] Tilemap mPathfindingTilemap;
    [SerializeField] Vector3Int mTileOffset;
    [SerializeField] List<Tile> mTileList;
    [SerializeField] List<Tile> mPreviewTileList;
    [FormerlySerializedAs("EdgeTile")] [SerializeField] Tile mEdgeTile;

    private Dictionary<Vector3Int, DSLNode> mMap;
    private Vector3Int mLastChangedPreviewTilePos;

    // List index = (int)GridState 
    private List<int> mTileUsageLimitList;
    private List<int> mTileUsageCounter;
    private List<List<Vector3Int>> mTilePositionList;

    private DStarLite mDStarLite;
    private AStar mAStar;

    void Start()
    {
        TileMapSize = new Tuple<int, int>(20, 20);
        mTileOffset = new Vector3Int(TileMapSize.Item1 / 2, TileMapSize.Item2 / 2, 0);
        mouseGridState = mDefaultGridState;
        mMap = new Dictionary<Vector3Int, DSLNode>();
        int halfMapSizeX = TileMapSize.Item1 / 2;
        int halfMapSizeY = TileMapSize.Item2 / 2;
        for (int i = -halfMapSizeX; i < halfMapSizeX; i++)
        {
            for (int j = -halfMapSizeY; j < halfMapSizeY; j++)
            {
                DSLNode node = new DSLNode(new Vector3Int(i, j, 0), GridState.Path);
                mMap.Add(new Vector3Int(i, j, 0), node);
            }
        }

        int tileCount = TileMapSize.Item1 * TileMapSize.Item2;
        mTileUsageLimitList = new List<int>()
        {
            tileCount, 1, tileCount, 1
        };

        mDefaultGridState = GridState.Path;


        mTileUsageCounter = new List<int>();
        mTilePositionList = new List<List<Vector3Int>>();
        for (int i = 0; i < (int)GridState.Length; i++)
        {
            mTileUsageCounter.Add(0);
            mTilePositionList.Add(new List<Vector3Int>());
        }

        mTileUsageCounter[(int)mDefaultGridState] = tileCount;
        for (int i = -halfMapSizeX; i < halfMapSizeX; i++)
        {
            for (int j = -halfMapSizeY; j < halfMapSizeY; j++)
            {
                mTilePositionList[(int)mDefaultGridState].Add(new Vector3Int(i, j, 0));
            }
        }

        temp();
    }

    void temp()
    {
        // DSLNode startNode = new DSLNode(new Vector3Int(0, 0, 0), GridState.Source);
        // DSLNode endNode = new DSLNode(new Vector3Int(1, 0, 0), GridState.Dest);
        // Dictionary<DSLNode, List<DSLNode>> graph = new Dictionary<DSLNode, List<DSLNode>>();
        // graph.Add(startNode, new List<DSLNode>(){endNode});
        // graph.Add(endNode, new List<DSLNode>(){startNode});
        //
        // mDStarLite = new DStarLite(graph, startNode, endNode);
        // mDStarLite.ComputeShortestPath();
        // Debug.Log(mDStarLite.GetShortestPathCost());
        // var path = mDStarLite.GetPath();
        // foreach (var v in path)
        // {
        //     Debug.Log(v);
        // }
    }
    
    void Update()
    {
        processInputGridState();
        processMouseClickInput();
        processOnMouseInput();
        processPathfinding();
    }

    private void processPathfinding()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canMakePathfinding())
        {
            // if (mDStarLite == null)
            // {
            //     Dictionary<DSLNode, List<DSLNode>> graph = getGraphFromMap();
            //     DSLNode startNode = mMap[mTilePositionList[(int)GridState.Source][0]];
            //     DSLNode endNode = mMap[mTilePositionList[(int)GridState.Dest][0]];
            //     mDStarLite = new DStarLite(graph, startNode, endNode);
            // }
            //
            // mDStarLite.ComputeShortestPath();
            // Debug.Log(mDStarLite.GetShortestPathCost());
            // var path = mDStarLite.GetPath();
            // foreach (var v in path)
            // {
            //     Debug.Log(v);
            // }

            if (mDStarLite == null)
            {
                Dictionary<DSLNode, List<DSLNode>> graph = getGraphFromMap();
                DSLNode startNode = mMap[mTilePositionList[(int)GridState.Source][0]];
                DSLNode endNode = mMap[mTilePositionList[(int)GridState.Dest][0]];
                mAStar = new AStar(graph, startNode, endNode);
            }

            DrawPathfinding(mAStar.ComputePath());
        }
    }

    private void DrawPathfinding(List<Vector3Int> path)
    {
        mPathfindingTilemap.ClearAllTiles();
        for(int i = 1; i < path.Count - 1; i++)
        {
            mPathfindingTilemap.SetTile(path[i], mEdgeTile);
        }
        
    }

    private bool canMakePathfinding()
    {
        return mTileUsageCounter[(int)GridState.Source] == 1 && mTileUsageCounter[(int)GridState.Dest] == 1;
    }

    private Dictionary<DSLNode, List<DSLNode>> getGraphFromMap()
    {
        Dictionary<DSLNode, List<DSLNode>> temp = new Dictionary<DSLNode, List<DSLNode>>();

        int halfMapSizeX = TileMapSize.Item1 / 2;
        int halfMapSizeY = TileMapSize.Item2 / 2;
        foreach (DSLNode node in mMap.Values)
        {
            temp.Add(node, new List<DSLNode>());
        }

        for (int i = -halfMapSizeX; i < halfMapSizeX; i++)
        {
            for (int j = -halfMapSizeY; j < halfMapSizeY; j++)
            {
                DSLNode currentNode = mMap[new Vector3Int(i, j, 0)];
                if (currentNode.NodeState == GridState.Wall)
                {
                    continue;
                }

                List<DSLNode> neighbors = getNeighbor(currentNode);
                temp[currentNode].AddRange(neighbors);
            }
        }

        return temp;
    }

    //Temp
    private int[] mNextX = new int[4] { 1, -1, 0, 0 };
    private int[] mNextY = new int[4] { 0, 0, -1, 1 };

    private List<DSLNode> getNeighbor(DSLNode node)
    {
        List<DSLNode> neighbors = new List<DSLNode>();

        for (int i = 0; i < mNextX.Length; i++)
        {
            int nextX = node.Pos.x + mNextX[i];
            int nextY = node.Pos.y + mNextY[i];
            if (!isCellInMap(new Vector3Int(nextX, nextY, 0)))
            {
                continue;
            }

            DSLNode neighbor = mMap[new Vector3Int(nextX, nextY, 0)];
            if (neighbor.NodeState != GridState.Wall)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private void processOnMouseInput()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = mTilemap.WorldToCell(mouseWorldPos);
        if (!cellPos.Equals(mLastChangedPreviewTilePos))
        {
            mPreviewTilemap.SetTile(mLastChangedPreviewTilePos, null);
        }

        if (!isCellInMap(cellPos))
        {
            return;
        }

        mLastChangedPreviewTilePos = cellPos;
        mPreviewTilemap.SetTile(cellPos, mPreviewTileList[(int)mouseGridState]);
    }

    private void processMouseClickInput()
    {
        if (Input.GetMouseButton(0))
        {
            mPathfindingTilemap.ClearAllTiles();
            
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = mTilemap.WorldToCell(mouseWorldPos);
            if (!isCellInMap(cellPos))
            {
                return;
            }

            setTile(cellPos, mouseGridState);
        }
    }

    private void setTile(Vector3Int cellPos, GridState newState)
    {
        GridState nowGridState = mMap[cellPos].NodeState;
        if (!isCellInMap(cellPos) || nowGridState == newState)
        {
            return;
        }

        if (mTileUsageCounter[(int)newState] >= mTileUsageLimitList[(int)newState])
        {
            setTile(mTilePositionList[(int)newState][0], mDefaultGridState);
        }

        mTileUsageCounter[(int)nowGridState]--;
        mTilePositionList[(int)nowGridState].Remove(cellPos);

        mTileUsageCounter[(int)newState]++;
        mTilePositionList[(int)newState].Add(cellPos);
        mTilemap.SetTile(cellPos, mTileList[(int)newState]);
        mMap[cellPos].NodeState = newState;
    }

    private void processInputGridState()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeGridState(GridState.Path);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeGridState(GridState.Source);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeGridState(GridState.Wall);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ChangeGridState(GridState.Dest);
        }
    }


    public void ChangeGridState(GridState state)
    {
        mouseGridState = state;
    }

    private bool isCellInMap(Vector3Int cellPos)
    {
        return cellPos.x >= -TileMapSize.Item1 / 2 && cellPos.x < TileMapSize.Item1 / 2 &&
               cellPos.y >= -TileMapSize.Item2 / 2 && cellPos.y < TileMapSize.Item2 / 2;
    }
}