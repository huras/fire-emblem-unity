using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fire_Emblem_Engine
{
    public class MapCanvasHUDController : MonoBehaviour
    {
        public Text unitylabel;
        public Transform actionsMenu;
        public MeshRenderer[] cursorBars;
        public Color cursorColorsEmptyField;
        public Color cursorColorsUnit;
        void paintCursor(Color color)
        {
            for(int i = 0; i < cursorBars.Length; i++)
            {
                MeshRenderer item = cursorBars[i];
                item.material.color = color;
            }
        }

        public enum HUDState { Navigating, ChosingUnitsMovement, MovingUnit, ChosingWhoToAttack, ChosingAttackStrategies };
        public HUDState currentHUDState = HUDState.Navigating;
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
            #region Defines actions menu
            if (tile.units.Count == 0)
            {
                actionsMenu.gameObject.SetActive(false);
            }
            else if (tile.units.Count == 1)
            {
                actionsMenu.gameObject.SetActive(true);
            }
            else if (tile.units.Count > 1)
            {
                actionsMenu.gameObject.SetActive(true);
            }
            #endregion
            lastProcessedTile = tile;
        }
        public void ProcessHUDMessages(FireEmblemEngine.MapHUDMessages msg)
        {
            switch (msg)
            {
                case FireEmblemEngine.MapHUDMessages.Move:
                    {

                    }break;
                case FireEmblemEngine.MapHUDMessages.Attack:
                    {

                    }
                    break;
            }
        }
    }

}
