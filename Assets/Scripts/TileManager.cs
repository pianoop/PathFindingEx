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
        Dest
    }
    
    public GridState mouseGridState;
    [SerializeField] Tilemap mTilemap;
    [SerializeField] Vector3Int mTileOffset = new Vector3Int(10, 10, 0); 
    [SerializeField] List<Tile> tileList;
    private List<int> mTileUsageLimitList;

    void Start()
    {
        mTileUsageLimitList = new List<int>()
        {
            40, 1, 40, 1
        };
    }
    
    void Update()
    {
        processInputGridState();
        processMouseInput();
    }

    private void processMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = mTilemap.WorldToCell(mouseWorldPos);
            
            mTilemap.SetTile(cellPos, tileList[(int)mouseGridState]);
        }
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
