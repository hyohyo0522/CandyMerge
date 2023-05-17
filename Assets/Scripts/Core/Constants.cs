using UnityEngine;

namespace Hyo.core
{
    public static class Constants
    {
        #region �� ����
        public static string NameOfGameScene = "GameScene"; //���Ӿ�
        public static string NameOfCellGridDesignSceneName = "CellDesignScene"; // �������׸��� ������+Json���� �����
        #endregion

        #region ���� ��ġ
        public static string jsonPathOfGridCellDesign = "D:/UnityProject/CandyParty_LeeHyoJung/Assets/Data"; // CellGridDesignTool.cs���� Path.Path.Combine�̿��ؼ� �޾ƿ� ��
        #endregion ���� ��ġ

        #region �̸�
        public static string JsonNameOfGridCellDesign = "CellGridFor21Level";
        #endregion

        #region ���̽� ���� 
        public static string JsonFileOfCellGridDesigne;
        #endregion ���̽� ���� 

        #region ���� �����
        public static float SwipeDuration = 0.2f;
        public static float DestroyDuraion = 0.2f;
        public static Vector3 errorVector3 = new Vector3(99,99,99);
        
        #endregion


    }
}


