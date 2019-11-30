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
            ChangeHUDState(HUDState.Navigating);
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
        public void ChangeHUDState(HUDState newHUDState)
        {
            switch (newHUDState)
            {
                case HUDState.Navigating:
                    {                        
                        SetActionsMenu(false);
                        
                        mayMoveCursor = true;

                        currentHUDState = newHUDState;
                    }
                    break;
                case HUDState.ChosingUnitAction:
                    {
                        if(lastProcessedTile.units.Count > 0)
                        {
                            paintCursor(cursorColorChoosingActions);
                            mayMoveCursor = false;

                            SetActionsMenuByTile(lastProcessedTile);

                            currentHUDState = newHUDState;

                            Actions[] defaultActions = { Actions.Move, Actions.Attack };
                            setActionButtons(defaultActions);
                        }
                    }
                    break;
                case HUDState.ChosingUnitsMovement:
                    {
                        mayMoveCursor = true;
                        SetActionsMenu(lastProcessedTile);
                        paintMovimentationGrid(lastProcessedTile, new Color(9 / 255f, 0, 1, 58 / 255f));
                        currentHUDState = newHUDState;
                    }
                    break;
            }
        }
        void paintMovimentationGrid(TileController tile, Color color)
        {
            UnitController slowerUnitInTile = findSlowestUnitInTile(tile);
            if (slowerUnitInTile != null)
            {
                int radius = slowerUnitInTile.moveRadius;
                paintTileRecursively(tile, 0, radius, color);
            }
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
            actionBtns.Clear();
        }
        public enum Actions { Move, Attack };
        void setActionButtons(Actions[] actions)
        {
            cleanActionsBtn();
            actionsMenuBG.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 164 * actions.Length);
            for (int i = 0; i < actions.Length; i++)
            {
                Debug.Log(actions[i].ToString());
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
                default: return FireEmblemEngine.MapHUDMessages.None;
            }
        }

        public Vector3 cursorPosition = Vector3.zero;
        bool mayMoveCursor = false;
        public Vector3 MoveCursor(Vector3 movimentation, int mapHeight, int mapWidth)
        {            
            if(mayMoveCursor)
            {
                cursorPosition += movimentation;
                if (cursorPosition.x >= mapHeight)
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

        TileController lastProcessedTile = null;
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
                                    ChangeHUDState(HUDState.ChosingUnitAction);
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
                                    ChangeHUDState(HUDState.ChosingUnitsMovement);
                                }
                                break;
                            case FireEmblemEngine.MapHUDMessages.Attack:
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
