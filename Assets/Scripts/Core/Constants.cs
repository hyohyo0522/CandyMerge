using UnityEngine;

namespace Hyo.core
{
    public static class Constants
    {
        #region 씬 네임
        public static string NameOfGameScene = "GameScene"; //게임씬
        public static string NameOfCellGridDesignSceneName = "CellDesignScene"; // 육각셀그리드 디자인+Json파일 저장씬
        #endregion

        #region 저장 위치
        public static string jsonPathOfGridCellDesign = "D:/UnityProject/CandyParty_LeeHyoJung/Assets/Data"; // CellGridDesignTool.cs에서 Path.Path.Combine이용해서 받아온 값
        #endregion 저장 위치

        #region 이름
        public static string JsonNameOfGridCellDesign = "CellGridFor21Level";
        #endregion

        #region 제이슨 파일 
        public static string JsonFileOfCellGridDesigne;
        #endregion 제이슨 파일 

        #region 지정 상수값
        public static float SwipeDuration = 0.2f;
        public static float DestroyDuraion = 0.2f;
        public static Vector3 errorVector3 = new Vector3(99,99,99);
        
        #endregion


    }
}


