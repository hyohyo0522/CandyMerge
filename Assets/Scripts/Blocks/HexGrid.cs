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

        // 멥구성관련 
        private int mapWidth = 4;
        private int mapHeight = 4;
        private float offsetXforGid = 0f;
        //private float offsetYforGid = 0.48f;
        public Transform gridTransform;

        // 헥사셀관련
        public GameObject hexPrebab;
        private float hexRadius; // 헥사반지름은 따로 실행할 때 따로 계산해서 넣어놓기 
        float effectForHexRadius = 1.00f; //반지름에 곱해주어 헥사간의 간격을 조정한다. 


        // 헥사블럭 연결
        HexBlockController m_BlockController;


        #region 헥사 Coordinate 저장 관련 
        // [TODO] 다 private로 만들고 외부에서는 함수로만 값을 받아오게 만들까?
        // 전체 헥사그리드셀 저장 리스트(셀그리드디자인씬에서 사용함)
        private Dictionary<Vector3, HexCell> totalgrid = new Dictionary<Vector3, HexCell>();
        public Dictionary<Vector3, HexCell> TotalGrids { get { return totalgrid; } }


        // 활성화 셀 좌표값 담은 리스트(Json 데이터를 주고받는 용도 + a)
        private List<Vector3> activatedCells = new List<Vector3>(); // 활성가능한 그리드는 딕셔너리의 키가 되는 키값만 저장한다. 

        // 게임신에서는 activatedCells 과 같은 값을 공유하지만, public으로 구분해서 외부에서 쓸 수 있도록 함 
        public List<Vector3> gameGridCoordinates = new List<Vector3>();

        //좌표값에 따른 셀위치 저장한 Dic  >> 좌표랑 위치 같은 vector3하니까 헷갈려서 위치를 일단 vector2로, 셀위치랑 블럭위치 구분해서 사용할 것임!!
        private Dictionary<Vector3, Vector2> cellPosition = new Dictionary<Vector3, Vector2>();


        // 실제 게임에 쓰이는 셀그리드
        private Dictionary<Vector3, HexCell> gamegrid = new Dictionary<Vector3, HexCell>();
        public Dictionary<Vector3, HexCell> GameGrids { get { return gamegrid; } }

        #endregion 헥사 Coordinate 저장 관련

        #region 터치/스와이프 관련 (Input)

        InputManager _inputmanger;
        bool m_cTouchDown; // 입력상태 처리 플래그, 유효한 셀을 선택한 경우 true;
        Vector3 m_DownCoordinate; // 누른 셀 좌표값
        Vector3 m_TouchPos; // 터치한 위치 (그리드 기준 로컬좌표)


        #endregion 터치/스와이프 관련 (Input)

        #region 씬 구분 관련 > 본 스크립트 작동의 기준

        //TODO 지금은 씬 이름과 bool값으로 그리드의 게임상 작동을 분리하지만, 나중에는 스크립트를 아예 분리해버리자! (기능중심.cs/그리드Info.cs/게임상작동.cs)
        bool IsGameScene = false;
        Scene scene;

        #endregion 씬 구분 관련 > 본 스크립트 작동의 기준

        #region 지정 에러값 (NOT NULL변수들을 위한 값)
        Vector3 ErrorVector3 = new Vector3(-99, -99, -99);
        #endregion 정 에러값 (NOT NULL변수들을 위한 값)





        // Start is called before the first frame update
        public void Start()
        {
            gridTransform = GetComponent<Transform>();
            //헥사반지름에 보정값 적용하여 헥사블럭간의 간격조정
            hexRadius = (hexPrebab.GetComponent<BoxCollider2D>().size.x / 2) * effectForHexRadius;

            //그리그위치를 오프셋 값으로 조정 
            Vector2 thisGridPosition = this.transform.position;
            thisGridPosition.x += offsetXforGid;
            //thisGridPosition.y += offsetYforGid;
            this.transform.position = thisGridPosition;
            _inputmanger = new InputManager(gridTransform);

            scene = SceneManager.GetActiveScene();

            createGridCell();

            //게임씬에서 쓸때 필요한 것들 설정
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

            //게임씬 터치  >> HexSwipe 구현하자!
            if (IsGameScene)
            {
                if(!m_cTouchDown && _inputmanger.isTouchDown)
                {
                    Vector2 point = _inputmanger.touch2BoardPosition;
                    HexCell whatTouched = GetTouchedHexCell(point); 

                    if(whatTouched != null) //유효한 셀 선택
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
                        // 선택 범위 벗어났을 때 코루틴 되는 걸 막아야 할 것 같다..
                        Vector3 targetCor = m_DownCoordinate.PlusCoodinate(swipeDir.GetTargetSwipeCor());
                        if (!gameGridCoordinates.Contains(targetCor))
                        {
                            Debug.Log("스와이프할 게임상 셀이 존재하지 않습니다.");
                            return;
                        }

                        m_BlockController.DoSwipAction(m_DownCoordinate, swipeDir);

                        
                    }

                    m_cTouchDown = false;

                    Debug.Log($"Swipe : {swipeDir} , Block = {m_DownCoordinate}");
                }
                return; // 이후부터는 게임씬이 아닌 셀그리드디자인씬일 때의 터치 구현
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

        #region 셀 디자인(활성/비활성)관련작업 메소드

        ///<summary>
        ///선택한 셀이 활성되지 않았다면 활성셀로 바꾸고 리스트에 추가 / 활성된 셀 선택하면 비활성화+리스트제거
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


        #endregion 셀 디자인(활성/비활성)관련작업 메소드


        #region Input 관련

        // 터치위치> 터치위치 받은 그리드 안 셀 반환
        private HexCell GetTouchedHexCell(Vector2 touchPosition)
        {
            HexCell touched = null;

            //TODO : touchPosition의 Y값이 살짝 안맞는데 여기서 보정하는 게 나을까?

            //마우스클릭값을 헥사값으로 변경하는 수식 관련 : https://www.redblobgames.com/grids/hexagons/#pixel-to-hex
            #region q과 r,s 값 계산한 과정

            //Debug.Log($"큐가 0이 되는 의심지점 찾기 0 : 여기는 x값 비교함: {touchPosition.x}");
            //var q = ((2/3) * touchPosition.x)/hexRadius; //이렇게 계산하면 문제가 생겨서 하나하나 나눠서 계산함

            float q = touchPosition.x;
            //Debug.Log($"큐가 0이 되는 의심지점 찾기 1 {q}");
            float problemValueOfq = 2 / 3f;
            //Debug.Log($"큐가 0이 되는 의심지점 찾기 2-0 : problemValue = {problemValue}");
            q *= problemValueOfq;
            //Debug.Log($"큐가 0이 되는 의심지점 찾기 2 {q}");
            q /= hexRadius;
            //Debug.Log($"큐가 0이 되는 의심지점 찾기 3 {q}");


            // var r = (-1/3 * touchPosition.x + (Mathf.Sqrt(3)/3) * touchPosition.y)/hexRadius;
            float r = touchPosition.x;
            float problemValueOfr1 = -1 / 3f;
            float problemValueOfr2 = (Mathf.Sqrt(3)) / 3f;
            r *= problemValueOfr1;
            problemValueOfr2 *= touchPosition.y;
            r += problemValueOfr2;
            r /= hexRadius;

            q = axial_round(q, r).x;
            //Debug.Log($"큐가 0이 되는 의심지점 찾기 2 {q}");
            r = axial_round(q, r).y;


            //Debug.Assert(q.GetType() == typeof(int) && r.GetType() == typeof(int));

            int s = (int)(-q - r);
            #endregion q과 r,s 값 계산한 과정



            //Vector3 isTouchedIndex = new Vector3((int)q, (int)r, s);
            Vector3 isTouchedIndex = new Vector3((int)q, (int)s, r); // r과 s가 반대가 되는 현상이 있어 여기서 바꿔줌

            // 터치한 셀값 구하기
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
                //게임신 아니면 
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
            //터치 값을 헥사인덱스값으로 바꿔주기 위한 함수, 수식관련 : https://observablehq.com/@jrus/hexround

            int xgrid = (int)Math.Round(x);
            int ygrid = (int)Math.Round(y);
            x -= xgrid; y -= ygrid; // remainder
            int dx = ((int)Math.Round(x + 0.5 * y)) * (x * x >= y * y ? 1 : 0);
            int dy = ((int)Math.Round(x + 0.5 * y)) * (x * x < y * y ? 1 : 0);
            return new Vector2Int(xgrid + dx, ygrid + dy);

        }



        #endregion Input 관련


        #region 게임씬용 메소드 

        // DIC이용해서 좌표값에 따른 셀 위치 뱉어내는 툴


        public Vector2 GetCellPosition(Vector3 coordinate)
        {

            if (!IsGameScene)
            {
                Debug.Log("게임씬용 메소드를 게임씬이 아닐 때 사용하였습니다.");

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
                Debug.Log("에러값이 지정되었습니다.");
            }


            return position;


        }


        // 방향에 따른 이웃 셀 뱉어내는 메소드를 아래 확장메서드로 구현


        #endregion  게임씬용 메소드 






        #region 셀그리드 생성과정 관련 메소드 (Start 단계에서 작동됨)

        public void createGridCell()
        {
            if (scene.name == Constants.NameOfGameScene) // 지금 씬이 게임씬인가?
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
            //    Debug.Log($"No json Saved : 제이슨 파일이 없습니다! 저장경로 확인하세요. 저장경로 : {path}");
            //    GenGridCellShape(gamegrid, TempListMadeBySonSuBecauseJsonError);
            //    return;
            //}

            string path = $"Data/{jsonName}";
            var textData = Resources.Load<TextAsset>(path);
            if (textData == null)
            {
                Debug.Log($"No json Saved : 제이슨 파일이 없습니다! 저장경로 확인하세요. 저장경로 : {path}");
                GenGridCellShape(gamegrid, TempListMadeBySonSuBecauseJsonError);
                return;
            }

            Debug.Log("Generating shaped grid For Game Using json ...");

            //Json 값 가져와서 저장하기
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

            Debug.Assert(saveData != null, "jSON 파일을 불러오지 못했습니다.");
            activatedCells = saveData.SavedactivatedCellsInfo; // 제이슨 파일에 저장된 활성화된 셀 좌표값을 가져옴
            gameGridCoordinates = activatedCells; //public 용으로 전달할 셀위치값 저장

            //리스트값에 따라 좌표값 일치하는 셀만 생성
            GenGridCellShape(gamegrid, gameGridCoordinates);


        }

        /// <summary>
        /// 헥사 셀 그리드를 생성하는 함수
        /// </summary>
        /// <param name="storageDic"> 생성한 셀을 저장할 리스트</param>
        /// <param name="criteriaList"> 생성을 원하는 셀의 좌표를 담은 리스트,Null 허용 </param>
        private void GenGridCellShape(Dictionary<Vector3, HexCell> storageDic, List<Vector3> criteriaList = null)
        {
            HexCell cell;
            Vector3 pos = Vector3.zero;

            for (int q = -mapWidth + 1; q < mapWidth; q++)
            {
                int qOff = q >> 1;
                for (int r = -mapHeight - qOff - 1; r < mapHeight - qOff; r++) // int r = -mapHeight - qOff -1 : 생성된 그리드 맨 밑에 한줄 더 추가하기 위해 -1 추가
                {
                    pos.x = hexRadius * 3.0f / 2.0f * q;
                    pos.y = hexRadius * Mathf.Sqrt(3.0f) * (r + q / 2.0f);

                    // 헥사셀 위치도 위치의 오프셋 값을 적용해준다
                    pos.x += offsetXforGid;
                    //pos.y += offsetYforGid;

                    // ★r

                    //p r s 좌표에서 r과 s가 바뀌어 나오는 걸로 보여 순서를 바꿔줌
                    int c_q = q;
                    int c_r = -q - r;
                    int c_s = r;
                    
                    
                    Vector3 newCordinate = new Vector3(c_q, c_r, (c_s));
                    if (criteriaList != null) // 기준좌표 리스트가 있는지 확인 
                    {
                        // TODO 다른 자료구조 더 좋은 걸 쓸 수 있을 것 같은데, (이미 검사한 값은 검사 안한다던지..)고민을 더 해보고 싶다.
                        bool isNecessary = false;
                        foreach (Vector3 cor in criteriaList)
                        {
                            if (cor == newCordinate)
                            {
                                isNecessary = true;
                                // Debug.Log($"필요한 셀이라 생성합니다 :{newCordinate}");
                                break;
                            }
                        }
                       // Debug.Log($"삐빅 ! 필요없는 셀이라 생성하지 않습니다 :{newCordinate}");
                        if (!isNecessary) continue; // 필요한 셀이 아니면 생성하지 않는다. 

                    }

                    cell = CreateHexGO(pos, ("Hex[" + c_q + "," + c_r + "," + (c_s).ToString() + "]"), ("["+ c_q + c_r + (c_s) +"]"));

                    //각 셀의 위치값도 저장해두자! 
                    cell.Init(newCordinate, this);


                    if (IsGameScene)
                    {
                        cell.IsActivate(true, true);

                        //게임씬일때만 셀 위치 저장
                        
                        cellPosition.Add(cell.Coordinate, pos);
                        Debug.Log($"[{cell.Coordinate}] : {pos}");
                    }

                    //지정된 저장소에 저장 
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

            if (!IsGameScene) //게임씬이 아니면 좌표를 Text로 보여주게 한다. 
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


        #endregion 셀그리드 생성과정 관련 메소드 (Start 단계에서 작동됨)


    }

    public static class CoordinatesExtensionMethod
    {


        /// <summary>
        /// 이웃하는 육각형 좌표 뱉어내는 확장 메서드, 반환된 육각형좌표에 대한 상태확인은 따로 필요
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

