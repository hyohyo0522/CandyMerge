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

        //블럭생성
        void CreateHexBlock(Vector3 coordinate, BlockCandyType Type)
        {
            if(Type == BlockCandyType.SPECIAL)
            {
                //나중에 스페셜 블록 만들 때 여기서 어떤 로직 추가 하면 좋을 것 같다.
                return;
            }
            GameObject go = Instantiate(hexBlockPrefab, transform.position, Quaternion.identity);
            go.transform.parent = _prefabContainerInHierarchy.transform;


            HexBlock block = go.AddComponent<HexBlock>(); //모노비헤이비어이기 때문에 이런식으로 프리팹과 연결 
            block.Init(this, coordinate, Type, go);
            hexBlockItems.Add(coordinate,block);

        }



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
            HexBlock target = hexBlockItems[targetCoordinate];



            //2. 스와이프 가능한 블럭인지 체크한다. 

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

            //3. 스와이프 전 이동전 위치를 저장한다.
            HexBlock start = hexBlockItems[blockCordinate];

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

            DoDestroyBlocks(); //여기인가??


            // 빈 공간에 흐르는 블럭 만들기 : 일단 실험삼아 어떻게 나오나 해보자! 
            orderFlowBlocks(m_blockBuilder.allEmptyBlock());

            yield break;

        }


        //만들었지만 문제가 많은 상태로 제출합니다! ㅠ-ㅠ 
        public void orderFlowBlocks(List<Vector3> empties) // 좀 더 세련된 방법 + 좌우로 흐르는 방법 고민!!!
        {
            foreach (Vector3 blockPos in empties)
            {
                Vector3 upCor = blockPos.PlusCoodinate(HexArchive.HexDirections[1]);

                if (hexBlockItems.ContainsKey(upCor))
                {
                    //원래값 저장 
                    Vector2 targetPos = GetBlockposition(blockPos);
                    Vector3 upOriginCor = upCor;

                    //블럭 구하기
                    HexBlock UpBlock = hexBlockItems[upCor];

                    if (UpBlock != null)
                    {
                        StartCoroutine(UpBlock.MoveTo(targetPos, Constants.SwipeDuration));
                        UpBlock.ChangeCoordinate(blockPos);
                        UpdtateChangedInfo(UpBlock, UpBlock.CellCordinate);

                        //원래 업블럭이었던 건의 값 정리
                        m_hexBlockResults[upOriginCor] = BlockCandyType.NA;
                        hexBlockItems[upOriginCor] = null; //터치 눌문제 발생할 것임 >> 해결해야함!!
                    }

                }
                else 
                {
                    // 왜 여기 안들어올까?
                    Debug.Log("안 들어오나요?");
                    Vector2 targetPos = GetBlockposition(blockPos);
                    Vector3 upOriginCor = upCor;

                    // 여기서 블럭 타입 정해주는 로직 멋지게 짜면 좋을 것 같은데 .ㅠ-ㅠ
                    int random = Random.Range(0, System.Enum.GetValues(typeof(BlockCandyType)).Length - 2);

                    CreateHexBlock(upCor, (BlockCandyType)random);
                    HexBlock UpBlock = hexBlockItems[upCor];

                    StartCoroutine(UpBlock.MoveTo(targetPos, Constants.SwipeDuration));
                    UpBlock.ChangeCoordinate(blockPos);
                    UpdtateChangedInfo(UpBlock, UpBlock.CellCordinate);

                    //원래 업블럭이었던 건의 값 정리
                    m_hexBlockResults[upOriginCor] = BlockCandyType.NA;
                    hexBlockItems[upOriginCor] = null; //터치 눌문제 발생할 것임 >> 해결해야함!!


                }
                DoDestroyBlocks();

            }

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


