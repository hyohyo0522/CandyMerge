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

        //������
        void CreateHexBlock(Vector3 coordinate, BlockCandyType Type)
        {
            if(Type == BlockCandyType.SPECIAL)
            {
                //���߿� ����� ��� ���� �� ���⼭ � ���� �߰� �ϸ� ���� �� ����.
                return;
            }
            GameObject go = Instantiate(hexBlockPrefab, transform.position, Quaternion.identity);
            go.transform.parent = _prefabContainerInHierarchy.transform;


            HexBlock block = go.AddComponent<HexBlock>(); //�������̺���̱� ������ �̷������� �����հ� ���� 
            block.Init(this, coordinate, Type, go);
            hexBlockItems.Add(coordinate,block);

        }



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
            HexBlock target = hexBlockItems[targetCoordinate];



            //2. �������� ������ ������ üũ�Ѵ�. 

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

            //3. �������� �� �̵��� ��ġ�� �����Ѵ�.
            HexBlock start = hexBlockItems[blockCordinate];

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

            DoDestroyBlocks(); //�����ΰ�??


            // �� ������ �帣�� �� ����� : �ϴ� ������ ��� ������ �غ���! 
            orderFlowBlocks(m_blockBuilder.allEmptyBlock());

            yield break;

        }


        //��������� ������ ���� ���·� �����մϴ�! ��-�� 
        public void orderFlowBlocks(List<Vector3> empties) // �� �� ���õ� ��� + �¿�� �帣�� ��� ���!!!
        {
            foreach (Vector3 blockPos in empties)
            {
                Vector3 upCor = blockPos.PlusCoodinate(HexArchive.HexDirections[1]);

                if (hexBlockItems.ContainsKey(upCor))
                {
                    //������ ���� 
                    Vector2 targetPos = GetBlockposition(blockPos);
                    Vector3 upOriginCor = upCor;

                    //�� ���ϱ�
                    HexBlock UpBlock = hexBlockItems[upCor];

                    if (UpBlock != null)
                    {
                        StartCoroutine(UpBlock.MoveTo(targetPos, Constants.SwipeDuration));
                        UpBlock.ChangeCoordinate(blockPos);
                        UpdtateChangedInfo(UpBlock, UpBlock.CellCordinate);

                        //���� �����̾��� ���� �� ����
                        m_hexBlockResults[upOriginCor] = BlockCandyType.NA;
                        hexBlockItems[upOriginCor] = null; //��ġ ������ �߻��� ���� >> �ذ��ؾ���!!
                    }

                }
                else 
                {
                    // �� ���� �ȵ��ñ�?
                    Debug.Log("�� ��������?");
                    Vector2 targetPos = GetBlockposition(blockPos);
                    Vector3 upOriginCor = upCor;

                    // ���⼭ �� Ÿ�� �����ִ� ���� ������ ¥�� ���� �� ������ .��-��
                    int random = Random.Range(0, System.Enum.GetValues(typeof(BlockCandyType)).Length - 2);

                    CreateHexBlock(upCor, (BlockCandyType)random);
                    HexBlock UpBlock = hexBlockItems[upCor];

                    StartCoroutine(UpBlock.MoveTo(targetPos, Constants.SwipeDuration));
                    UpBlock.ChangeCoordinate(blockPos);
                    UpdtateChangedInfo(UpBlock, UpBlock.CellCordinate);

                    //���� �����̾��� ���� �� ����
                    m_hexBlockResults[upOriginCor] = BlockCandyType.NA;
                    hexBlockItems[upOriginCor] = null; //��ġ ������ �߻��� ���� >> �ذ��ؾ���!!


                }
                DoDestroyBlocks();

            }

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


