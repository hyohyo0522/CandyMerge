using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hyo.Util
{
    public class TouchHandler : IInputHandlerBase
    {
        bool IInputHandlerBase.isInputDown
        {
            get
            {
                if (Input.touchCount <= 0)
                {
                    return false;
                }
                return Input.GetTouch(0).phase == TouchPhase.Began;
            }
        }

        bool IInputHandlerBase.isInputUp
        {
            get
            {
                if (Input.touchCount <= 0)
                {
                    return false;
                }
                return Input.GetTouch(0).phase == TouchPhase.Ended;
            }
        }

        Vector2 IInputHandlerBase.inputPosition
        {
            get
            {
                if (Input.touchCount <= 0)
                {
                    return Vector2.zero;
                }
                return Input.GetTouch(0).position;
            }
        }
    }
}