using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Hyo.HexItems
{
    public class HexCell : MonoBehaviour
    {

        Vector3 coordinate; //HexCell¿« ¿Œµ¶Ω∫∞™
        public Vector3 Coordinate { get { return coordinate; } }
        public TMP_Text cellName;


        SpriteRenderer sr;
        public bool isActiveCell { get; private set; } //»∞º∫ºø¿Œ∞°?

        HexGrid _myGrid;


        public void Init(Vector3 myCoordinate, HexGrid myGrid)
        {
            coordinate = myCoordinate;
            _myGrid = myGrid;
            sr = GetComponent<SpriteRenderer>();
        }




        public void ChangeColor(bool change, bool IsGameScene = false)
        {

            if (!IsGameScene)
            {
                if (isActiveCell)
                {
                    sr.material.color = change ? Color.red : Color.green;
                }
                else
                {
                    sr.material.color = change ? Color.red : Color.white;
                }
            }

        }

        public void IsActivate(bool value, bool IsGameScene= false)
        {
            // ºø≈∏¿‘ ±‚∫ª 
            isActiveCell = value;

            if (!IsGameScene)
            {
                sr.material.color = value ? Color.green : Color.white;
            }

        }
    }

}

