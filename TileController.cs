using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Fire_Emblem_Engine
{
    public class TileController : MonoBehaviour
    {
        public MeshRenderer textureRenderer;
        public Transform unitsParent;

        public TileController neighbourTileUp = null, neighbourTileDown = null, neighbourTileLeft = null, neighbourTileRight = null;

        public List<UnitController> units = new List<UnitController>();
        float width = 1, height = 1;

        public void PlaceUnit(UnitController unit)
        {            
            if(units.Count < 9)
            {                
                units.Add(unit);
                unit.transform.parent = unitsParent;
                unit.transform.localPosition = Vector3.zero;
                unit.transform.localRotation = Quaternion.identity;
                unit.gameObject.name = units.Count.ToString();

                ReplaceAllUnits();

                //int x = units.Count / 3 - 1;
                //int z = units.Count % 3 - 1;
            }
        }
        public void ReplaceAllUnits()
        {
            switch (units.Count)
            {
                case 1:
                    {
                        int x = 0, z = 0;
                        units[0].transform.localPosition = new Vector3(x * width / 4, 0, z * height / 4);
                    }
                    break;
                case 9:
                    {
                        for(int i = 0; i < units.Count; i++)
                        {
                            int x = 0, z = 0;
                            if (i == 0)
                            {
                                x = -1;
                                z = 1;
                            }
                            else if (i == 1)
                            {
                                x = 0;
                                z = 1;
                            }
                            else if (i == 2)
                            {
                                x = 1;
                                z = 1;
                            }

                            else if (i == 3)
                            {
                                x = -1;
                                z = 0;
                            }
                            else if (i == 4)
                            {
                                x = 0;
                                z = 0;
                            }
                            else if (i == 5)
                            {
                                x = 1;
                                z = 0;
                            }

                            if (i == 6)
                            {
                                x = -1;
                                z = -1;
                            }
                            else if (i == 7)
                            {
                                x = 0;
                                z = -1;
                            }
                            else if (i == 8)
                            {
                                x = 1;
                                z = -1;
                            }

                            units[i].transform.localPosition = new Vector3(x * width / 4, 0, z * height / 4);
                        }
                    }
                    break;
            }
        }

        public void InitialiseTile()
        {
            hasTileUp = neighbourTileUp != null;
            hasTileDown = neighbourTileDown != null;
            hasTileLeft = neighbourTileLeft != null;
            hasTileRight = neighbourTileRight != null;
        }
    }
}
