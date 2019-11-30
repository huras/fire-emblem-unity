﻿using System.Collections;
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
        public Transform actionButton;

        public Text unitylabel;
        public Transform actionsMenu;
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
        public Color cursorColorsEmptyField;
        public Color cursorColorsUnit;
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
                        mayMoveCursor = true;
                        SetActionsMenu(false);
                        currentHUDState = newHUDState;
                    }
                    break;
                case HUDState.ChosingUnitAction:
                    {
                        if(lastProcessedTile.units.Count > 0)
                        {
                            mayMoveCursor = false;
                            SetActionsMenuByTile(lastProcessedTile);
                            currentHUDState = newHUDState;
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
            #region Defines cursor
            if (tile.units.Count == 0)
            {
                paintCursor(cursorColorsEmptyField);
            }
            else if (tile.units.Count == 1)
            {
                paintCursor(cursorColorsUnit);
            }
            else if (tile.units.Count > 1)
            {
                paintCursor(cursorColorsUnit);
            }
            #endregion
            #region Defines label
            if (tile.units.Count == 0)
            {
                unitylabel.text = "No units here";
                paintCursor(cursorColorsEmptyField);
            }
            else if (tile.units.Count == 1)
            {
                unitylabel.text = tile.units[0].job.ToString();
                paintCursor(cursorColorsUnit);
            }
            else if (tile.units.Count > 1)
            {
                paintCursor(cursorColorsUnit);
                unitylabel.text = tile.units.Count.ToString();
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
                    jobsDetails += " " + groupsCount[i].ToString() + " " + item.ToString() + " ";
                }
                unitylabel.text += " (" + jobsDetails + ") ";
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
