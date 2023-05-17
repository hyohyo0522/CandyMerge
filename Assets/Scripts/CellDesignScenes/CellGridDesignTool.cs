using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;  
using TMPro;
using System.Linq;
using Hyo.core;

public class CellGridDesignTool : MonoBehaviour
{
    /// <summary>
    /// 이 스크립트를 작성한 이유는 
    /// 육각형 3-match 게임판을 만들 때,
    /// 전체적으로 육각형 모양을 깔아놓고 그중 터치로 선택한 모양들만
    /// 게임판 제작을 위한 json파일로 뽑아내기 위해 만들었습니다!
    /// </summary>

    #region 싱글톤생성(+Awake 함수)

    public static CellGridDesignTool Instance = null;
    private void Awake()
    {
        if (Instance == null) //instance가 null. 즉, 시스템상에 존재하고 있지 않을때
        {
            Instance = this; //내자신을 instance로 넣어줍니다.
            DontDestroyOnLoad(gameObject); //OnLoad(씬이 로드 되었을때) 자신을 파괴하지 않고 유지
        }
        else
        {
            if (Instance != this) //instance가 내가 아니라면 이미 instance가 하나 존재하고 있다는 의미
                Destroy(this.gameObject); //둘 이상 존재하면 안되는 객체이니 방금 AWake된 자신을 삭제
        }
    }

    #endregion 싱글톤생성(+Awake 함수)


    //제이슨 데이터 관련
    string jsonName = "CellGridFor21Level"; // Start 함수에서 Constants.cs에 저장된 값으로 한번 더 갱신할 것임
    string jsonPath;
    List<Vector3> savedInfo;

    //헥사 그리드셀 관련
    public GameObject HexGrid_Prefab; // 프리팹 통해서 쓰자


    //UI관련
    public TMP_Text msgText;
    string guideMsg = "Toch HexCells  that will be activated in the game. Selected cells are green.";
    string saveMsg = "Json File Saved : ";
    string saveErrorMsgOfNoData = "No Save Data";
    WaitForSeconds TermToComebackDefaultMsg = new WaitForSeconds(5f);

    //리스트 전송관련
    bool hasData = false;






    void Start()
    {
        ////Newtonsoft.Json의 Vector3타입 normalized 문제 해결 위한 코드
        //JsonSerializerSettings setting = new JsonSerializerSettings();
        //setting.Formatting = Formatting.Indented;
        //setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        jsonName = Constants.JsonNameOfGridCellDesign;
        jsonPath = Path.Combine(Application.dataPath + "/Data", jsonName);
        jsonPath = $"{jsonPath}.txt";
        Debug.Log($"제이슨 데이터 파일 주소 :{jsonPath}");
        saveMsg += jsonPath;
        msgText.text = guideMsg;
        hasData = false;
        savedInfo = null;



    }

    private void Update()
    {
        //m_grid.Update();
    }

    public void CellGrigDesignJsonSave(string Description = null) // TODO : Description 설명저장할 수 있는 기능 추가.
    {
        if (savedInfo.Count <= 0)
        {
            ChangeMsgText(saveErrorMsgOfNoData);
            return;
        }


        CellDesignData data = new CellDesignData();
        data.Description = Description;
        data.SavedactivatedCellsInfo = savedInfo;

        string json = JsonUtility.ToJson(data);
        //string json = JsonConvert.SerializeObject(data);
        //FileStream fileStream = new FileStream(jsonPath, FileMode.Create);
        //byte[] bytedata = Encoding.UTF8.GetBytes(json);
        //fileStream.Write(bytedata, 0, bytedata.Length);
        //fileStream.Close();
        File.WriteAllText(jsonPath, json); //>> 이거 왜 안됨..
        //Constants.JsonFileOfCellGridDesigne = json;
        //PlayerPrefs.SetString("DesignCell", json);
        Debug.Log($"제이슨 :{json}");


        ChangeMsgText(saveMsg);

    }

    public void GetDeliveredList(List<Vector3> saved)
    {
        if (saved.Count <= 0)
        {
            ChangeMsgText(saveErrorMsgOfNoData);
            return;
        }

        savedInfo = saved;
        if (!hasData) hasData = true;
        ChangeMsgText(guideMsg);

    }

    void ChangeMsgText(string msg)
    {
        if (msg == guideMsg)
        {
            msgText.text = guideMsg;
            msgText.text += NumberOfActivatedCell();
            return;
        }

        StartCoroutine(ChangeMsgCoroutine(msg));
    }

    IEnumerator ChangeMsgCoroutine(string newMsg)
    {
        msgText.text = newMsg;
        yield return TermToComebackDefaultMsg;
        msgText.text = guideMsg;
        msgText.text += NumberOfActivatedCell();
    }

    string NumberOfActivatedCell()
    {
        string numMsg = null;
        if (savedInfo.Count > 0)
        {
            int Num = savedInfo.Count();
            numMsg = " / Now Activated Cell : " + Num.ToString();
        }

        return numMsg;
    }

}

[System.Serializable]
public class CellDesignData
{
    public string Description;
    public List<Vector3> SavedactivatedCellsInfo = new List<Vector3>();
}

//[System.Serializable]
//public class UJsonTester
//{
//    public Vector3 v3;

//    public UJsonTester() { }

//    public UJsonTester(float f)
//    {
//        v3 = new Vector3(f, f, f);
//    }

//    public UJsonTester(Vector3 v)
//    {
//        v3 = v;
//    }
//}

//public class JsonExample : MonoBehaviour
//{
//    void Start()
//    {
//        UJsonTester jt = new UJsonTester(transform.position);
//        Debug.Log(JsonUtility.ToJson(jt));
//    }

//}