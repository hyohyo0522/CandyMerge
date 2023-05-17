using UnityEngine;

namespace Hyo.core
{

    public static class HexArchive 
    {


        public static Vector3[] HexDirections = new Vector3[]
        {
            new Vector3(1, 0, -1),  //RIGHT_UP : 0 !
            new Vector3(0, 1, -1),   //UP : 1 !
            new Vector3(-1, 1, 0),  //LEFT_UP : 2
            new Vector3(-1, 0, 1),  //LEFT_DOWN : 3
            new Vector3(0, -1, 1),  //DOWN : 4 !
            new Vector3(1, -1, 0)  //RIGHT_DOWN : 5 !
            
            
        };


    }


}
