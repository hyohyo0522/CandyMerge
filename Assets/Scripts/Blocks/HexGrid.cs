using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;   
using Hyo.Util;
using Hyo.core;
using UnityEngine.SceneManagement;


namespace Hyo.HexItems
{
    public class HexGrid : MonoBehaviour
    {

        // �㱸������ 
        private int mapWidth = 4;
        private int mapHeight = 4;
        private float offsetXforGid = 0f;
        //private float offsetYforGid = 0.48f;
        public Transform gridTransform;

        // ��缿����
        public GameObject hexPrebab;
        private float hexRadius; // ���������� ���� ������ �� ���� ����ؼ� �־���� 
        float effectForHexRadius = 1.00f; //�������� �����־� ��簣�� ������ �����Ѵ�. 


        // ���� ����
        HexBlockController m_BlockController;


        #region ��� Coordinate ���� ���� 
        // [TODO] �� private�� ����� �ܺο����� �Լ��θ� ���� �޾ƿ��� �����?
        // ��ü ���׸��弿 ���� ����Ʈ(���׸�������ξ����� �����)
        private Dictionary<Vector3, HexCell> totalgrid = new Dictionary<Vector3, HexCell>();
        public Dictionary<Vector3, HexCell> TotalGrids { get { return totalgrid; } }


        // Ȱ��ȭ �� ��ǥ�� ���� ����Ʈ(Json �����͸� �ְ�޴� �뵵 + a)
        private List<Vector3> activatedCells = new List<Vector3>(); // Ȱ�������� �׸���� ��ųʸ��� Ű�� �Ǵ� Ű���� �����Ѵ�. 

        // ���ӽſ����� activatedCells �� ���� ���� ����������, public���� �����ؼ� �ܺο��� �� �� �ֵ��� �� 
        public List<Vector3> gameGridCoordinates = new List<Vector3>();

        //��ǥ���� ���� ����ġ ������ Dic  >> ��ǥ�� ��ġ ���� vector3�ϴϱ� �򰥷��� ��ġ�� �ϴ� vector2��, ����ġ�� ����ġ �����ؼ� ����� ����!!
        private Dictionary<Vector3, Vector2> cellPosition = new Dictionary<Vector3, Vector2>();


        // ���� ���ӿ� ���̴� ���׸���
        private Dictionary<Vector3, HexCell> gamegrid = new Dictionary<Vector3, HexCell>();
        public Dictionary<Vector3, HexCell> GameGrids { get { return gamegrid; } }

        #endregion ��� Coordinate ���� ����

        #region ��ġ/�������� ���� (Input)

        InputManager _inputmanger;
        bool m_cTouchDown; // �Է»��� ó�� �÷���, ��ȿ�� ���� ������ ��� true;
        Vector3 m_DownCoordinate; // ���� �� ��ǥ��
        Vector3 m_TouchPos; // ��ġ�� ��ġ (�׸��� ���� ������ǥ)


        #endregion ��ġ/�������� ���� (Input)

        #region �� ���� ���� > �� ��ũ��Ʈ �۵��� ����

        //TODO ������ �� �̸��� bool������ �׸����� ���ӻ� �۵��� �и�������, ���߿��� ��ũ��Ʈ�� �ƿ� �и��ع�����! (����߽�.cs/�׸���Info.cs/���ӻ��۵�.cs)
        bool IsGameScene = false;
        Scene scene;

        #endregion �� ���� ���� > �� ��ũ��Ʈ �۵��� ����

        #region ���� ������ (NOT NULL�������� ���� ��)
        Vector3 ErrorVector3 = new Vector3(-99, -99, -99);
        #endregion �� ������ (NOT NULL�������� ���� ��)





        // Start is called before the first frame update
        public void Start()
        {
            gridTransform = GetComponent<Transform>();
            //���������� ������ �����Ͽ� �������� ��������
            hexRadius = (hexPrebab.GetComponent<BoxCollider2D>().size.x / 2) * effectForHexRadius;

            //�׸�����ġ�� ������ ������ ���� 
            Vector2 thisGridPosition = this.transform.position;
            thisGridPosition.x += offsetXforGid;
            //thisGridPosition.y += offsetYforGid;
            this.transform.position = thisGridPosition;
            _inputmanger = new InputManager(gridTransform);

            scene = SceneManager.GetActiveScene();

            createGridCell();

            //���Ӿ����� ���� �ʿ��� �͵� ����
            if (IsGameScene)
            {
                m_BlockController = this.GetComponent<HexBlockController>();
            }
            else
            {
                m_BlockController = null;
            }

        }

