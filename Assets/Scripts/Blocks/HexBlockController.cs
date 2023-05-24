using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hyo.Scriptable;
using Hyo.Util;
using Hyo.core;


namespace Hyo.HexItems
{
    public class HexBlockController : MonoBehaviour
    {

        //헥사블럭관련
        public GameObject hexBlockPrefab; // 트랜스폼과 스프라이트를 가지는 친구, 인스펙터에서 할당
        public Transform _prefabContainerInHierarchy;
        [SerializeField] NormalBlockConfig normalBlockConfig;

        //블럭 저장소
        Dictionary<Vector3,HexBlock> hexBlockItems = new Dictionary<Vector3, HexBlock>();

        //블럭타입저장소
        Dictionary<Vector3, BlockCandyType> m_hexBlockResults = new Dictionary<Vector3, BlockCandyType>();
        public Dictionary<Vector3, BlockCandyType> HexBlockResults { get { return m_hexBlockResults; } }

        //헥사 관련 클래스
        HexGrid m_HexGrid; //헥사 그리드 >> 셀 정보와 좌표값 관련 
        BlockBuilder m_blockBuilder; // 블럭 빌더 >> 블럭 셔플과 블록 중복 검사 

        //블럭 무브 관련
        bool m_bRunning; // 액션 실행 상태 : 실행중인 경우 true


        private void Start()
        {
            Debug.Log("Start 함수 진입");
            m_HexGrid = this.gameObject.GetComponent<HexGrid>();
            m_blockBuilder = new BlockBuilder(this, m_HexGrid.gameGridCoordinates);

            m_blockBuilder.BlockSuffle();
            m_hexBlockResults = m_blockBuilder.HexBlockResults();

            foreach(KeyValuePair<Vector3, BlockCandyType> item in m_hexBlockResults)
            {
                //나중에 블럭인포info.cs나 스테이지info 같은 걸 이용해서 특수블럭이 생성될 좌표값을 여기서 걸러내서 블럭들을 만들 수 있지 않을까??
                CreateHexBlock(item.Key, item.Value);
            }

        }

        #region 블럭생성

        //블럭생성
        void CreateHexBlock(Vector3 coordinate, BlockCandyType Type)
        {
            if (Type == BlockCandyType.SPECIAL)
            {
                //나중에 스페셜 블록 만들 때 여기서 어떤 로직 추가 하면 좋을 것 같다.
                return;
            }
            GameObject go = Instantiate(hexBlockPrefab, transform.position, Quaternion.identity);
            go.transform.parent = _prefabContainerInHierarchy.transform;


            HexBlock block = go.AddComponent<HexBlock>(); //모노비헤이비어이기 때문에 이런식으로 프리팹과 연결 
            block.Init(this, coordinate, Type, go);


            if (hexBlockItems.ContainsKey(coordinate))
            {
                //오류 검사 >> 블럭 생성전 중복된 값이 들어가있는지 확인
                Debug.Assert(hexBlockItems[coordinate] == null, "블럭을 생성하고자 하나 이미 블럭이 있습니다.");

                hexBlockItems[coordinate] = block;
            }
            else
            {
                hexBlockItems.Add(coordinate, block);
            }


        }

        /// <summary>
        /// 게임 도중에 사용, 원하는 좌표에 새로운 블럭 랜덤으로 생성, 생성원하는 좌표가 게임블럭안쪽에만 위치해야함 
        /// </summary>
        /// <param name="coordinate"></param>
        void CreateRandomHexBlock(Vector3 coordinate)
        {
            int random = Random.Range(0, System.Enum.GetValues(typeof(BlockCandyType)).Length - 2);
            BlockCandyType myType = (BlockCandyType)random;

            CreateHexBlock(coordinate, myType);


            Debug.Assert(m_hexBlockResults.ContainsKey(coordinate),"블럭타입리스트에 미리 생성된 좌표키가 없습니다."); 
            // >> 생성원하는 위치가 그리드 안쪽 위치에서만 할 수 있도록 함 
            
            Debug.Assert(m_hexBlockResults[coordinate] == BlockCandyType.NA, "블럭타입을 갱신하고자 하나 이미 블럭타입정보가 있습니다");
            m_hexBlockResults[coordinate] = myType;




        }

        #endregion 블럭생성 





        #region 블럭 이동 명령

        //블럭 이동관련

