using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hyo.Scriptable
{
    [CreateAssetMenu(menuName = "Candy/SpecialBlock Config", fileName = "SpecialBlockConfig.asset")]
    public class SpecialBlockConfig : ScriptableObject
    {
        public GameObject[] SpecialBlocks;
    }
}