        public void Update()
        {
            OnInputHandler();
        }

        void OnInputHandler()
        {

            //���Ӿ� ��ġ  >> HexSwipe ��������!
            if (IsGameScene)
            {
                if(!m_cTouchDown && _inputmanger.isTouchDown)
                {
                    Vector2 point = _inputmanger.touch2BoardPosition;
                    HexCell whatTouched = GetTouchedHexCell(point); 

                    if(whatTouched != null) //��ȿ�� �� ����
                    {
                        m_cTouchDown = true;
                        m_DownCoordinate = whatTouched.Coordinate;
                        m_TouchPos = point;
                    }

                }else if (m_cTouchDown && _inputmanger.isTouchUp)
                {
                    Vector2 point = _inputmanger.touch2BoardPosition;
                    HexSwipe swipeDir = _inputmanger.EvalHexSwipeDir(m_TouchPos, point);

                    if(swipeDir != HexSwipe.NA)
                    {
                        // ���� ���� ����� �� �ڷ�ƾ �Ǵ� �� ���ƾ� �� �� ����..
                        Vector3 targetCor = m_DownCoordinate.PlusCoodinate(swipeDir.GetTargetSwipeCor());
                        if (!gameGridCoordinates.Contains(targetCor))
                        {
                            Debug.Log("���������� ���ӻ� ���� �������� �ʽ��ϴ�.");
                            return;
                        }

                        m_BlockController.DoSwipAction(m_DownCoordinate, swipeDir);

                        
                    }

                    m_cTouchDown = false;

                    Debug.Log($"Swipe : {swipeDir} , Block = {m_DownCoordinate}");
                }
                return; // ���ĺ��ʹ� ���Ӿ��� �ƴ� ���׸�������ξ��� ���� ��ġ ����
            }



            if (_inputmanger.isTouchDown)
            {
                Vector2 point = _inputmanger.touch2BoardPosition;
                HexCell whatTouched = GetTouchedHexCell(point);

                if(whatTouched != null)
                {
                    TouchCellResult(whatTouched);
                }
            }



        }

        #region �� ������(Ȱ��/��Ȱ��)�����۾� �޼ҵ�

        ///<summary>
        ///������ ���� Ȱ������ �ʾҴٸ� Ȱ������ �ٲٰ� ����Ʈ�� �߰� / Ȱ���� �� �����ϸ� ��Ȱ��ȭ+����Ʈ����
        ///</summary>
        void TouchCellResult(HexCell cell)
        {
            bool isAleadyActive = cell.isActiveCell ? true : false;
            if (!isAleadyActive)
            {
                cell.IsActivate(true);
                activatedCells.Add(cell.Coordinate);
                DeliverActivatedCellListToCellDesign();

            }
            else
            {
                cell.IsActivate(false);

                for (int i = 0; i < activatedCells.Count; i++)
                {
                    if (activatedCells[i] == cell.Coordinate)
                    {
                        activatedCells.RemoveAt(i);
                        DeliverActivatedCellListToCellDesign();
                        break;
                    }
                }
            }
        }

        void DeliverActivatedCellListToCellDesign()
        {
            CellGridDesignTool.Instance.GetDeliveredList(activatedCells);
        }


        #endregion �� ������(Ȱ��/��Ȱ��)�����۾� �޼ҵ�


        #region Input ����

