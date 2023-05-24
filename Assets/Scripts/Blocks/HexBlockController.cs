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

        //��������
        public GameObject hexBlockPrefab; // Ʈ�������� ��������Ʈ�� ������ ģ��, �ν����Ϳ��� �Ҵ�
        public Transform _prefabContainerInHierarchy;
        [SerializeField] NormalBlockConfig normalBlockConfig;

        //�� �����
        Dictionary<Vector3,HexBlock> hexBlockItems = new Dictionary<Vector3, HexBlock>();

        //��Ÿ�������
        Dictionary<Vector3, BlockCandyType> m_hexBlockResults = new Dictionary<Vector3, BlockCandyType>();
        public Dictionary<Vector3, BlockCandyType> HexBlockResults { get { return m_hexBlockResults; } }

        //��� ���� Ŭ����
        HexGrid m_HexGrid; //��� �׸��� >> �� ������ ��ǥ�� ���� 
        BlockBuilder m_blockBuilder; // �� ���� >> �� ���ð� ��� �ߺ� �˻� 

        //�� ���� ����
        bool m_bRunning; // �׼� ���� ���� : �������� ��� true


        private void Start()
        {
            Debug.Log("Start �Լ� ����");
            m_HexGrid = this.gameObject.GetComponent<HexGrid>();
            m_blockBuilder = new BlockBuilder(this, m_HexGrid.gameGridCoordinates);

            m_blockBuilder.BlockSuffle();
            m_hexBlockResults = m_blockBuilder.HexBlockResults();

            foreach(KeyValuePair<Vector3, BlockCandyType> item in m_hexBlockResults)
            {
                //���߿� ������info.cs�� ��������info ���� �� �̿��ؼ� Ư������ ������ ��ǥ���� ���⼭ �ɷ����� ������ ���� �� ���� ������??
                CreateHexBlock(item.Key, item.Value);
            }

        }

        #region ������

        //������
        void CreateHexBlock(Vector3 coordinate, BlockCandyType Type)
        {
            if (Type == BlockCandyType.SPECIAL)
            {
                //���߿� ����� ��� ���� �� ���⼭ � ���� �߰� �ϸ� ���� �� ����.
                return;
            }
            GameObject go = Instantiate(hexBlockPrefab, transform.position, Quaternion.identity);
            go.transform.parent = _prefabContainerInHierarchy.transform;


            HexBlock block = go.AddComponent<HexBlock>(); //�������̺���̱� ������ �̷������� �����հ� ���� 
            block.Init(this, coordinate, Type, go);


            if (hexBlockItems.ContainsKey(coordinate))
            {
                //���� �˻� >> �� ������ �ߺ��� ���� ���ִ��� Ȯ��
                Debug.Assert(hexBlockItems[coordinate] == null, "���� �����ϰ��� �ϳ� �̹� ���� �ֽ��ϴ�.");

                hexBlockItems[coordinate] = block;
            }
            else
            {
                hexBlockItems.Add(coordinate, block);
            }


        }

        /// <summary>
        /// ���� ���߿� ���, ���ϴ� ��ǥ�� ���ο� �� �������� ����, �������ϴ� ��ǥ�� ���Ӻ����ʿ��� ��ġ�ؾ��� 
        /// </summary>
        /// <param name="coordinate"></param>
        void CreateRandomHexBlock(Vector3 coordinate)
        {
            int random = Random.Range(0, System.Enum.GetValues(typeof(BlockCandyType)).Length - 2);
            BlockCandyType myType = (BlockCandyType)random;

            CreateHexBlock(coordinate, myType);


            Debug.Assert(m_hexBlockResults.ContainsKey(coordinate),"��Ÿ�Ը���Ʈ�� �̸� ������ ��ǥŰ�� �����ϴ�."); 
            // >> �������ϴ� ��ġ�� �׸��� ���� ��ġ������ �� �� �ֵ��� �� 
            
            Debug.Assert(m_hexBlockResults[coordinate] == BlockCandyType.NA, "��Ÿ���� �����ϰ��� �ϳ� �̹� ��Ÿ�������� �ֽ��ϴ�");
            m_hexBlockResults[coordinate] = myType;




        }

        #endregion ������ 





        #region �� �̵� ���

        //�� �̵�����

        public void DoSwipAction(Vector3 blockCordinate, HexSwipe swipeDir)
        {
            //�˻�� �ʿ��� �� ������ �ֱ�
            Debug.Assert(hexBlockItems.ContainsKey(blockCordinate), "���������� ���� �������� �ʽ��ϴ�.");

            StartCoroutine(CoDoSwipeAction(blockCordinate, swipeDir));


        }

        IEnumerator CoDoSwipeAction(Vector3 blockCordinate, HexSwipe swipeDir)
        {
            if (!m_bRunning)  //�ٸ� �׼��� ���� ���̸� PASS
            {
                m_bRunning = true;    //�׼� ���� ���� ON

                //1. swipe action ����
                Returnable<bool> bSwipedBlock = new Returnable<bool>(false);


                yield return SwipeAction(blockCordinate, swipeDir, bSwipedBlock);


                m_bRunning = false;    //�׼� ���� ���� OFF
                //�ٸ� �׼� ���� �� ������ �ϴ� �̷��� ��������. 



            }


            yield break;
        }

        public IEnumerator SwipeAction(Vector3 blockCordinate, HexSwipe swipeDir, Returnable<bool> actionResult)
        {
            Debug.Log("������� ����??");

            actionResult.value = false; //�ڷ�ƾ ���ϰ� RESET

            //1. ���������Ǵ� ��� �� ��ġ�� ���Ѵ�. (using SwipeDir Extension Method)
            //blockCordinate += swipeDir.GetTargetCol();
            Vector3 targetCoordinate = blockCordinate.PlusCoodinate(swipeDir.GetTargetSwipeCor());
            if (!hexBlockItems.ContainsKey(targetCoordinate))
            {
                Debug.Log("������ ���� ������Ͽ� �����ϴ�.");

            }

            //2. �������� �� �̵��� ��ġ�� �����Ѵ�.
            HexBlock start = hexBlockItems[blockCordinate];
            HexBlock target = hexBlockItems[targetCoordinate];

            //3. �������� ������ ������ üũ�Ѵ�. 

            #region �������� ���� ���� �˻�

            if (!target)
            {
                Debug.Log("�������� ������ ���� ���׿�!");
                yield break;

            }
            if (!hexBlockItems.ContainsKey(targetCoordinate))
            {
                Debug.Log("�����Դϴ�! �������� ������ ������ üũ�ϼ���.");
                yield break;
            };

            if (target.BlockType == BlockType.EMPTY)
            {
                Debug.Log("�����Դϴ�! �������� ������ ������ üũ�ϼ���.");
                yield break;
            };

            if (target.CandyType == BlockCandyType.NA)
            {
                Debug.Log("�����Դϴ�! �������� ������ ������ üũ�ϼ���.");
                yield break;
            };

            //TODO ���߿� ����� ������ �˻��ϴ� �׸��� ���⼭ �߰��ϴ� �� ������? 

            #endregion �������� ���� ���� �˻�



            if (!start) //Null���� ����
            {
                yield break;
            }

            Vector2 startPosition = GetBlockposition(blockCordinate);
            Vector2 targetPosition = GetBlockposition(targetCoordinate);


            //4.�������� �׼��� �����Ѵ�.
            StartCoroutine(start.MoveTo(targetPosition, Constants.SwipeDuration));
            StartCoroutine(target.MoveTo(startPosition, Constants.SwipeDuration));

            yield return new WaitForSeconds(Constants.SwipeDuration);

            //5. ��ǥ�� �ٲٱ�
            start.ChangeCoordinate(targetCoordinate);
            target.ChangeCoordinate(blockCordinate);

            //6 ������� Ű-����� �ٲٱ�
            hexBlockItems[targetCoordinate] = start;
            hexBlockItems[blockCordinate] = target;


            UpdtateChangedInfo(start,start.CellCordinate);
            UpdtateChangedInfo(target, target.CellCordinate);

            actionResult.value = false;


            //[05.24 �ڷ�ƾ�� ���� ���ذ� ������ �� ����. ]

            DoDestroyBlocks(); //�����ΰ�??

            //while (m_blockBuilder.allEmptyBlock().Count > 0)
            //{
            //    FillTheBlankOfThreeUpperHexes(m_blockBuilder.allEmptyBlock());
            //    DoDestroyBlocks();
            //}



            //FillTheBlankOfThreeUpperHexes(m_blockBuilder.allEmptyBlock());
            // �� ������ �帣�� �� ����� : �ϴ� ������ ��� ������ �غ���! 


            yield break;

        }


        public void orderFlowBlocks(List<Vector3> empties) 
        {

            //Ư����ġ���� ����ִ� ������ ������ ���� ��θ� ����ϴ� ������ Ȱ������!

            foreach (Vector3 blockPos in empties)
            {
                Vector3 upCor = blockPos.PlusCoodinate(HexArchive.HexDirections[1]);

                if (hexBlockItems[upCor] != null) 
                {
                    //������ ���� 
                    Vector2 targetPos = GetBlockposition(blockPos);
                    Vector3 upOriginCor = upCor;

                    //�� ���ϱ�
                    HexBlock UpBlock = hexBlockItems[upCor];

                    if (UpBlock != null)
                    {
                        StartCoroutine(UpBlock.MoveTo(targetPos, Constants.SwipeDuration));
                        Debug.Log($"���� ��������! : [�ٷ� ����] : {upCor}�� {blockPos} �� ��������.");
                        UpBlock.ChangeCoordinate(blockPos);
                        UpdtateChangedInfo(UpBlock, UpBlock.CellCordinate);

                        //���� �����̾��� ���� �� ����
                        m_hexBlockResults[upOriginCor] = BlockCandyType.NA;
                        hexBlockItems[upOriginCor] = null; //��ġ ������ �߻��� ���� >> �ذ��ؾ���!!
                    }

                }
                else 
                {
                    Vector2 targetPos = GetBlockposition(blockPos);
                    Vector3 upOriginCor = upCor;
                    Debug.Log($"tatgetpos[�����] : {blockPos} �� upOriginCor[�ٷ� ����]�� ��ǥ :  {upOriginCor} ");

                    /*
                    // [TODO]���⼭ �� Ÿ�� �����ִ� ���� �ֱ� ������ ¥�� ���� �� ������ .��-��
                    int random = Random.Range(0, System.Enum.GetValues(typeof(BlockCandyType)).Length - 2);

                    //[TODO] ���ο� ���� �����Ǵ� ��ġ�� �ٽ� �������! 
                    CreateHexBlock(upCor, (BlockCandyType)random);
                    HexBlock UpBlock = hexBlockItems[upCor];

                    StartCoroutine(UpBlock.MoveTo(targetPos, Constants.SwipeDuration));
                    UpBlock.ChangeCoordinate(blockPos);
                    UpdtateChangedInfo(UpBlock, UpBlock.CellCordinate);

                    //���� �����̾��� ���� �� ����
                    m_hexBlockResults[upOriginCor] = BlockCandyType.NA;
                    hexBlockItems[upOriginCor] = null; //��ġ ������ �߻��� ���� >> �ذ��ؾ���!!
                    */

                }
                DoDestroyBlocks();

            }

        }


        /// <summary>
        /// ����� ���� �� Ȥ�� ������,���������κ��� ���� ����߷� ��ĭ�� ä��� �Լ�
        /// </summary>
        public void FillTheBlankOfThreeUpperHexes(List<Vector3> empties)
        {
            bool isNewGenStart = false;
            Queue<Vector3> ways; // ���̵��� ��ǥ ������ ť 
            Vector3 start;
            Vector3 end;

            //1. ���� ���� ����ǥ���� ã�Ƽ� [��������ġ]�� ���
            end = new Vector3(99, 99, -99); //�񱳰� ������ ���ذ��� �־��ش�. 

            foreach(Vector3 item in empties)
            {
                Debug.Log($"�� ������ ���� ���� : {item}");
            }


            foreach(Vector3 item in empties)
            {
                if(item.y< end.y || item.z > end.z) // ���� ��� ���� ���� ������ �˻� 
                {
                    end = item;
                }
            }
            Debug.Log($"������ end�� ���� : {end}");

            //2. ���� ��ǥ�� ä���� ���� �ִ��� Ȯ��
            //2-1 ������ �� ���� [��ŸƮ ��]���� ����
            start = GetfilledUpperHex(end);  

            //2-2 ������ �ٷ� end��ǥ�� ���� �� �������� [��ŸƮ��]��ġ�� �����ϰ� ++  ���ο� �� �����ؾ��� 
            if (start == Constants.errorVector3) //�������� ������ start��ġ�� �����ؾ��Ѵ�. 
            {
                start = GetHighestHexAlongLine(end);
                //�������� ���ο� ������������ϹǷ� true ����
                isNewGenStart = true; // 

            }

            if (isNewGenStart) CreateRandomHexBlock(start);


            // 3. [��ŸƮ��]�� [������]���� �귯���� ��θ� ���ͼ� ���������
            ways = GetWayToFlow(start,end, empties); //��� ����


            StartCoroutine(MoveToFillTheBlank(start,end, ways));


            // 4. ����ġ�̵��� ���� ���� �����ϱ� 
            // >> �̹� �ڷ�ƾ �Լ����� �� �ǽ��� 
        }

        public IEnumerator MoveToFillTheBlank(Vector3 start, Vector3 end, Queue<Vector3> ways)
        {

            //�̵��Ǿ�� �ϴ� ��ġ ����

            //�ϴ� ����..

            HexBlock startBlock = hexBlockItems[start];

            //[TODO] ���� ��ŸƮ���� �����س�����
            //[TODO] �����Ҷ����� ����������ұ�???
            // ���ŵǾ�� �ϴ� ���� >> ���� ��ŸƮ�� 

            Vector3 nowStartCoordinate = start; // while�� ������ �ٲ� ���� ��Ÿ���� ��ǥ���� �ʱⰪ ���� 

            while (ways.Count>0)
            {

                Vector3 nextMoveCoordinate = ways.Dequeue();
                Vector2 nextPosition = GetBlockposition(nextMoveCoordinate);
                StartCoroutine(startBlock.MoveTo(nextPosition, Constants.SwipeDuration));

                yield return new WaitForSeconds(Constants.SwipeDuration);

            //[���������� �� 1.] �ű� ������ �ٲ��ش� >> �ϴ� �̰� �� 

                startBlock.ChangeCoordinate(nextMoveCoordinate);
                UpdtateChangedInfo(startBlock, startBlock.CellCordinate);
                hexBlockItems[nowStartCoordinate] = null;
                m_hexBlockResults[nowStartCoordinate] = BlockCandyType.NA;
                nowStartCoordinate = nextMoveCoordinate;

            }



        }

        /// <summary>
        /// [�ٷ� �� Ȥ�� ��������/�������� ä���� ���� ��ǥ���� ��� �Լ�] ��ȯ���� �������̸� ��,��������,�������� �ƹ� ���� ���� �����̴�. >
        /// </summary>
        /// <param name="emptyHexCell">�ٷ� ���� ��������,�������� �ִ� ���� �ִ��� �˻��ϰ� ���� ��ġ�� ��ǥ</param>
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

            //�翷�� ��� Ȱ��ȭ �� �����ϴ� ������ ��쿡�� ���⼭ �������� �̾�����.
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


            //���⼭ ���� ����Լ��� ���� ������ ��� ���ϴ��Ŀ� ����[���긮�� ������]�� �����ȴ�. 
            // �ٵ� ���� ���� �������� ������ �ٷ� ������  �� �귯������ ����� �Ͱ�
            // �ٷ� ���� �� ������ ���� ������ ������ ������� �� �� �帣�� �ϰ� �����Ƿ�
            if (!isActivated[0])
            {
                return filled; // ���� ���� �������� ������ �ٱ������� �� ���� �����ؾ� �ϹǷ� �������� ������. 
            }
            else
            {
                return GetfilledUpperHex(NextupHex); //���� ���� �����ϸ� �ѹ� �� �˻��غ���!
            }


        }

        /// <summary>
        /// q���ο��� ���� ���� ��ġ�� ����ǥ�� ��ȯ�ϴ� �Լ�, Ȱ���� ���� ���� ��ȯ
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        Vector3 GetHighestHexAlongLine(Vector3 start)
        {
            Debug.Assert(hexBlockItems.ContainsKey(start), "�߸��� ��ǥ���� �Է�");

            Vector3 hightestHex = Constants.errorVector3;
            hightestHex.z = -99; //�񱳸� ���� ���� ���� >> ���Ÿ���� �ٷ� ���� �������� q,r,s(x,y,z)�� (0,-1,+1)�Ǿ�� �Ѵ�. 
            foreach (Vector3 item in hexBlockItems.Keys)
            {
                // [TODO] �ι�° ���� �ٽ� �����ϱ�!!!!!! R�� S ��� �˻��ؾ��� �� ����!!!!
                if (item.x == start.x && hightestHex.y > item.y && hightestHex.z < item.z)
                {
                    hightestHex = item;
                }
            }
            return hightestHex;
        }

        /// <summary>
        /// ���� ������ ��θ� List<Vector3>�� ��ȯ���ִ� �Լ�, ��δ� �������� �����ϰ� ������������ ������ ���
        /// </summary>
        /// <param name="start">�� ������ġ</param>
        /// <param name="end">�� ����ġ</param>
        /// <param name="wayOfempties">���� �̵��� �� �ִ� ��ġ���� ���� ����Ʈ, ���� �� ����������Ʈ</param>
        /// <returns></returns>
        Queue<Vector3> GetWayToFlow(Vector3 start, Vector3 end , List<Vector3> wayOfempties)
        {
            // ���� �̵� ����� ���� �� �ִ� �Լ�!!!!!
            //1. ������ �ް�, ���� �ް�, ��ΰ� �� �������� ���� ������ ����Ʈ�� �޴´�.
            //2. �� ��� �����ؼ� ��ȯ �ھ ������� ��θ� ����ؾ��ұ�?
            //  >>�ڡ� ��ưԻ������� ���� ���� q�������� Ȯ���ϰ� �ƴϸ� �̵��ϰ� ���� �޿� ������ �帣�� ������. 

            Queue<Vector3> ways = new Queue<Vector3>();
            Vector3 now = start;

            Debug.Log($"��ŸƮ�� ��ǥ�� ? : {start}");
            Debug.Log($"������ ��ǥ�� ? : {end}");

            //1. �ϴ� q���� ���߱� 
            if (start.x != end.x)
            {
                if(start.x < end.x) // �������� ���� ���ʿ� �ִ�.
                {
                    now = now.PlusCoodinate(HexArchive.HexDirections[5]);
                    Debug.Assert(wayOfempties.Contains(now),"�߰��Ϸ��� ��ΰ� ������� �ʽ��ϴ�. ");
                    Debug.Log("����������� ��ǥ :"+ now);
                    ways.Enqueue(now);
                }
                else // �������� ���� �����ʿ� �ִ�.
                {

                    now = now.PlusCoodinate(HexArchive.HexDirections[3]);
                    Debug.Assert(wayOfempties.Contains(now), "�߰��Ϸ��� ��ΰ� ������� �ʽ��ϴ�. ");
                    Debug.Log("����������� ��ǥ :" + now);
                    ways.Enqueue(now);
                }

            }


            while (ways.Peek() != end) // ������ �����ϰ� �Ǹ� ����߰��� �׸��д�. 
            {
                ways.Enqueue(now.PlusCoodinate(HexArchive.HexDirections[4]));
                now = now.PlusCoodinate(HexArchive.HexDirections[4]);
            }

            return ways;

        }




        public void MoveBlockAlongTheBlankWay(Vector3 start, Vector3 depart, List<Vector3> empties) // start������ �������� �̾Ƽ� ����.
        {
            //�ڷᱸ���� �����ߴ� �� �̿�����!!!! 

            //start ������ �ĺ��� �����ϴ� �迭�� Ȱ���ؼ� ��� ���ο��� �켱 �������� �����������,
            //���� ������� ������ ���ٸ� ���� �ٱ������� �̵��ϸ鼭 �� �� �ִ� ���� �ִ� start ������ ã���ִ� �Լ��� ������.
            //  �� �Լ��� ���� ����  ���Ÿ�� �߰��������� �ٷ� �� �� �̿��ϴ� ���� �������� ���� �������� �˻��غ��� �ٷ� �� �Լ��� ������ ���ִ� �͵� ���� �� ����.


            // ����� ��� �ȿ����� 
            Vector3[] found = new Vector3[empties.Count];
            Vector3[] parent = new Vector3[empties.Count];
            

            // �ڡڡ� �ϴ� depart���� start�� ���� ���� ã�Ƽ� ��θ� ���� ������!! :
            // >> stat�� empties�� ���Խ����� ���¿��� �ұ�?? �ƴϾ �� �� ����!!!
            //�ϴ� ��θ� ã��!


            Queue<Vector3> q = new Queue<Vector3>();
            q.Enqueue(start); // ���Լ����̹Ƿ� ���� ���� ��ŸƮ ������ ť�� �־��ش�. 

            while (q.Count > 0)
            {
                Vector3 now = q.Dequeue();

                Vector3[] neighbours = now.myNeighbors();

                for (int i = 0; i < neighbours.Length; i++)
                {
                    if (!hexBlockItems.ContainsKey(neighbours[i]) || !empties.Contains(neighbours[i]))
                    { //��ǥ�� ���忡 ���� ��ǥ�̰ų�, ����� ����ǥ�� �ƴϸ� ��ŵ�Ѵ�. 
                        continue;
                    }

                    //�귯�������� �����̾�� �ϴµ� depart����>stat�� ���Ƿ� �ϴ� �ö󰡴� ��ǥ�� ã�ƾ��Ѵ�
                    // >>> �׷��� ��ǥ(q, r, s)���� r�� �۾����� �ϰ�, s�� Ŀ���� �Ѵ�. 
                    if (now.y > neighbours[i].y || now.z < neighbours[i].z)
                    {
                        continue;
                    }

                    q.Enqueue(neighbours[i]);
                    //ound.

            }
            }
            


            //��ΰ� �ִ����� Ȯ���ؾ��� (�߰��� �������� ���� ���ݾ�!!)
            // >> �̰� �츮�� ã�� ���Vector ����Ʈ�� ������ ���幰�� 
            

            //Neighbor ��ǥ�� ��ȯ���ִ� �Լ��� Ȱ���� ���̴�. 



        }



        #endregion �� �̵� ���

        #region �� ���� ���/����


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

                    //���� ���� 
                    hexBlockItems[matched.CellCordinate] = null;
                    m_hexBlockResults[matched.CellCordinate] = BlockCandyType.NA;
                }

            }



        }

        #endregion �� ���� ���/����



        #region �� ��ǥ �̿�/����

        public Vector2 GetBlockposition(Vector3 coordinate) //��缿 ��ġ�� ������ġ�� ��¦ �ٸ� ���� �־ �����ؼ� �����!
        {
            Vector2 myPosition = m_HexGrid.GetCellPosition(coordinate);

            //������ �����Ѵ�.
            //myPosition.y += offstBlockPositionY;

            return myPosition;

        }


        /// <summary>
        /// �� ������ �ٲ� ������ ������Ʈ �ϴ� �Լ� >>�� ���� ���������� �����ϵ��� ����.
        /// </summary>
        public void UpdtateChangedInfo(HexBlock own, Vector3 chandedCor)
        {
            //�� ��Ʈ�ѷ��� ���� ����Ʈ ����
            own.ChangeCoordinate(chandedCor);
            hexBlockItems[chandedCor] = own;
            m_hexBlockResults[chandedCor] = own.CandyType;
            m_blockBuilder.UpdateListRef_cordinate();


        }

        public List<Vector3> GetMathcedBlockInfo()
        {
            return m_blockBuilder.GetAllMatchVector3coordinatesList();
        }

        #endregion  �� ��ǥ �̿�/����






        //�� ���־�/����
        public Sprite GetNormalBlockSprite(BlockCandyType candy)
        {
            Sprite CandySprite = normalBlockConfig.NormalBlocks[(int)candy];

            return CandySprite;

        }


  
    }


}


