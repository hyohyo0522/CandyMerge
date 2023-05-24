using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hyo.core;
using Hyo.Util;
using Random = UnityEngine.Random;

namespace Hyo.HexItems
{
    public class BlockBuilder
    {
        // Start is called before the first frame update

        //초기생성값
        HexBlockController m_bController;
        List<Vector3> allCoordinateOnGame;
        public bool Created_hexBlockResults { get; private set; }
        public bool shuffled { get; private set; }

        // 최종 블록 빌드값 >> HexBlockController에 넘겨줄 것이다.
        //Dictionary<Vector3, HexBlock> hexBlockResults = new Dictionary<Vector3, HexBlock>();
        Dictionary<Vector3, BlockCandyType> hexBlockResults = new Dictionary<Vector3, BlockCandyType>();
        public Dictionary<Vector3, BlockCandyType> HexBlockResults()
        {
            Debug.Assert(Created_hexBlockResults); //일단, 처음 셔플하고 블럭 생성될 때만 쓸 수 있게 일단 이 디버깅 추가! 
            return hexBlockResults;
        }


        #region 각 좌표당 라인 관리(중복검사)를 위한 값
        //참고자료 : https://www.redblobgames.com/grids/hexagons >> Cube cooordinates 좌표 사용하여 라인별 검사를 쭉 진행하기로 함

        // 각 검사 진행 방향 : q-Up / r-RightUp / s-LeftUp
        Vector3 q_inspectionDir = HexArchive.HexDirections[1];
        Vector3 r_inspectionDir = HexArchive.HexDirections[0];
        Vector3 s_inspectionDir = HexArchive.HexDirections[2];

        #endregion 각 좌표당 라인 관리(중복검사)를 위한 값
        


        //생성자
        public BlockBuilder(HexBlockController myBcon, List<Vector3> gameCoordinates)
        {
            m_bController = myBcon;
            allCoordinateOnGame = gameCoordinates;

            //여기서 이제 위 정보들을 가지고 좌표관리를 위한 값들을 지정해주자!
            CreateRandomBlocks(allCoordinateOnGame);
            Created_hexBlockResults = true;
        }




        #region 블럭정보갱신


        /// <summary>
        /// 블럭을 제거한 후 이동하고 나서 실행되어야 한다. 
        /// </summary>
        /// <param name="changedCor"></param>
        public void UpdateListRef_cordinate()
        {

            hexBlockResults = m_bController.HexBlockResults;
        }




        #endregion  블럭정보갱신


        #region 블럭 셔플


        public void BlockSuffle()
        {

            while(GetAllMatchVector3coordinatesList().Count>0) //중복좌표 리스트가 없을 동안
            {
                CreateRandomBlocks(GetAllMatchVector3coordinatesList());
            }

            if (!shuffled) shuffled = true;
        }


        void CreateRandomBlocks(List<Vector3> corList)
        {
            
            foreach(Vector3 coodinate in corList)
            {
                int random = Random.Range(0, System.Enum.GetValues(typeof(BlockCandyType)).Length - 2);

                BlockCandyType myType = (BlockCandyType)random;

                if (hexBlockResults.ContainsKey(coodinate))
                {
                    hexBlockResults[coodinate] = myType;
                }
                else
                {
                    hexBlockResults.Add(coodinate, myType);
                }

            }

        }




        #endregion 블럭 셔플


        #region 블럭 중복 검사


        /// <summary>
        /// 햔재 매치된 블럭의 좌표값들 얻는 함수 > 초기 블럭셔플이나, 매치 검사에 사용하자.
        /// </summary>
        /// <returns></returns>
        public List<Vector3> GetAllMatchVector3coordinatesList() //  
        {
            //이거 사용할 것임
            /*  Dictionary<Vector3, BlockCandyType> hexBlockResults  */

            // 참고할 Dic 이 Null값인지 검사 >>나중에 카운트 검사하려나??

            Debug.Assert(hexBlockResults != null, "검사하려는 Dictionary가 Null값입니다. 확인해주세요.");

            //라인 마칠때마다 mathcesSaved 값 저장해주고 최종적으로 전체 매치 블럭 결과 담을 함수
            List<Vector3> resultOfAllMatchedCooordinate = new List<Vector3>();



            //q축 검사 
            Vector3 axis = q_inspectionDir;
            resultOfAllMatchedCooordinate.AddRangeNoDuplication(MatchCheckWithHexAxis(q_inspectionDir)); //q축 검사한 값 추가

            //r축 검사
            axis = r_inspectionDir;
            resultOfAllMatchedCooordinate.AddRangeNoDuplication(MatchCheckWithHexAxis(r_inspectionDir)); //r축 검사한 값 추가


            //s축 검사
            axis = s_inspectionDir;
            resultOfAllMatchedCooordinate.AddRangeNoDuplication(MatchCheckWithHexAxis(s_inspectionDir)); //r축 검사한 값 추가

            return resultOfAllMatchedCooordinate;


        }