        // ��ġ��ġ> ��ġ��ġ ���� �׸��� �� �� ��ȯ
        private HexCell GetTouchedHexCell(Vector2 touchPosition)
        {
            HexCell touched = null;

            //TODO : touchPosition�� Y���� ��¦ �ȸ´µ� ���⼭ �����ϴ� �� ������?

            //���콺Ŭ������ ��簪���� �����ϴ� ���� ���� : https://www.redblobgames.com/grids/hexagons/#pixel-to-hex
            #region q�� r,s �� ����� ����

            //Debug.Log($"ť�� 0�� �Ǵ� �ǽ����� ã�� 0 : ����� x�� ����: {touchPosition.x}");
            //var q = ((2/3) * touchPosition.x)/hexRadius; //�̷��� ����ϸ� ������ ���ܼ� �ϳ��ϳ� ������ �����

            float q = touchPosition.x;
            //Debug.Log($"ť�� 0�� �Ǵ� �ǽ����� ã�� 1 {q}");
            float problemValueOfq = 2 / 3f;
            //Debug.Log($"ť�� 0�� �Ǵ� �ǽ����� ã�� 2-0 : problemValue = {problemValue}");
            q *= problemValueOfq;
            //Debug.Log($"ť�� 0�� �Ǵ� �ǽ����� ã�� 2 {q}");
            q /= hexRadius;
            //Debug.Log($"ť�� 0�� �Ǵ� �ǽ����� ã�� 3 {q}");


            // var r = (-1/3 * touchPosition.x + (Mathf.Sqrt(3)/3) * touchPosition.y)/hexRadius;
            float r = touchPosition.x;
            float problemValueOfr1 = -1 / 3f;
            float problemValueOfr2 = (Mathf.Sqrt(3)) / 3f;
            r *= problemValueOfr1;
            problemValueOfr2 *= touchPosition.y;
            r += problemValueOfr2;
            r /= hexRadius;

            q = axial_round(q, r).x;
            //Debug.Log($"ť�� 0�� �Ǵ� �ǽ����� ã�� 2 {q}");
            r = axial_round(q, r).y;


            //Debug.Assert(q.GetType() == typeof(int) && r.GetType() == typeof(int));

            int s = (int)(-q - r);
            #endregion q�� r,s �� ����� ����



            //Vector3 isTouchedIndex = new Vector3((int)q, (int)r, s);
            Vector3 isTouchedIndex = new Vector3((int)q, (int)s, r); // r�� s�� �ݴ밡 �Ǵ� ������ �־� ���⼭ �ٲ���

            // ��ġ�� ���� ���ϱ�
            if (IsGameScene)
            {
                foreach (KeyValuePair<Vector3, HexCell> items in gamegrid)
                {
                    if (items.Key == isTouchedIndex)
                    {
                        touched = gamegrid[isTouchedIndex];
                        //break;
                    }

                }
            }
            else
            {
                //���ӽ� �ƴϸ� 
                foreach (KeyValuePair<Vector3, HexCell> items in totalgrid)
                {
                    if (items.Key == isTouchedIndex)
                    {
                        items.Value.ChangeColor(true);
                        touched = totalgrid[isTouchedIndex];
                        Debug.Log(items.Key);
                        //break;
                    }
                    else { items.Value.ChangeColor(false); }

                }

            }



            return touched;
        }

        private Vector2Int axial_round(float x, float y)
        {
            //��ġ ���� ����ε��������� �ٲ��ֱ� ���� �Լ�, ���İ��� : https://observablehq.com/@jrus/hexround

            int xgrid = (int)Math.Round(x);
            int ygrid = (int)Math.Round(y);
            x -= xgrid; y -= ygrid; // remainder
            int dx = ((int)Math.Round(x + 0.5 * y)) * (x * x >= y * y ? 1 : 0);
            int dy = ((int)Math.Round(x + 0.5 * y)) * (x * x < y * y ? 1 : 0);
            return new Vector2Int(xgrid + dx, ygrid + dy);

        }



        #endregion Input ����


        #region ���Ӿ��� �޼ҵ� 

        // DIC�̿��ؼ� ��ǥ���� ���� �� ��ġ ���� ��


        public Vector2 GetCellPosition(Vector3 coordinate)
        {

            if (!IsGameScene)
            {
                Debug.Log("���Ӿ��� �޼ҵ带 ���Ӿ��� �ƴ� �� ����Ͽ����ϴ�.");

            }

            Vector2 position;

            if (cellPosition.ContainsKey(coordinate))
            {
                position = cellPosition[coordinate];
            }
            else
            {
                position = ErrorVector3;
                Debug.Log(coordinate);
                Debug.Log("�������� �����Ǿ����ϴ�.");
            }


            return position;


        }


