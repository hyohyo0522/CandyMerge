using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hyo.Scriptable
{
    [CreateAssetMenu(menuName = "Candy/Block Config", fileName = "NormalBlockConfig.asset")]
    public class NormalBlockConfig : ScriptableObject
    {
        public Sprite[] NormalBlocks;
    }

}