        public void DoSwipAction(Vector3 blockCordinate, HexSwipe swipeDir)
        {
            //검사식 필요한 것 있으면 넣기
            Debug.Assert(hexBlockItems.ContainsKey(blockCordinate), "스와이프할 블럭이 존재하지 않습니다.");

            StartCoroutine(CoDoSwipeAction(blockCordinate, swipeDir));


        }

        IEnumerator CoDoSwipeAction(Vector3 blockCordinate, HexSwipe swipeDir)
        {
            if (!m_bRunning)  //다른 액션이 수행 중이면 PASS
            {
                m_bRunning = true;    //액션 실행 상태 ON

                //1. swipe action 수행
                Returnable<bool> bSwipedBlock = new Returnable<bool>(false);


                yield return SwipeAction(blockCordinate, swipeDir, bSwipedBlock);


                m_bRunning = false;    //액션 실행 상태 OFF
                //다른 액션 넣을 수 있으니 일단 이렇게 만들어놓자. 



            }


            yield break;
        }

        public IEnumerator SwipeAction(Vector3 blockCordinate, HexSwipe swipeDir, Returnable<bool> actionResult)
        {
            Debug.Log("여기까지 오니??");

            actionResult.value = false; //코루틴 리턴값 RESET

            //1. 스와이프되는 상대 블럭 위치를 구한다. (using SwipeDir Extension Method)
            //blockCordinate += swipeDir.GetTargetCol();
            Vector3 targetCoordinate = blockCordinate.PlusCoodinate(swipeDir.GetTargetSwipeCor());
            if (!hexBlockItems.ContainsKey(targetCoordinate))
            {
                Debug.Log("선택한 셀이 생성목록에 없습니다.");

            }

            //2. 스와이프 전 이동전 위치를 저장한다.
            HexBlock start = hexBlockItems[blockCordinate];
            HexBlock target = hexBlockItems[targetCoordinate];

            //3. 스와이프 가능한 블럭인지 체크한다. 

            #region 스와이프 가능 조건 검사

            if (!target)
            {
                Debug.Log("스와이프 가능한 블럭이 없네요!");
                yield break;

            }
            if (!hexBlockItems.ContainsKey(targetCoordinate))
            {
                Debug.Log("오류입니다! 스와이프 가능한 블럭인지 체크하세요.");
                yield break;
            };

            if (target.BlockType == BlockType.EMPTY)
            {
                Debug.Log("오류입니다! 스와이프 가능한 블럭인지 체크하세요.");
                yield break;
            };

            if (target.CandyType == BlockCandyType.NA)
            {
                Debug.Log("오류입니다! 스와이프 가능한 블럭인지 체크하세요.");
                yield break;
            };

            //TODO 나중에 스페셜 블럭인지 검사하는 항목을 여기서 추가하는 게 좋을까? 

            #endregion 스와이프 가능 조건 검사



            if (!start) //Null오류 방지
            {
                yield break;
            }

            Vector2 startPosition = GetBlockposition(blockCordinate);
            Vector2 targetPosition = GetBlockposition(targetCoordinate);


            //4.스와이프 액션을 수행한다.
            StartCoroutine(start.MoveTo(targetPosition, Constants.SwipeDuration));
            StartCoroutine(target.MoveTo(startPosition, Constants.SwipeDuration));

            yield return new WaitForSeconds(Constants.SwipeDuration);

            //5. 좌표값 바꾸기
            start.ChangeCoordinate(targetCoordinate);
            target.ChangeCoordinate(blockCordinate);

            //6 저장소의 키-밸류값 바꾸기
            hexBlockItems[targetCoordinate] = start;
            hexBlockItems[blockCordinate] = target;


            UpdtateChangedInfo(start,start.CellCordinate);
            UpdtateChangedInfo(target, target.CellCordinate);

            actionResult.value = false;


            //[05.24 코루틴에 대한 이해가 부족한 것 같다. ]

            DoDestroyBlocks(); //여기인가??

            //while (m_blockBuilder.allEmptyBlock().Count > 0)
            //{
            //    FillTheBlankOfThreeUpperHexes(m_blockBuilder.allEmptyBlock());
            //    DoDestroyBlocks();
            //}



            //FillTheBlankOfThreeUpperHexes(m_blockBuilder.allEmptyBlock());
            // 빈 공간에 흐르는 블럭 만들기 : 일단 실험삼아 어떻게 나오나 해보자! 


            yield break;

        }


