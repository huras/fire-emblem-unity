using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fire_Emblem_Engine
{
    public class MapCanvasHUDController : MonoBehaviour
    {
        public void Start()
        {
            SetHUDState(HUDState.Navigating);
        }

        //prefabs
        public GameObject actionButtonPrefab;

        public Text unitCountlabel, unitJobsLabel;
        public Transform actionsMenu;
        public RectTransform actionsMenuBG;
        void SetActionsMenu(bool state)
        {
            actionsMenu.gameObject.SetActive(state);
        }
        void SetActionsMenuByTile(TileController tile)
        {
            #region Defines actions menu
            if (tile.units.Count == 0)
            {
                SetActionsMenu(false);
            }
            else if (tile.units.Count == 1)
            {
                SetActionsMenu(true);
            }
            else if (tile.units.Count > 1)
            {
                SetActionsMenu(true);
            }
            #endregion
        }
        public MeshRenderer[] cursorBars;
        public Color cursorColorsEmptyField, cursorEligibleTile, cursorColorChoosingActions , cursorColorNotEligibleTile;
        void paintCursor(Color color)
        {
            for (int i = 0; i < cursorBars.Length; i++)
            {
                MeshRenderer item = cursorBars[i];
                item.material.color = color;
            }
        }

        public enum HUDState { Navigating, ChosingUnitAction ,ChosingUnitsMovement, MovingUnit, ChosingWhoToAttack, ChosingAttackStrategies };
        public HUDState currentHUDState = HUDState.Navigating;
        public void SetHUDState(HUDState newHUDState)
        {
            bool mayChangeState = false;
            switch (newHUDState)
            {
                case HUDState.Navigating:
                    {                        
                        SetActionsMenu(false);
                        
                        mayMoveCursor = true;

                        mayChangeState = true;
                    }
                    break;
                case HUDState.ChosingUnitAction:
                    {
                        if (lastProcessedTile.units.Count > 0)
                        {
                            tileForActions = lastProcessedTile;
                            paintCursor(cursorColorChoosingActions);
                            paintTileRecursively(tileForActions, 0, 0, new Color(90 / 255f, 90 / 255f, 90 / 255f, 48 / 255f));
                            mayMoveCursor = false;
                            Debug.Log(Time.deltaTime);

                            SetActionsMenuByTile(lastProcessedTile);

                            mayChangeState = true;

                            Actions[] defaultActions = { Actions.Move, Actions.Attack, Actions.Cancel };
                            setActionButtons(defaultActions);
                        }
                    }
                    break;
                case HUDState.ChosingUnitsMovement:
                    {
                        UnitController slowerUnitInTile = findSlowestUnitInTile(lastProcessedTile);
                        if (slowerUnitInTile != null)
                        {
                            int radius = slowerUnitInTile.moveRadius;
                            paintMovimentationGrid(lastProcessedTile, new Color(9 / 255f, 0, 1, 58 / 255f), radius);
                            paintTileRecursively(tileForActions, 0, 0, new Color(90 / 255f, 90 / 255f, 90 / 255f, 48 / 255f));
                            ConfineCursorToRadius(lastProcessedTile, radius);
                        }

                        Actions[] defaultActions = { Actions.Walk, Actions.Cancel };
                        setActionButtons(defaultActions);

                        mayMoveCursor = true;
                        SetActionsMenu(lastProcessedTile);
                        mayChangeState = true;
                    }
                    break;
            }

            if (mayChangeState)
            {
                LeaveHUDState();
                currentHUDState = newHUDState;
            }
        }
        public void LeaveHUDState()
        {
            switch (currentHUDState)
            {
                case HUDState.ChosingUnitAction:
                    {
                        //tileForActions = null;
                    }
                    break;
                case HUDState.ChosingUnitsMovement:
                    {
                        Unconfine();
                        UnitController slowerUnitInTile = findSlowestUnitInTile(tileForActions);
                        if (slowerUnitInTile != null)
                        {
                            int radius = slowerUnitInTile.moveRadius;
                            paintMovimentationGrid(tileForActions, new Color(0,0,0,0), radius);
                        }
                    }
                    break;
            }
        }
        void paintMovimentationGrid(TileController tile, Color color, int radius)
        {
            paintTileRecursively(tile, 0, radius, color);
        }
        void paintTileRecursively(TileController tile, int distanceFromCenter, int maxDistanceFromCenter, Color color)
        {
            tile.tintTileRenderer.gameObject.SetActive(true);
            tile.tintTileRenderer.material.color = color;
            if (distanceFromCenter + 1 <= maxDistanceFromCenter)
            {
                if(tile.hasTileUp)
                    paintTileRecursively(tile.neighbourTileUp, distanceFromCenter + 1, maxDistanceFromCenter, color);
                if (tile.hasTileDown)
                    paintTileRecursively(tile.neighbourTileDown, distanceFromCenter + 1, maxDistanceFromCenter, color);
                if (tile.hasTileLeft)
                    paintTileRecursively(tile.neighbourTileLeft, distanceFromCenter + 1, maxDistanceFromCenter, color);
                if (tile.hasTileRight)
                    paintTileRecursively(tile.neighbourTileRight, distanceFromCenter + 1, maxDistanceFromCenter, color);
            }
        }

        List<ActionButton> actionBtns = new List<ActionButton>();
        void cleanActionsBtn()
        {
            for(int i = 0; i < actionBtns.Count; i++)
            {
                GameObject.Destroy(actionBtns[i].gameObject);
            }
            actionBtns.Clear();
        }
        public enum Actions { Move, Attack, Walk, Cancel };
        void setActionButtons(Actions[] actions)
        {
            cleanActionsBtn();
            actionsMenuBG.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 164 * actions.Length);
            for (int i = 0; i < actions.Length; i++)
            {
                ActionButton newActionBtn = createActionButton(actions[i].ToString(), getMessageByAction(actions[i]), i);
                actionBtns.Add(newActionBtn);
            }
        }
        ActionButton createActionButton(string label, FireEmblemEngine.MapHUDMessages msg, int btnIndex)
        {
            GameObject actBtnObj = GameObject.Instantiate(actionButtonPrefab, Vector3.zero, Quaternion.identity, actionsMenu);
            ActionButton actBtn = actBtnObj.GetComponent<ActionButton>();
            actBtn.btn.onClick.AddListener(() => ProcessHUDMessagesByState(msg));
            actBtn.text.text = label;
            actBtn.rect.localPosition = new Vector3(btnIndex * 167 + 5, 5, 0);
            return actBtn;
        }
        FireEmblemEngine.MapHUDMessages getMessageByAction(Actions action)
        {
            switch (action)
            {
                case Actions.Move:
                    {
                        return FireEmblemEngine.MapHUDMessages.Move;
                    }
                case Actions.Attack:
                    {
                        return FireEmblemEngine.MapHUDMessages.Attack;
                    }
                case Actions.Cancel:
                    {
                        return FireEmblemEngine.MapHUDMessages.Cancel;
                    }
                case Actions.Walk:
                    {
                        return FireEmblemEngine.MapHUDMessages.SelectTile;
                    }
                default: return FireEmblemEngine.MapHUDMessages.None;
            }
        }

        public Vector3 cursorPosition = Vector3.zero;
        bool mayMoveCursor = false;

        public void SetCursor(Vector3 newPosition)
        {
            MoveCursor(newPosition - cursorPosition);
        }
        int mapHeight, mapWidth;
        TileController[,] tileMap;
        public void SetMapInfo(int mapHeight,  int mapWidth, TileController[,] tileMap)
        {
            this.mapHeight = mapHeight;
            this.mapWidth = mapWidth;
            this.tileMap = tileMap;
        }
        public Vector3 MoveCursor(Vector3 movimentation)
        {            
            if(mayMoveCursor)
            {
                Vector3 cursorNewPosition = cursorPosition + movimentation;
                if (cursorNewPosition.x >= mapHeight)
                {
                    cursorNewPosition.x = mapHeight - 1;
                }
                if (cursorNewPosition.x < 0)
                {
                    cursorNewPosition.x = 0;
                }

                if (cursorNewPosition.z >= mapWidth)
                {
                    cursorNewPosition.z = mapWidth - 1;
                }
                if (cursorNewPosition.z < 0)
                {
                    cursorNewPosition.z = 0;
                }

                bool mayChangePosition = true;
                if (isConfined)
                {
                    if (!confinementArea.Contains(tileMap[(int)cursorNewPosition.x, (int)cursorNewPosition.z]))
                    {
                        mayChangePosition = false;
                    }
                }

                if(mayChangePosition)
                cursorPosition = cursorNewPosition;
            }

            return cursorPosition;
        }
        UnitController findSlowestUnitInTile(TileController tile)
        {
            int slowestUnitIndex = -1;
            for (int i = 0; i < tile.units.Count; i++)
            {
                if (slowestUnitIndex == -1)
                {
                    slowestUnitIndex = i;
                } else
                {
                    if (tile.units[slowestUnitIndex].moveRadius < tile.units[i].moveRadius)
                    {
                        slowestUnitIndex = i;
                    }
                }
            }

            if (slowestUnitIndex == -1)
            {
                return null;
            } else
            {
                return tile.units[slowestUnitIndex];
            }
        }
        public bool isConfined = false;
        List<TileController> confinementArea = new List<TileController>();
        void ConfineCursorToRadius(TileController center, int radius)
        {
            isConfined = true;
            RecursiveConfineTiles(center, radius, 0);
        }
        void RecursiveConfineTiles(TileController currentTile, int maxRadius, int currentRadius)
        {
            if (!confinementArea.Contains(currentTile))
                confinementArea.Add(currentTile);

            if(currentRadius + 1 <= maxRadius)
            {
                if (currentTile.hasTileUp)
                    RecursiveConfineTiles(currentTile.neighbourTileUp, maxRadius, currentRadius + 1);
                if (currentTile.hasTileDown)
                    RecursiveConfineTiles(currentTile.neighbourTileDown, maxRadius, currentRadius + 1);
                if (currentTile.hasTileLeft)
                    RecursiveConfineTiles(currentTile.neighbourTileLeft, maxRadius, currentRadius + 1);
                if (currentTile.hasTileRight)
                    RecursiveConfineTiles(currentTile.neighbourTileRight, maxRadius, currentRadius + 1);
            }
        }
        void Unconfine()
        {
            isConfined = false;
            confinementArea.Clear();
        }

        //HUD Messages
        public TileController lastProcessedTile = null;
        public TileController tileForActions = null;
        public void UpdateCursorInfoHUD(TileController tile)
        {
            #region Defines label
            if (tile.units.Count == 0)
            {
                unitCountlabel.text = "0";
                unitJobsLabel.text = "";
            }
            else if (tile.units.Count == 1)
            {
                unitCountlabel.text = tile.units.Count.ToString();
                unitJobsLabel.text = tile.units[0].job.ToString();
                
            }
            else if (tile.units.Count > 1)
            {
                unitCountlabel.text = tile.units.Count.ToString();
                List<UnitController.Jobs> jobsGroup = new List<UnitController.Jobs>();
                List<int> groupsCount = new List<int>();

                for (int i = 0; i < tile.units.Count; i++)
                {
                    UnitController item = tile.units[i];
                    if (!jobsGroup.Contains(item.job))
                    {
                        groupsCount.Add(1);
                        jobsGroup.Add(item.job);
                    }
                    else
                    {
                        groupsCount[jobsGroup.IndexOf(item.job)] += 1;
                    }
                }

                string jobsDetails = "";
                for (int i = 0; i < jobsGroup.Count; i++)
                {
                    UnitController.Jobs item = jobsGroup[i];
                    jobsDetails += " " + groupsCount[i].ToString() + " " + item.ToString();
                    if(groupsCount[i] > 1)
                    {
                        jobsDetails += "s";
                    }
                    jobsDetails += " ";
                }
                unitJobsLabel.text = " (" + jobsDetails + ") ";
            }
            #endregion

            #region Defines cursor color
            if (lastProcessedTile)
            {
                if (currentHUDState == HUDState.Navigating)
                {
                    if (lastProcessedTile.units.Count == 0)
                    {
                        paintCursor(cursorColorsEmptyField);
                    }
                    else if (lastProcessedTile.units.Count >= 1)
                    {
                        paintCursor(cursorEligibleTile);
                    }
                }
                else if (currentHUDState == HUDState.ChosingUnitsMovement)
                {
                    if (lastProcessedTile.units.Count == 0)
                    {
                        paintCursor(cursorEligibleTile);
                    }
                    else if (lastProcessedTile.units.Count >= 1)
                    {
                        paintCursor(cursorColorNotEligibleTile);
                    }
                }
            }
            #endregion

            lastProcessedTile = tile;
        }
        public void ProcessHUDMessagesByState(FireEmblemEngine.MapHUDMessages msg)
        {
            switch (currentHUDState)
            {
                case HUDState.Navigating:
                    {
                        switch (msg)
                        {
                            case FireEmblemEngine.MapHUDMessages.SelectTile:
                                {
                                    SetHUDState(HUDState.ChosingUnitAction);
                                }
                                break;
                        }
                    }
                    break;
                case HUDState.ChosingUnitAction:
                    {
                        switch (msg)
                        {
                            case FireEmblemEngine.MapHUDMessages.Move:
                                {
                                    SetHUDState(HUDState.ChosingUnitsMovement);
                                }
                                break;
                            case FireEmblemEngine.MapHUDMessages.Attack:
                                {

                                }
                                break;
                            case FireEmblemEngine.MapHUDMessages.Cancel:
                                {
                                    SetHUDState(HUDState.Navigating);
                                }
                                break;
                        }
                    }
                    break;
                case HUDState.ChosingUnitsMovement:
                    {
                        switch (msg)
                        {
                            case FireEmblemEngine.MapHUDMessages.Cancel:
                                {
                                    lastProcessedTile = tileForActions;
                                    SetCursor(new Vector3(tileForActions.x, 0, tileForActions.y));
                                    SetHUDState(HUDState.ChosingUnitAction);
                                }
                                break;
                            case FireEmblemEngine.MapHUDMessages.SelectTile:
                                {

                                }
                                break;
                        }
                    }
                    break;
            }
        }
    }
}
