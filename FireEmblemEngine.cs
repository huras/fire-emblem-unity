using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fire_Emblem_Engine;

public class FireEmblemEngine : MonoBehaviour {

    // Use this for initialization
    public int mapHeight, mapWidth;
	void Awake () {
        BuildStartingGrid(mapHeight, mapWidth);
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
            if (!onOddTile)
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
            if (onOddTile)
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
            if (!onOddTile)
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
            if (onOddTile)
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

        Vector3 oldcursorPosition = cursorPosition;
        cursorPosition += movimentation;
        if(cursorPosition.x >= mapHeight)
        {
            cursorPosition.x = mapHeight - 1;
        }
        if (cursorPosition.x < 0)
        {
            cursorPosition.x = 0;
        }

        if (cursorPosition.z >= mapWidth)
        {
            cursorPosition.z = mapWidth - 1;
        }
        if (cursorPosition.z < 0)
        {
            cursorPosition.z = 0;
        }

        if ((cursorPosition.x) % 2 == 1)
        {
            mapCursor.transform.position = Vector3.Lerp(mapCursor.transform.position, cursorPosition + new Vector3(0, 0, 0.5f), 5.6f * Time.deltaTime);
        }
        else
        {
            mapCursor.transform.position = Vector3.Lerp(mapCursor.transform.position, cursorPosition, 6.6f * Time.deltaTime);
        }
        UpdateCursorHUD();
    }

    public Transform tilegridParent;
    public GameObject tilePrefab;
    public TileController[,] tileGrid;

    public Material grassMaterial;
    void BuildStartingGrid(int width, int height)
    {
        tileGrid = new TileController[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileController newTile = GameObject.Instantiate(tilePrefab, new Vector3(x, 0, y + ((x % 2 == 0) ? 0 : 0.5f)), Quaternion.identity).GetComponent<TileController>();
                newTile.renderer.material = grassMaterial;
                tileGrid[x, y] = newTile;
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

    public enum MapHUDMessages { Move = 0, Attack = 1 };
    public void ProcessHUDMessage(int msg) {

    }
}
