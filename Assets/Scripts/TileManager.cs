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
    [SerializeField] Vector3Int mTileOffset = new Vector3Int(10, 10, 0); 
    [SerializeField] List<Tile> tileList;
    
    // List index = (int)GridState 
    private List<int> mTileUsageLimitList;
    private List<int> mTileUsageCounter;
    private List<List<Vector3Int>> mTilePositionList;
    
    void Start()
    {
        mTileUsageLimitList = new List<int>()
        {
            40, 1, 40, 1
        };
        
        mDefaultGridState = GridState.Path;
        
        
        mTileUsageCounter = new List<int>();
        mTilePositionList = new List<List<Vector3Int>>();
        for (int i = 0; i < (int)GridState.Length; i++)
        {
            mTileUsageCounter.Add(0);
            mTilePositionList.Add(new List<Vector3Int>());
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
            Vector3Int cellPos = mTilemap.WorldToCell(mouseWorldPos);
            
            setTile(applyCellPosOffset(cellPos), mouseGridState);
        }
    }

    private void setTile(Vector3Int arrayCellPos, GridState state)
    {
        /// 1. 타일 limit 확인(yes -> 2)
        /// 1-1. 초과시 이전 tilePosList를 통해 index:0 타일을 Path타일로 변경(list 정보도 수정)
        /// 2. tile set 후 list에 추가

        if (mTileUsageCounter[(int)state] >= mTileUsageLimitList[(int)state])
        {
            setTile(mTilePositionList[(int)state][0], mDefaultGridState);
            mTilePositionList[(int)state].RemoveAt(0);
            mTileUsageCounter[(int)mDefaultGridState]--;
        }
        
        mTileUsageCounter[(int)state]++;
        mTilePositionList[(int)state].Add(arrayCellPos);
        mTilemap.SetTile(arrayCellPos, tileList[(int)state]);
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

    private Vector3Int applyCellPosOffset(Vector3Int cellPos)
    {
        return cellPos + mTileOffset;
    }

    public void ChangeGridState(GridState state)
    {
        mouseGridState = state;
    }
}
