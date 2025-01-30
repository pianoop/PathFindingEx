using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

public class TileManager : MonoBehaviour
{
    public Tuple<int, int> TileMapSize;

    public enum GridState
    {
        Path,
        Source,
        Wall,
        Dest,

        Length
    }

    public GridState mouseGridState;
    [SerializeField] GridState mDefaultGridState;
    [SerializeField] Tilemap mTilemap;
    [SerializeField] Vector3Int mTileOffset;
    [SerializeField] List<Tile> tileList;

    private List<List<GridState>> mMap;

    // List index = (int)GridState 
    private List<int> mTileUsageLimitList;
    private List<int> mTileUsageCounter;
    private List<List<Vector3Int>> mTilePositionList;

    void Start()
    {
        TileMapSize = new Tuple<int, int>(20, 20);
        mTileOffset = new Vector3Int(TileMapSize.Item1 / 2, TileMapSize.Item2 / 2, 0);
        mouseGridState = mDefaultGridState;
        mMap = new List<List<GridState>>();
        for (int i = 0; i < TileMapSize.Item1; i++)
        {
            mMap.Add(new List<GridState>());
            for (int j = 0; j < TileMapSize.Item2; j++)
            {
                mMap[i].Add(mDefaultGridState);
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
        processMouseInput();
    }

// TODO: on mouse 시에 임시로 타일이 변경되는 것처럼 보이는 기능

    private void processMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int arrayCellPos = mTilemap.WorldToCell(mouseWorldPos);

            setTile(arrayCellPos, mouseGridState);
        }
    }

    private void setTile(Vector3Int cellPos, GridState newState)
    {
        Tuple<int, int> arrayPos = getMapArrayPos(cellPos);
        GridState nowGridState = mMap[arrayPos.Item1][arrayPos.Item2];
        if (nowGridState == newState)
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
        mTilemap.SetTile(cellPos, tileList[(int)newState]);
        mMap[arrayPos.Item1][arrayPos.Item2] = newState;
    }

    private void processInputGridState()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeGridState(GridState.Path);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeGridState(GridState.Wall);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeGridState(GridState.Source);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ChangeGridState(GridState.Dest);
        }
    }

    private Tuple<int, int> getMapArrayPos(Vector3Int cellPos)
    {
        Vector3Int arrayCellPos = cellPos + mTileOffset;
        return new Tuple<int, int>((int)arrayCellPos.x, (int)arrayCellPos.y);
    }

    public void ChangeGridState(GridState state)
    {
        mouseGridState = state;
    }
}