        /// <summary>
        /// 해당 hexBlockResults를 이용하여 헥사 축(q,r,s) 기준으로 중복좌표리스트 반환하는 함수...만들기 너무 힘들었다..
        /// </summary>
        /// <param name="axixCheckMatch">기준이 되는 헥사 축(q,r,s)의 방향 벡터 ^---^</param>
        /// <returns></returns>
        private List<Vector3> MatchCheckWithHexAxis(Vector3 axixCheckMatch) // 
        {
            //새로운 축 검사시마다 갱신해줘야 하는 값들 
            List<Vector3> mathcesSaved = new List<Vector3>(); //새 축 검사시 매칭된 블록좌표값 저장 >> 큐를 사용하면 좀더 좋은 점이 있었을까?
            List<Vector3> Instected = new List<Vector3>(); // 검사완료리스트 ,검사완료값 저장 : 새로운 라인 검사할 때마다 클리어하고 다시 써준다. 

            //q라인 검사 
            Vector3 axisDir = axixCheckMatch;
            foreach (KeyValuePair<Vector3, BlockCandyType> nowBlock in hexBlockResults)
            {

                //BlockCandyType nowType = BlockCandyType.NA; //초기값
                int matchCount = 1;
                Vector3 myPos = nowBlock.Key;

                // while문 한번 돌릴 때 헥사타일의 축의 한 라인을 지난다고 보면 된다. 
                while (true) //타입이 같으면 다음값 진행하며 쭈욱 진행됨 
                {

                    Vector3 nextPos = (myPos).PlusCoodinate(axisDir);
                    Vector3 PreviousPos = (myPos).PlusCoodinate(-(axisDir));

                    //지금 내가 내 포스가 이미 예전에 검사한 것인지 확인 >> 이미 검사한 것이면 조건 확인한 후 무조건 While문>break로 빠져나가기
                    if (Instected.Contains(myPos))
                    {
                        if (matchCount >= 2)
                        {
                            if (mathcesSaved.Contains(myPos))
                            {//이 상황에서 내가 저장된 값이면 다음값이 존재하면 나랑 같은 값으로 저장되어있다고 보면 된다.
                             // 아니면 이미 이전 값들과 같아서 저장되어 있다고 보면 된다.
                                break;
                            }
                            else // 내가 저장이 안되었다면  다음순서 있는지 확인해서 다음셀 있으면 다음셀 나랑 같은지 확인/
                            {
                                //다음값이 존재하는지 검사한다.
                                if (hexBlockResults.ContainsKey(nextPos))
                                {
                                    if (hexBlockResults[myPos] == hexBlockResults[nextPos]) //다음값도 나랑 같으면 저장
                                    {
                                        mathcesSaved.Add(myPos);
                                        mathcesSaved.Add(nextPos);

                                        //여기서 다음값 검사완료 리스트 저장 안되있으면 저장해줘야 하나?? >>굳이..??
                                        break;
                                    }
                                    else //다음값 나랑 같지 않은데  matchCount가 2면 뒤에값 빼줘야함? >> TODO 이 아래로 뭔가 더 세련되게 적을 수 있을 것 같은데 아직 잘 모르곘다 ㅠㅠ
                                    {
                                        if (matchCount == 2)
                                        {
                                            mathcesSaved.Remove(PreviousPos);
                                        }
                                        else
                                        {
                                            //그런데 3 이상이면 나는 저장해줘야함
                                            mathcesSaved.Add(myPos);
                                        }
                                    }
                                }
                                else //다음값이 존재하지 않으면?
                                {
                                    if (matchCount == 2)
                                    {
                                        mathcesSaved.Remove(PreviousPos);
                                    }
                                    else
                                    {
                                        //그런데 3 이상이면 나는 저장해줘야함
                                        mathcesSaved.Add(myPos);
                                    }
                                }

                                break;
                            }
                        }

                        //현재값이 이미 검사한 값인데 maxCount가 1이면 나간다.
                        break;
                    }


                    // 다음 키값이 존재하는 지에 대한 검사를 진행 >> 존재하지 않으면 조건 확인한 후 무조건 While문>break로 빠져나가기
                    if (!hexBlockResults.ContainsKey(nextPos))
                    {
                        if (matchCount == 2) //이미 이전값은 지금 값과 같아서 저장한 상태
                        {
                            //여기서는 이미 matchCount == 2이기 때문에 이전값이 있다고 보고 검사 따로 하지 말자.

                            // 오류 체크 위한 디버깅 
                            if (!hexBlockResults.ContainsKey(PreviousPos))
                            {

                            }

                            Debug.Assert(hexBlockResults.ContainsKey(PreviousPos), "이전 값이 없는데, matchCount가 2입니다. 확인하세요"); //있어서는 안되는 상황!
                            mathcesSaved.Remove(PreviousPos); //이전값 매치리스트에서 빼주기
                        }
                        else if (matchCount >= 3)
                        {
                            mathcesSaved.Add(myPos); //나를 매치리스트에 추가하기
                        }

                        Instected.Add(myPos); //나를 검사한 값에 추가 
                        //다음키값이 없으므로 빠져나온다. 
                        break;
                    }


                    //이제 여기서부터는 다음키값도 존재하고, 이미 검사한 값도 아닌상황

                    //나랑 다음값이 같은지 확인하고 >> 다음값과 내가 같으면 일단 저장함 
                    if (hexBlockResults[myPos] == hexBlockResults[nextPos]) //나랑 다음값이 같다.
                    {
                        mathcesSaved.Add(myPos);
                        Instected.Add(myPos);
                        matchCount++;
                    }
                    else //나랑 다음 값이 다르다. 
                    {
                        if (matchCount == 2)
                        {
                            //여기서는 이미 matchCount == 2이기 때문에 이전값이 있다고 보고 검사 따로 하지 말자.
                            Debug.Assert(hexBlockResults.ContainsKey(PreviousPos), "이전 값이 없는데, matchCount가 2입니다. 확인하세요"); //있어서는 안되는 상황!
                            // 정상 !
                            mathcesSaved.Remove(PreviousPos);
                        }

                        if (matchCount >= 3)
                        {
                            mathcesSaved.Add(myPos);
                        }

                        Instected.Add(myPos); //나를 검사한 식에 넣는다. 
                        matchCount = 1;  //매치 카운트 초기화해준다.
                    }


                    //다음 while 진행될 수 있도록 값들 갱신해서 마무리해야함
                    myPos = nextPos;
                }

            }


            return mathcesSaved;
        }