        public void orderFlowBlocks(List<Vector3> empties) 
        {

            //특정위치에서 비어있는 육각형 셀으로 가는 경로를 계산하는 공식을 활용하자!

            foreach (Vector3 blockPos in empties)
            {
                Vector3 upCor = blockPos.PlusCoodinate(HexArchive.HexDirections[1]);

                if (hexBlockItems[upCor] != null) 
                {
                    //원래값 저장 
                    Vector2 targetPos = GetBlockposition(blockPos);
                    Vector3 upOriginCor = upCor;

                    //블럭 구하기
                    HexBlock UpBlock = hexBlockItems[upCor];

                    if (UpBlock != null)
                    {
                        StartCoroutine(UpBlock.MoveTo(targetPos, Constants.SwipeDuration));
                        Debug.Log($"블럭이 움직였다! : [바로 위블럭] : {upCor}가 {blockPos} 로 움직였다.");
                        UpBlock.ChangeCoordinate(blockPos);
                        UpdtateChangedInfo(UpBlock, UpBlock.CellCordinate);

                        //원래 업블럭이었던 건의 값 정리
                        m_hexBlockResults[upOriginCor] = BlockCandyType.NA;
                        hexBlockItems[upOriginCor] = null; //터치 눌문제 발생할 것임 >> 해결해야함!!
                    }

                }
                else 
                {
                    Vector2 targetPos = GetBlockposition(blockPos);
                    Vector3 upOriginCor = upCor;
                    Debug.Log($"tatgetpos[현재블럭] : {blockPos} 의 upOriginCor[바로 위블럭]의 좌표 :  {upOriginCor} ");

                    /*
                    // [TODO]여기서 블럭 타입 정해주는 로직 넣기 멋지게 짜면 좋을 것 같은데 .ㅠ-ㅠ
                    int random = Random.Range(0, System.Enum.GetValues(typeof(BlockCandyType)).Length - 2);

                    //[TODO] 새로운 블럭이 생성되는 위치를 다시 계산하자! 
                    CreateHexBlock(upCor, (BlockCandyType)random);
                    HexBlock UpBlock = hexBlockItems[upCor];

                    StartCoroutine(UpBlock.MoveTo(targetPos, Constants.SwipeDuration));
                    UpBlock.ChangeCoordinate(blockPos);
                    UpdtateChangedInfo(UpBlock, UpBlock.CellCordinate);

                    //원래 업블럭이었던 건의 값 정리
                    m_hexBlockResults[upOriginCor] = BlockCandyType.NA;
                    hexBlockItems[upOriginCor] = null; //터치 눌문제 발생할 것임 >> 해결해야함!!
                    */

                }
                DoDestroyBlocks();

            }

        }


        /// <summary>
        /// 비워진 블럭의 위 혹은 오른위,왼위쪽으로부터 블럭을 떨어뜨려 빈칸을 채우는 함수
        /// </summary>
        public void FillTheBlankOfThreeUpperHexes(List<Vector3> empties)
        {
            bool isNewGenStart = false;
            Queue<Vector3> ways; // 블럭이동할 좌표 저장한 큐 
            Vector3 start;
            Vector3 end;

            //1. 가장 위의 블럭좌표부터 찾아서 [도착블럭위치]로 잡기
            end = new Vector3(99, 99, -99); //비교가 용이한 기준값을 넣어준다. 

            foreach(Vector3 item in empties)
            {
                Debug.Log($"빈셀 공간에 대한 정보 : {item}");
            }


            foreach(Vector3 item in empties)
            {
                if(item.y< end.y || item.z > end.z) // 점점 가운데 위로 가는 쪽으로 검사 
                {
                    end = item;
                }
            }
            Debug.Log($"결정된 end의 값은 : {end}");

            //2. 밑의 좌표를 채워줄 블럭이 있는지 확인
            //2-1 있으면 그 블럭을 [스타트 블럭]으로 지정
            start = GetfilledUpperHex(end);  

            //2-2 없으면 바로 end좌표의 가장 위 블럭공간을 [스타트블럭]위치로 지정하고 ++  새로운 블럭 생성해야함 
            if (start == Constants.errorVector3) //에러값이 나오면 start위치를 조정해야한다. 
            {
                start = GetHighestHexAlongLine(end);
                //시작점에 새로운 블럭생성해줘야하므로 true 설정
                isNewGenStart = true; // 

            }

            if (isNewGenStart) CreateRandomHexBlock(start);


            // 3. [스타트블럭]이 [도착블럭]으로 흘러가는 경로를 얻어와서 흘려보내기
            ways = GetWayToFlow(start,end, empties); //경로 얻어옴


            StartCoroutine(MoveToFillTheBlank(start,end, ways));


            // 4. 블럭위치이동에 따른 정보 갱신하기 
            // >> 이미 코루틴 함수에서 다 실시함 
        }

