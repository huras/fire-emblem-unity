using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fire_Emblem_Engine;

public class FireEmblemEngine : MonoBehaviour {

    // Use this for initialization
    public int mapHeight, mapWidth;
	void Awake () {
        BuildStartingGrid(mapHeight, mapWidth);
        mapHUDController.SetMapInfo(mapHeight, mapWidth, tileGrid);
        cursorPosition = new Vector3(mapHeight / 2, 0, mapWidth / 2);
        DoTesting();
    }
	
	// Update is called once per frame
	void Update () {
        EngineLoop();
    }

    public Transform mapCursor;
    void EngineLoop()
    {
        RunCursor();
    }

    enum CursorState { Idle, Moving, Clicking};
    CursorState currentCursorState = CursorState.Idle;
    public Vector3 cursorPosition = Vector3.zero;
    void RunCursor()
    {
        bool onOddTile = (cursorPosition.x) % 2 == 1;
        Vector3 movimentation = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            if (!onOddTile && tileOffset > 0)
            {
                movimentation += Vector3.right;
            }
            else
            {
                movimentation += Vector3.right + Vector3.forward;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            if (onOddTile && tileOffset > 0)
            {
                movimentation += Vector3.right;
            }
            else
            {
                movimentation += Vector3.right - Vector3.forward;
            }            
        }
        else if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            if (onOddTile && tileOffset > 0)
            {
                movimentation += Vector3.left;
            }
            else
            {
                movimentation += Vector3.left + Vector3.forward;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            if (onOddTile && tileOffset > 0)
            {
                movimentation += Vector3.left;
            }
            else
            {
                movimentation += Vector3.left - Vector3.forward;
            }
        }

        else if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            movimentation += Vector3.forward;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            movimentation -= Vector3.forward;
        }

        else if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            movimentation += Vector3.left;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            movimentation += Vector3.right;
        }
        
        cursorPosition = mapHUDController.MoveCursor(movimentation);

        if ((cursorPosition.x) % 2 == 1)
        {
            mapCursor.transform.position = Vector3.Lerp(mapCursor.transform.position, cursorPosition + new Vector3(0, 0, tileOffset), 5.6f * Time.deltaTime);
        }
        else
        {
            mapCursor.transform.position = Vector3.Lerp(mapCursor.transform.position, cursorPosition, 6.6f * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            mapHUDController.ProcessHUDMessagesByState(MapHUDMessages.SelectTile);
        }

        UpdateCursorHUD();
    }

    public Transform tilegridParent;
    public GameObject tilePrefab;
    public TileController[,] tileGrid;

    public Material grassMaterial;
    float tileOffset = 0.0f;
    void BuildStartingGrid(int width, int height)
    {
        tileGrid = new TileController[width, height];

        //build grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileController newTile = GameObject.Instantiate(tilePrefab, new Vector3(x, 0, y + ((x % 2 == 0) ? 0 : tileOffset)), Quaternion.identity).GetComponent<TileController>();
                newTile.textureRenderer.material = grassMaterial;
                tileGrid[x, y] = newTile;
                newTile.x = x;
                newTile.y = y;
            }
        }

        //assign neighbours
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileController item = tileGrid[x, y];
                if(y > 0)
                {
                    item.neighbourTileUp = tileGrid[x, y - 1];
                } if (y < height - 1)
                {
                    item.neighbourTileDown = tileGrid[x, y + 1];
                }

                if (x > 0)
                {
                    item.neighbourTileLeft = tileGrid[x - 1, y ];
                }
                if (x < width - 1)
                {
                    item.neighbourTileRight = tileGrid[x + 1, y];
                }
            }
        }

        //init tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileController item = tileGrid[x, y];
                item.InitialiseTile();
            }
        }
    }

    void DoTesting()
    {
        for (int i = 0; i < 9; i++)
            SpawnUnit(UnitController.Jobs.Warrior, tileGrid[3, 3]);
        
        SpawnUnit(UnitController.Jobs.Warrior, tileGrid[3, 1]);
        SpawnUnit(UnitController.Jobs.Mage, tileGrid[3, 2]);
        SpawnUnit(UnitController.Jobs.Warrior, tileGrid[1, 6]);
        SpawnUnit(UnitController.Jobs.Mage, tileGrid[5, 5]);
        SpawnUnit(UnitController.Jobs.Warrior, tileGrid[5, 5]);
        SpawnUnit(UnitController.Jobs.Warrior, tileGrid[5, 5]);
    }

    public GameObject warriorPrefab, magePrefab;
    public UnitController SpawnUnit(UnitController.Jobs unitJob, TileController tile)
    {
        switch (unitJob)
        {
            case UnitController.Jobs.Warrior:
                {
                    UnitController retorno = GameObject.Instantiate(warriorPrefab,Vector3.zero, Quaternion.identity).GetComponent<UnitController>();
                    retorno.job = unitJob;

                    tile.PlaceUnit(retorno);
                    return retorno;
                }
                break;
            case UnitController.Jobs.Mage:
                {
                    UnitController retorno = GameObject.Instantiate(magePrefab, Vector3.zero, Quaternion.identity).GetComponent<UnitController>();
                    retorno.job = unitJob;

                    tile.PlaceUnit(retorno);
                    return retorno;
                }
                break;
        }

        return null;
    }

    public MapCanvasHUDController mapHUDController;
    public void UpdateCursorHUD()
    {
        mapHUDController.UpdateCursorInfoHUD(tileGrid[(int)cursorPosition.x, (int)cursorPosition.z]);
    }

    public enum MapHUDMessages { None = -1, Move = 0, Attack = 1, SelectTile = 2, Cancel = 3 };
    public void ProcessHUDMessage(int msg) {
        mapHUDController.ProcessHUDMessagesByState((MapHUDMessages) msg);
    }
}
