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
    [SerializeField] Vector3Int mTileOffset;
    [SerializeField] List<Tile> mTileList;

    private Dictionary<Vector3Int, DSLNode> mMap;
    private Vector3Int mLastChangedPreviewTilePos;
    
    // List index = (int)GridState 
    private List<int> mTileUsageLimitList;
    private List<int> mTileUsageCounter;
    private List<List<Vector3Int>> mTilePositionList;

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
        for (int i = 0; i < TileMapSize.Item1; i++)
        {
            for (int j = 0; j < TileMapSize.Item2; j++)
            {
                mTilePositionList[(int)mDefaultGridState].Add(new Vector3Int(i, j, 0));
            }
        }
    }

    void Update()
    {
        processInputGridState();
        processMouseClickInput();
        processOnMouseInput();
    }

// TODO: on mouse 시에 임시로 타일이 변경되는 것처럼 보이는 기능

    private void processOnMouseInput()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = mTilemap.WorldToCell(mouseWorldPos);
        if (!cellPos.Equals(mLastChangedPreviewTilePos))
        {
            mPreviewTilemap.SetTile(mLastChangedPreviewTilePos, null);
        }

        if (!isMouseInGrid(cellPos))
        {
            return;
        }

        mLastChangedPreviewTilePos = cellPos;
        mPreviewTilemap.SetTile(cellPos, mTileList[(int)mouseGridState]);
        
        
    }
    private void processMouseClickInput()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = mTilemap.WorldToCell(mouseWorldPos);
            if (!isMouseInGrid(cellPos))
            {
                return;
            }

            setTile(cellPos, mouseGridState);
        }
    }

    private void setTile(Vector3Int cellPos, GridState newState)
    {
        GridState nowGridState = mMap[cellPos].NodeState;
        if (!isMouseInGrid(cellPos) || nowGridState == newState)
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

    private bool isMouseInGrid(Vector3Int cellPos)
    {
        return cellPos.x >= -TileMapSize.Item1 / 2 && cellPos.x < TileMapSize.Item1 / 2 &&
               cellPos.y >= -TileMapSize.Item2 / 2 && cellPos.y < TileMapSize.Item2 / 2;
    }
}