        public IEnumerator MoveToFillTheBlank(Vector3 start, Vector3 end, Queue<Vector3> ways)
        {

            //이동되어야 하는 위치 저장

            //일단 저장..

            HexBlock startBlock = hexBlockItems[start];

            //[TODO] 이전 스타트정보 저장해놔야함
            //[TODO] 무브할때마다 갱신해줘야할까???
            // 갱신되어야 하는 정보 >> 원래 스타트의 

            Vector3 nowStartCoordinate = start; // while이 돌동안 바뀔 현재 스타블럭의 좌표값의 초기값 세팅 

            while (ways.Count>0)
            {

                Vector3 nextMoveCoordinate = ways.Dequeue();
                Vector2 nextPosition = GetBlockposition(nextMoveCoordinate);
                StartCoroutine(startBlock.MoveTo(nextPosition, Constants.SwipeDuration));

                yield return new WaitForSeconds(Constants.SwipeDuration);

            //[★정보갱신 모델 1.] 옮길 때마다 바꿔준다 >> 일단 이걸 픽 

                startBlock.ChangeCoordinate(nextMoveCoordinate);
                UpdtateChangedInfo(startBlock, startBlock.CellCordinate);
                hexBlockItems[nowStartCoordinate] = null;
                m_hexBlockResults[nowStartCoordinate] = BlockCandyType.NA;
                nowStartCoordinate = nextMoveCoordinate;

            }



        }

        /// <summary>
        /// [바로 위 혹은 오른쪽위/왼쪽위의 채워진 블럭의 좌표값을 뱉는 함수] 반환값이 에러값이면 위,오른쪽위,왼쪽위에 아무 블럭도 없는 상태이다. >
        /// </summary>
        /// <param name="emptyHexCell">바로 위나 오른쪽위,왼쪽위에 있는 블럭이 있는지 검사하고 싶은 위치의 좌표</param>
        /// <returns></returns>
        Vector3 GetfilledUpperHex(Vector3 emptyHexCell)
        {
            Vector3 filled = Constants.errorVector3;
            Vector3 NextupHex = emptyHexCell.PlusCoodinate(HexArchive.HexDirections[1]);
            Vector3 NextRightUpHex = emptyHexCell.PlusCoodinate(HexArchive.HexDirections[0]);
            Vector3 NextLeftUpHex = emptyHexCell.PlusCoodinate(HexArchive.HexDirections[2]);


            bool[] isActivated = new bool[] 
            {
                hexBlockItems.ContainsKey(NextupHex)?true:false, // UP
                hexBlockItems.ContainsKey(NextRightUpHex)?true:false, // RIGHT_UP
                hexBlockItems.ContainsKey(NextLeftUpHex)?true:false, // LEFT_U
            };


            if (isActivated[0] && HexBlockResults[NextupHex] != BlockCandyType.NA)
            {
                filled = NextupHex;
                return filled;
            }

            //양옆이 모두 활성화 된 존재하는 블럭셀인 경우에는 여기서 램덤으로 뽑아주자.
            if(isActivated[1] && isActivated[2] && 
                HexBlockResults[NextRightUpHex] != BlockCandyType.NA 
                && HexBlockResults[NextLeftUpHex] != BlockCandyType.NA)
            {
                int i = Random.RandomRange(1, 101);
                if (i > 50)
                {
                    filled = NextRightUpHex;
                    return filled;
                }
                else
                {
                    filled = NextLeftUpHex;
                    return filled;
                }
            }
            
            if (isActivated[1] && HexBlockResults[NextRightUpHex] != BlockCandyType.NA)
            {
                filled = NextRightUpHex;
                return filled;
            }

            if (isActivated[2] && HexBlockResults[NextLeftUpHex] != BlockCandyType.NA)
            {
                filled = NextLeftUpHex;
                return filled;
            }


            //여기서 다음 재귀함수로 들어가는 조건을 어떻게 정하느냐에 따라[★흘리는 모양새★]가 결정된다. 
            // 근데 나는 맨위 셀공간이 없으면 바로 위에서  ↓ 흘러내리게 만들고 싶고
            // 바로 위에 셀 공간이 있을 때에만 꺽은선 모양으로 ↘ ↙ 흐르게 하고 싶으므로
            if (!isActivated[0])
            {
                return filled; // 위에 셀이 존재하지 않으면 바깥공간에 새 블럭을 생성해야 하므로 에러값을 보낸다. 
            }
            else
            {
                return GetfilledUpperHex(NextupHex); //위에 셀이 존재하면 한번 더 검사해보자!
            }


        }

