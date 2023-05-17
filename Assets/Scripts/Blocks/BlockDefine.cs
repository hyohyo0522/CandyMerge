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

    public enum BlockCandyType // 캔디 이미지 
    {
        NA = -1,   //Not Assigned
        CANYDY_0 = 0,
        CANYDY_1 = 1,
        CANYDY_2 = 2,
        CANYDY_3 = 3,
        CANYDY_4 = 4,
        CANYDY_5 = 5,
        SPECIAL =6, //향후 스페셜 캔디 추가! 위 BlockType이 SPECIAL이면 자동으로 SPECIAL이 되게 만들고 싶다.
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
