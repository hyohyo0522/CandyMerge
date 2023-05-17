using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hyo.Util
{
    public interface IInputHandlerBase
    {
        bool isInputDown { get; }
        bool isInputUp { get; }
        Vector2 inputPosition { get; } //Screen(ÇÈ¼¿) ÁÂÇ¥
    }
}


