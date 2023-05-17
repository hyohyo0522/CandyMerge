using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hyo.HexItems
{
    public enum BlockType
    {
        EMPTY =0, 
        BASIC =1,
        SPECIAL=2
    }

    public enum BlockCandyType // ĵ�� �̹��� 
    {
        NA = -1,   //Not Assigned
        CANYDY_0 = 0,
        CANYDY_1 = 1,
        CANYDY_2 = 2,
        CANYDY_3 = 3,
        CANYDY_4 = 4,
        CANYDY_5 = 5,
        SPECIAL =6, //���� ����� ĵ�� �߰�! �� BlockType�� SPECIAL�̸� �ڵ����� SPECIAL�� �ǰ� ����� �ʹ�.
    }



    static class BlockMethod
    {
        public static bool IsSafeEqual(this HexBlock block, HexBlock targetBlock)
        {
            if(block == null)
            {
                return false;
            }

            return block.IsEqual(targetBlock);
        }

        
    }
}