        #endregion 블럭 중복 검사


        #region 떨어지는 블럭 구현
        
        public List<Vector3> allEmptyBlock()  //비어있는 블럭 리스트를 구하는 함수 
        {
            List<Vector3> b_empty = new List<Vector3>();

            //q라인 검사 
            Vector3 axisDir = HexArchive.HexDirections[4];
            foreach (KeyValuePair<Vector3, BlockCandyType> nowBlock in hexBlockResults)
            {
                Vector3 downBlockPos = nowBlock.Key.PlusCoodinate(axisDir);

                if (hexBlockResults.ContainsKey(downBlockPos) && !b_empty.Contains(downBlockPos)) //밑좌표로 만든 블럭이 게임상에 존재하는것인가?
                {
                    if (hexBlockResults[downBlockPos] == BlockCandyType.NA)
                    {
                        b_empty.Add(downBlockPos);

                    }
                } 

            }

            return b_empty;
        }


        //q라인별로 비어있는 블럭 리스트 정리하는 함수 >> 이건 Axial coordinates 자료를 사용하면 좋을 것 같다! 


        //주어진 리스트에서 q축 최솟값 뽑는 함수
        int q_MinByListVecto3(List<Vector3> list)
        {
            int min = 0;
            foreach(Vector3 value in list)
            {
                if (value.x < min)
                {
                    min = (int)value.x;
                }
            }
            return min;
        }

        //주어진 리스트에서 q축 최대값 뽑는 함수
        int q_MaXByListVecto3(List<Vector3> list)
        {
            int max = 0;
            foreach (Vector3 value in list)
            {
                if (value.x < max)
                {
                    max = (int)value.x;
                }
            }
            return max;
        }




        #endregion  떨어지는 블럭 구현

    }



}

