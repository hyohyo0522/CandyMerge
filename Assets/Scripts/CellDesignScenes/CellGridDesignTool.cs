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
    /// �� ��ũ��Ʈ�� �ۼ��� ������ 
    /// ������ 3-match �������� ���� ��,
    /// ��ü������ ������ ����� ��Ƴ��� ���� ��ġ�� ������ ���鸸
    /// ������ ������ ���� json���Ϸ� �̾Ƴ��� ���� ��������ϴ�!
    /// </summary>

    #region �̱������(+Awake �Լ�)

    public static CellGridDesignTool Instance = null;
    private void Awake()
    {
        if (Instance == null) //instance�� null. ��, �ý��ۻ� �����ϰ� ���� ������
        {
            Instance = this; //���ڽ��� instance�� �־��ݴϴ�.
            DontDestroyOnLoad(gameObject); //OnLoad(���� �ε� �Ǿ�����) �ڽ��� �ı����� �ʰ� ����
        }
        else
        {
            if (Instance != this) //instance�� ���� �ƴ϶�� �̹� instance�� �ϳ� �����ϰ� �ִٴ� �ǹ�
                Destroy(this.gameObject); //�� �̻� �����ϸ� �ȵǴ� ��ü�̴� ��� AWake�� �ڽ��� ����
        }
    }

    #endregion �̱������(+Awake �Լ�)


    //���̽� ������ ����
    string jsonName = "CellGridFor21Level"; // Start �Լ����� Constants.cs�� ����� ������ �ѹ� �� ������ ����
    string jsonPath;
    List<Vector3> savedInfo;

    //��� �׸��弿 ����
    public GameObject HexGrid_Prefab; // ������ ���ؼ� ����


    //UI����
    public TMP_Text msgText;
    string guideMsg = "Toch HexCells  that will be activated in the game. Selected cells are green.";
    string saveMsg = "Json File Saved : ";
    string saveErrorMsgOfNoData = "No Save Data";
    WaitForSeconds TermToComebackDefaultMsg = new WaitForSeconds(5f);

    //����Ʈ ���۰���
    bool hasData = false;






    void Start()
    {
        ////Newtonsoft.Json�� Vector3Ÿ�� normalized ���� �ذ� ���� �ڵ�
        //JsonSerializerSettings setting = new JsonSerializerSettings();
        //setting.Formatting = Formatting.Indented;
        //setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        jsonName = Constants.JsonNameOfGridCellDesign;
        jsonPath = Path.Combine(Application.dataPath + "/Data", jsonName);
        jsonPath = $"{jsonPath}.txt";
        Debug.Log($"���̽� ������ ���� �ּ� :{jsonPath}");
        saveMsg += jsonPath;
        msgText.text = guideMsg;
        hasData = false;
        savedInfo = null;



    }

    private void Update()
    {
        //m_grid.Update();
    }

    public void CellGrigDesignJsonSave(string Description = null) // TODO : Description ���������� �� �ִ� ��� �߰�.
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
        File.WriteAllText(jsonPath, json); //>> �̰� �� �ȵ�..
        //Constants.JsonFileOfCellGridDesigne = json;
        //PlayerPrefs.SetString("DesignCell", json);
        Debug.Log($"���̽� :{json}");


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