        /// <summary>
        /// q라인에서 가장 높은 위치의 셀좌표를 반환하는 함수, 활성셀 안의 값만 반환
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        Vector3 GetHighestHexAlongLine(Vector3 start)
        {
            Debug.Assert(hexBlockItems.ContainsKey(start), "잘못된 좌표값이 입력");

            Vector3 hightestHex = Constants.errorVector3;
            hightestHex.z = -99; //비교를 위해 값을 조정 >> 헥사타일이 바로 위에 있으려면 q,r,s(x,y,z)가 (0,-1,+1)되어야 한다. 
            foreach (Vector3 item in hexBlockItems.Keys)
            {
                // [TODO] 두번째 조건 다시 검토하기!!!!!! R과 S 모두 검사해야할 것 같다!!!!
                if (item.x == start.x && hightestHex.y > item.y && hightestHex.z < item.z)
                {
                    hightestHex = item;
                }
            }
            return hightestHex;
        }

        /// <summary>
        /// 블럭이 지나갈 경로를 List<Vector3>로 반환해주는 함수, 경로는 시작점을 제외하고 도착점까지를 포함한 경로
        /// </summary>
        /// <param name="start">블럭 시작위치</param>
        /// <param name="end">블럭 끝위치</param>
        /// <param name="wayOfempties">블럭이 이동할 수 있는 위치들을 담은 리스트, 보통 빈 셀공간리스트</param>
        /// <returns></returns>
        Queue<Vector3> GetWayToFlow(Vector3 start, Vector3 end , List<Vector3> wayOfempties)
        {
            // 블럭의 이동 모양을 만들 수 있는 함수!!!!!
            //1. 시작점 받고, 끝점 받고, 경로가 될 셀공간에 대한 정보를 리스트로 받는다.
            //2. 블럭 경로 저장해서 반환 ★어떤 방식으로 경로를 계산해야할까?
            //  >>★★ 어렵게생각하지 말고 같은 q라인인지 확인하고 아니면 이동하게 한후 쭈욱 밑으로 흐르게 만들자. 

            Queue<Vector3> ways = new Queue<Vector3>();
            Vector3 now = start;

            Debug.Log($"스타트의 좌표는 ? : {start}");
            Debug.Log($"엔드의 좌표는 ? : {end}");

            //1. 일단 q라인 맞추기 
            if (start.x != end.x)
            {
                if(start.x < end.x) // 시작점이 끝점 왼쪽에 있다.
                {
                    now = now.PlusCoodinate(HexArchive.HexDirections[5]);
                    Debug.Assert(wayOfempties.Contains(now),"추가하려는 경로가 비어있지 않습니다. ");
                    Debug.Log("집어넣으려는 좌표 :"+ now);
                    ways.Enqueue(now);
                }
                else // 시작점이 끝점 오른쪽에 있다.
                {

                    now = now.PlusCoodinate(HexArchive.HexDirections[3]);
                    Debug.Assert(wayOfempties.Contains(now), "추가하려는 경로가 비어있지 않습니다. ");
                    Debug.Log("집어넣으려는 좌표 :" + now);
                    ways.Enqueue(now);
                }

            }


            while (ways.Peek() != end) // 끝점을 포함하게 되면 경로추가를 그만둔다. 
            {
                ways.Enqueue(now.PlusCoodinate(HexArchive.HexDirections[4]));
                now = now.PlusCoodinate(HexArchive.HexDirections[4]);
            }

            return ways;

        }