        // ���⿡ ���� �̿� �� ���� �޼ҵ带 �Ʒ� Ȯ��޼���� ����


        #endregion  ���Ӿ��� �޼ҵ� 






        #region ���׸��� �������� ���� �޼ҵ� (Start �ܰ迡�� �۵���)

        public void createGridCell()
        {
            if (scene.name == Constants.NameOfGameScene) // ���� ���� ���Ӿ��ΰ�?
            {
                IsGameScene = true;
                GenerateShapeFromJsonDateForGameGrid();

            }
            else
            {
                IsGameScene = false;
                GenRectShapeForTotalGrid();

            }

        }

        private void GenRectShapeForTotalGrid()
        {
            Debug.Log("Generating rectangular shaped grid...");



            GenGridCellShape(totalgrid);

        }


        private void GenerateShapeFromJsonDateForGameGrid()
        {
            string jsonName = Constants.JsonNameOfGridCellDesign;
            //string path = Path.Combine(Constants.jsonPathOfGridCellDesign, Constants.jsonPathOfGridCellDesign);

            //path = Path.Combine(Application.dataPath + "/Data", jsonName);
            //path = $"{path}.txt";


            //if (!File.Exists(path))
            //{
            //    Debug.Log($"No json Saved : ���̽� ������ �����ϴ�! ������ Ȯ���ϼ���. ������ : {path}");
            //    GenGridCellShape(gamegrid, TempListMadeBySonSuBecauseJsonError);
            //    return;
            //}

            string path = $"Data/{jsonName}";
            var textData = Resources.Load<TextAsset>(path);
            if (textData == null)
            {
                Debug.Log($"No json Saved : ���̽� ������ �����ϴ�! ������ Ȯ���ϼ���. ������ : {path}");
                GenGridCellShape(gamegrid, TempListMadeBySonSuBecauseJsonError);
                return;
            }

            Debug.Log("Generating shaped grid For Game Using json ...");

            //Json �� �����ͼ� �����ϱ�
            CellDesignData saveData = new CellDesignData();
            //FileStream fileStream = new FileStream(path, FileMode.Open);
            //byte[] bytedate = new byte[fileStream.Length];
            //fileStream.Read(bytedate, 0, bytedate.Length);
            //fileStream.Close();
            //string loadJson = Encoding.UTF8.GetString(bytedate);

            string loadJson = textData.text;
            //string loadJson = File.ReadAllText(path); 
            //string loadJson = PlayerPrefs.GetString("DesignCell");
            saveData = JsonUtility.FromJson<CellDesignData>(loadJson);
            //saveData = JsonConvert.DeserializeObject<CellDesignData>(loadJson);

            Debug.Assert(saveData != null, "jSON ������ �ҷ����� ���߽��ϴ�.");
            activatedCells = saveData.SavedactivatedCellsInfo; // ���̽� ���Ͽ� ����� Ȱ��ȭ�� �� ��ǥ���� ������
            gameGridCoordinates = activatedCells; //public ������ ������ ����ġ�� ����

            //����Ʈ���� ���� ��ǥ�� ��ġ�ϴ� ���� ����
            GenGridCellShape(gamegrid, gameGridCoordinates);


        }

