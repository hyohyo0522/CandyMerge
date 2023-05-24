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

        //�ʱ������
        HexBlockController m_bController;
        List<Vector3> allCoordinateOnGame;
        public bool Created_hexBlockResults { get; private set; }
        public bool shuffled { get; private set; }

        // ���� ��� ���尪 >> HexBlockController�� �Ѱ��� ���̴�.
        //Dictionary<Vector3, HexBlock> hexBlockResults = new Dictionary<Vector3, HexBlock>();
        Dictionary<Vector3, BlockCandyType> hexBlockResults = new Dictionary<Vector3, BlockCandyType>();
        public Dictionary<Vector3, BlockCandyType> HexBlockResults()
        {
            Debug.Assert(Created_hexBlockResults); //�ϴ�, ó�� �����ϰ� �� ������ ���� �� �� �ְ� �ϴ� �� ����� �߰�! 
            return hexBlockResults;
        }


        #region �� ��ǥ�� ���� ����(�ߺ��˻�)�� ���� ��
        //�����ڷ� : https://www.redblobgames.com/grids/hexagons >> Cube cooordinates ��ǥ ����Ͽ� ���κ� �˻縦 �� �����ϱ�� ��

        // �� �˻� ���� ���� : q-Up / r-RightUp / s-LeftUp
        Vector3 q_inspectionDir = HexArchive.HexDirections[1];
        Vector3 r_inspectionDir = HexArchive.HexDirections[0];
        Vector3 s_inspectionDir = HexArchive.HexDirections[2];

        #endregion �� ��ǥ�� ���� ����(�ߺ��˻�)�� ���� ��
        


        //������
        public BlockBuilder(HexBlockController myBcon, List<Vector3> gameCoordinates)
        {
            m_bController = myBcon;
            allCoordinateOnGame = gameCoordinates;

            //���⼭ ���� �� �������� ������ ��ǥ������ ���� ������ ����������!
            CreateRandomBlocks(allCoordinateOnGame);
            Created_hexBlockResults = true;
        }




        #region ����������


        /// <summary>
        /// ���� ������ �� �̵��ϰ� ���� ����Ǿ�� �Ѵ�. 
        /// </summary>
        /// <param name="changedCor"></param>
        public void UpdateListRef_cordinate()
        {

            hexBlockResults = m_bController.HexBlockResults;
        }




        #endregion  ����������


        #region �� ����


        public void BlockSuffle()
        {

            while(GetAllMatchVector3coordinatesList().Count>0) //�ߺ���ǥ ����Ʈ�� ���� ����
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




        #endregion �� ����


        #region �� �ߺ� �˻�


        /// <summary>
        /// �h�� ��ġ�� ���� ��ǥ���� ��� �Լ� > �ʱ� �������̳�, ��ġ �˻翡 �������.
        /// </summary>
        /// <returns></returns>
        public List<Vector3> GetAllMatchVector3coordinatesList() //  
        {
            //�̰� ����� ����
            /*  Dictionary<Vector3, BlockCandyType> hexBlockResults  */

            // ������ Dic �� Null������ �˻� >>���߿� ī��Ʈ �˻��Ϸ���??

            Debug.Assert(hexBlockResults != null, "�˻��Ϸ��� Dictionary�� Null���Դϴ�. Ȯ�����ּ���.");

            //���� ��ĥ������ mathcesSaved �� �������ְ� ���������� ��ü ��ġ �� ��� ���� �Լ�
            List<Vector3> resultOfAllMatchedCooordinate = new List<Vector3>();



            //q�� �˻� 
            Vector3 axis = q_inspectionDir;
            resultOfAllMatchedCooordinate.AddRangeNoDuplication(MatchCheckWithHexAxis(q_inspectionDir)); //q�� �˻��� �� �߰�

            //r�� �˻�
            axis = r_inspectionDir;
            resultOfAllMatchedCooordinate.AddRangeNoDuplication(MatchCheckWithHexAxis(r_inspectionDir)); //r�� �˻��� �� �߰�


            //s�� �˻�
            axis = s_inspectionDir;
            resultOfAllMatchedCooordinate.AddRangeNoDuplication(MatchCheckWithHexAxis(s_inspectionDir)); //r�� �˻��� �� �߰�

            return resultOfAllMatchedCooordinate;


        }

        /// <summary>
        /// �ش� hexBlockResults�� �̿��Ͽ� ��� ��(q,r,s) �������� �ߺ���ǥ����Ʈ ��ȯ�ϴ� �Լ�...����� �ʹ� �������..
        /// </summary>
        /// <param name="axixCheckMatch">������ �Ǵ� ��� ��(q,r,s)�� ���� ���� ^---^</param>
        /// <returns></returns>
        private List<Vector3> MatchCheckWithHexAxis(Vector3 axixCheckMatch) // 
        {
            //���ο� �� �˻�ø��� ��������� �ϴ� ���� 
            List<Vector3> mathcesSaved = new List<Vector3>(); //�� �� �˻�� ��Ī�� �����ǥ�� ���� >> ť�� ����ϸ� ���� ���� ���� �־�����?
            List<Vector3> Instected = new List<Vector3>(); // �˻�ϷḮ��Ʈ ,�˻�Ϸᰪ ���� : ���ο� ���� �˻��� ������ Ŭ�����ϰ� �ٽ� ���ش�. 

            //q���� �˻� 
            Vector3 axisDir = axixCheckMatch;
            foreach (KeyValuePair<Vector3, BlockCandyType> nowBlock in hexBlockResults)
            {

                //BlockCandyType nowType = BlockCandyType.NA; //�ʱⰪ
                int matchCount = 1;
                Vector3 myPos = nowBlock.Key;

                // while�� �ѹ� ���� �� ���Ÿ���� ���� �� ������ �����ٰ� ���� �ȴ�. 
                while (true) //Ÿ���� ������ ������ �����ϸ� �޿� ����� 
                {

                    Vector3 nextPos = (myPos).PlusCoodinate(axisDir);
                    Vector3 PreviousPos = (myPos).PlusCoodinate(-(axisDir));

                    //���� ���� �� ������ �̹� ������ �˻��� ������ Ȯ�� >> �̹� �˻��� ���̸� ���� Ȯ���� �� ������ While��>break�� ����������
                    if (Instected.Contains(myPos))
                    {
                        if (matchCount >= 2)
                        {
                            if (mathcesSaved.Contains(myPos))
                            {//�� ��Ȳ���� ���� ����� ���̸� �������� �����ϸ� ���� ���� ������ ����Ǿ��ִٰ� ���� �ȴ�.
                             // �ƴϸ� �̹� ���� ����� ���Ƽ� ����Ǿ� �ִٰ� ���� �ȴ�.
                                break;
                            }
                            else // ���� ������ �ȵǾ��ٸ�  �������� �ִ��� Ȯ���ؼ� ������ ������ ������ ���� ������ Ȯ��/
                            {
                                //�������� �����ϴ��� �˻��Ѵ�.
                                if (hexBlockResults.ContainsKey(nextPos))
                                {
                                    if (hexBlockResults[myPos] == hexBlockResults[nextPos]) //�������� ���� ������ ����
                                    {
                                        mathcesSaved.Add(myPos);
                                        mathcesSaved.Add(nextPos);

                                        //���⼭ ������ �˻�Ϸ� ����Ʈ ���� �ȵ������� ��������� �ϳ�?? >>����..??
                                        break;
                                    }
                                    else //������ ���� ���� ������  matchCount�� 2�� �ڿ��� �������? >> TODO �� �Ʒ��� ���� �� ���õǰ� ���� �� ���� �� ������ ���� �� �𸣁ٴ� �Ф�
                                    {
                                        if (matchCount == 2)
                                        {
                                            mathcesSaved.Remove(PreviousPos);
                                        }
                                        else
                                        {
                                            //�׷��� 3 �̻��̸� ���� �����������
                                            mathcesSaved.Add(myPos);
                                        }
                                    }
                                }
                                else //�������� �������� ������?
                                {
                                    if (matchCount == 2)
                                    {
                                        mathcesSaved.Remove(PreviousPos);
                                    }
                                    else
                                    {
                                        //�׷��� 3 �̻��̸� ���� �����������
                                        mathcesSaved.Add(myPos);
                                    }
                                }

                                break;
                            }
                        }

                        //���簪�� �̹� �˻��� ���ε� maxCount�� 1�̸� ������.
                        break;
                    }


                    // ���� Ű���� �����ϴ� ���� ���� �˻縦 ���� >> �������� ������ ���� Ȯ���� �� ������ While��>break�� ����������
                    if (!hexBlockResults.ContainsKey(nextPos))
                    {
                        if (matchCount == 2) //�̹� �������� ���� ���� ���Ƽ� ������ ����
                        {
                            //���⼭�� �̹� matchCount == 2�̱� ������ �������� �ִٰ� ���� �˻� ���� ���� ����.

                            // ���� üũ ���� ����� 
                            if (!hexBlockResults.ContainsKey(PreviousPos))
                            {

                            }

                            Debug.Assert(hexBlockResults.ContainsKey(PreviousPos), "���� ���� ���µ�, matchCount�� 2�Դϴ�. Ȯ���ϼ���"); //�־�� �ȵǴ� ��Ȳ!
                            mathcesSaved.Remove(PreviousPos); //������ ��ġ����Ʈ���� ���ֱ�
                        }
                        else if (matchCount >= 3)
                        {
                            mathcesSaved.Add(myPos); //���� ��ġ����Ʈ�� �߰��ϱ�
                        }

                        Instected.Add(myPos); //���� �˻��� ���� �߰� 
                        //����Ű���� �����Ƿ� �������´�. 
                        break;
                    }


                    //���� ���⼭���ʹ� ����Ű���� �����ϰ�, �̹� �˻��� ���� �ƴѻ�Ȳ

                    //���� �������� ������ Ȯ���ϰ� >> �������� ���� ������ �ϴ� ������ 
                    if (hexBlockResults[myPos] == hexBlockResults[nextPos]) //���� �������� ����.
                    {
                        mathcesSaved.Add(myPos);
                        Instected.Add(myPos);
                        matchCount++;
                    }
                    else //���� ���� ���� �ٸ���. 
                    {
                        if (matchCount == 2)
                        {
                            //���⼭�� �̹� matchCount == 2�̱� ������ �������� �ִٰ� ���� �˻� ���� ���� ����.
                            Debug.Assert(hexBlockResults.ContainsKey(PreviousPos), "���� ���� ���µ�, matchCount�� 2�Դϴ�. Ȯ���ϼ���"); //�־�� �ȵǴ� ��Ȳ!
                            // ���� !
                            mathcesSaved.Remove(PreviousPos);
                        }

                        if (matchCount >= 3)
                        {
                            mathcesSaved.Add(myPos);
                        }

                        Instected.Add(myPos); //���� �˻��� �Ŀ� �ִ´�. 
                        matchCount = 1;  //��ġ ī��Ʈ �ʱ�ȭ���ش�.
                    }


                    //���� while ����� �� �ֵ��� ���� �����ؼ� �������ؾ���
                    myPos = nextPos;
                }

            }


            return mathcesSaved;
        }

        #endregion �� �ߺ� �˻�


        #region �������� �� ����
        
        public List<Vector3> allEmptyBlock()  //����ִ� �� ����Ʈ�� ���ϴ� �Լ� 
        {
            List<Vector3> b_empty = new List<Vector3>();

            //q���� �˻� 
            Vector3 axisDir = HexArchive.HexDirections[4];
            foreach (KeyValuePair<Vector3, BlockCandyType> nowBlock in hexBlockResults)
            {
                Vector3 downBlockPos = nowBlock.Key.PlusCoodinate(axisDir);

                if (hexBlockResults.ContainsKey(downBlockPos) && !b_empty.Contains(downBlockPos)) //����ǥ�� ���� ���� ���ӻ� �����ϴ°��ΰ�?
                {
                    if (hexBlockResults[downBlockPos] == BlockCandyType.NA)
                    {
                        b_empty.Add(downBlockPos);

                    }
                } 

            }

            return b_empty;
        }


        //q���κ��� ����ִ� �� ����Ʈ �����ϴ� �Լ� >> �̰� Axial coordinates �ڷḦ ����ϸ� ���� �� ����! 


        //�־��� ����Ʈ���� q�� �ּڰ� �̴� �Լ�
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

        //�־��� ����Ʈ���� q�� �ִ밪 �̴� �Լ�
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




        #endregion  �������� �� ����

    }



}