        public void MoveBlockAlongTheBlankWay(Vector3 start, Vector3 depart, List<Vector3> empties) // start지점을 랜덤으로 뽑아서 쓰자.
        {
            //자료구조에 공부했던 걸 이용하자!!!! 

            //start 지점의 후보를 저장하는 배열을 활용해서 가운데 라인에서 우선 떨어지게 만들어주지만,
            //만약 가운데에서 갈수가 없다면 점점 바깥쪽으로 이동하면서 갈 수 있는 길이 있는 start 지점을 찾아주는 함수를 만들자.
            //  이 함수를 쓰기 전에  헥사타일 중간지점에서 바로 ↖ ↗ 이웃하는 셀이 떨어져야 예쁜 구조인지 검사해보고 바로 이 함수로 들어오게 해주는 것도 좋을 것 같다.


            // 비어진 경로 안에서만 
            Vector3[] found = new Vector3[empties.Count];
            Vector3[] parent = new Vector3[empties.Count];
            

            // ★★★ 일단 depart에서 start로 가는 길을 찾아서 경로를 만들어서 뒤집자!! :
            // >> stat로 empties에 포함시켜진 상태여야 할까?? 아니어도 될 것 같다!!!
            //일단 경로를 찾자!


            Queue<Vector3> q = new Queue<Vector3>();
            q.Enqueue(start); // 후입선출이므로 가장 먼저 스타트 지점을 큐에 넣어준다. 

            while (q.Count > 0)
            {
                Vector3 now = q.Dequeue();

                Vector3[] neighbours = now.myNeighbors();

                for (int i = 0; i < neighbours.Length; i++)
                {
                    if (!hexBlockItems.ContainsKey(neighbours[i]) || !empties.Contains(neighbours[i]))
                    { //좌표가 보드에 없는 좌표이거나, 비어진 블럭좌표가 아니면 스킵한다. 
                        continue;
                    }

                    //흘러내려가는 느낌이어야 하는데 depart에서>stat로 가므로 일단 올라가는 좌표만 찾아야한다
                    // >>> 그래서 좌표(q, r, s)에서 r은 작아져야 하고, s는 커져야 한다. 
                    if (now.y > neighbours[i].y || now.z < neighbours[i].z)
                    {
                        continue;
                    }

                    q.Enqueue(neighbours[i]);
                    //ound.

            }
            }
            


            //경로가 있는지도 확인해야함 (중간에 막혀있을 수도 있잖아!!)
            // >> 이건 우리가 찾은 경로Vector 리스트의 마지막 저장물과 
            

            //Neighbor 좌표를 반환해주는 함수를 활용할 것이다. 



        }



        #endregion 블럭 이동 명령

        #region 블럭 제거 명령/관리


        public void DoDestroyBlocks()
        {

            List<Vector3> MatchedBlock = GetMathcedBlockInfo();
            HexBlock matched;

            foreach (Vector3 cordinate in MatchedBlock)
            {
                matched = hexBlockItems[cordinate];

                if (matched !=null)
                {
                    StartCoroutine(matched.DestroyBlock(Constants.DestroyDuraion));

                    //정보 제거 
                    hexBlockItems[matched.CellCordinate] = null;
                    m_hexBlockResults[matched.CellCordinate] = BlockCandyType.NA;
                }

            }



        }

        #endregion 블럭 제거 명령/관리



        #region 블럭 좌표 이용/관리

        public Vector2 GetBlockposition(Vector3 coordinate) //헥사셀 위치와 헥사블럭위치가 살짝 다른 문제 있어서 구분해서 써야함!
        {
            Vector2 myPosition = m_HexGrid.GetCellPosition(coordinate);

            //블럭값을 보정한다.
            //myPosition.y += offstBlockPositionY;

            return myPosition;

        }


        /// <summary>
        /// 블럭 정보가 바뀔 때마다 업데이트 하는 함수 >>각 블럭이 개별적으로 실행하도록 하자.
        /// </summary>
        public void UpdtateChangedInfo(HexBlock own, Vector3 chandedCor)
        {
            //블럭 컨트롤러에 내부 리스트 수정
            own.ChangeCoordinate(chandedCor);
            hexBlockItems[chandedCor] = own;
            m_hexBlockResults[chandedCor] = own.CandyType;
            m_blockBuilder.UpdateListRef_cordinate();


        }

        public List<Vector3> GetMathcedBlockInfo()
        {
            return m_blockBuilder.GetAllMatchVector3coordinatesList();
        }

        #endregion  블럭 좌표 이용/관리






        //블럭 비주얼/상태
        public Sprite GetNormalBlockSprite(BlockCandyType candy)
        {
            Sprite CandySprite = normalBlockConfig.NormalBlocks[(int)candy];

            return CandySprite;

        }


  
    }


}