        /// <summary>
        /// ��� �� �׸��带 �����ϴ� �Լ�
        /// </summary>
        /// <param name="storageDic"> ������ ���� ������ ����Ʈ</param>
        /// <param name="criteriaList"> ������ ���ϴ� ���� ��ǥ�� ���� ����Ʈ,Null ��� </param>
        private void GenGridCellShape(Dictionary<Vector3, HexCell> storageDic, List<Vector3> criteriaList = null)
        {
            HexCell cell;
            Vector3 pos = Vector3.zero;

            for (int q = -mapWidth + 1; q < mapWidth; q++)
            {
                int qOff = q >> 1;
                for (int r = -mapHeight - qOff - 1; r < mapHeight - qOff; r++) // int r = -mapHeight - qOff -1 : ������ �׸��� �� �ؿ� ���� �� �߰��ϱ� ���� -1 �߰�
                {
                    pos.x = hexRadius * 3.0f / 2.0f * q;
                    pos.y = hexRadius * Mathf.Sqrt(3.0f) * (r + q / 2.0f);

                    // ��缿 ��ġ�� ��ġ�� ������ ���� �������ش�
                    pos.x += offsetXforGid;
                    //pos.y += offsetYforGid;

                    // ��r

                    //p r s ��ǥ���� r�� s�� �ٲ�� ������ �ɷ� ���� ������ �ٲ���
                    int c_q = q;
                    int c_r = -q - r;
                    int c_s = r;
                    
                    
                    Vector3 newCordinate = new Vector3(c_q, c_r, (c_s));
                    if (criteriaList != null) // ������ǥ ����Ʈ�� �ִ��� Ȯ�� 
                    {
                        // TODO �ٸ� �ڷᱸ�� �� ���� �� �� �� ���� �� ������, (�̹� �˻��� ���� �˻� ���Ѵٴ���..)����� �� �غ��� �ʹ�.
                        bool isNecessary = false;
                        foreach (Vector3 cor in criteriaList)
                        {
                            if (cor == newCordinate)
                            {
                                isNecessary = true;
                                // Debug.Log($"�ʿ��� ���̶� �����մϴ� :{newCordinate}");
                                break;
                            }
                        }
                       // Debug.Log($"�ߺ� ! �ʿ���� ���̶� �������� �ʽ��ϴ� :{newCordinate}");
                        if (!isNecessary) continue; // �ʿ��� ���� �ƴϸ� �������� �ʴ´�. 

                    }

                    cell = CreateHexGO(pos, ("Hex[" + c_q + "," + c_r + "," + (c_s).ToString() + "]"), ("["+ c_q + c_r + (c_s) +"]"));

                    //�� ���� ��ġ���� �����ص���! 
                    cell.Init(newCordinate, this);


                    if (IsGameScene)
                    {
                        cell.IsActivate(true, true);

                        //���Ӿ��϶��� �� ��ġ ����
                        
                        cellPosition.Add(cell.Coordinate, pos);
                        Debug.Log($"[{cell.Coordinate}] : {pos}");
                    }

                    //������ ����ҿ� ���� 
                    storageDic.Add(cell.Coordinate, cell);
                }
            }
        }


        private HexCell CreateHexGO(Vector3 postion, string nam, string cordinate)
        {
            GameObject go = Instantiate(hexPrebab, postion, Quaternion.identity);
            go.name = nam;

            go.transform.parent = this.transform;
            go.transform.localPosition = postion;
            Debug.Log($"{nam} : {go.transform.localPosition}");

            HexCell cell = go.GetComponent<HexCell>();

            if (!IsGameScene) //���Ӿ��� �ƴϸ� ��ǥ�� Text�� �����ְ� �Ѵ�. 
            {
                cell.cellName.text = cordinate;
            }

            return cell;
        }

        private List<Vector3> TempListMadeBySonSuBecauseJsonError = new List<Vector3>
    {
        new Vector3(0,0,0),
        new Vector3(0,-1,1),
        new Vector3(1,-1,0),
        new Vector3(2,-2,0),
        new Vector3(-3,0,3),
        new Vector3(1,-2,1),

    };


        #endregion ���׸��� �������� ���� �޼ҵ� (Start �ܰ迡�� �۵���)


    }

    public static class CoordinatesExtensionMethod
    {


        /// <summary>
        /// �̿��ϴ� ������ ��ǥ ���� Ȯ�� �޼���, ��ȯ�� ��������ǥ�� ���� ����Ȯ���� ���� �ʿ�
        /// </summary>
        public static Vector3[] myNeighbors(this Vector3 me)
        {
            Vector3[] neighbors = new Vector3[6];

            for (int i = 0; i < HexArchive.HexDirections.Length; i++)
            {
                Vector3 newNeighbor = me.PlusCoodinate(HexArchive.HexDirections[i]);
                neighbors[i] = newNeighbor;
            }

            return neighbors;


        }
    }

}

