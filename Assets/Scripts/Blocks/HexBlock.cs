using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hyo.Scriptable;

namespace Hyo.HexItems
{
    public class HexBlock : MonoBehaviour
    {

        #region Ÿ�԰���(BlockDefine)

        BlockType m_BlockType;
        public BlockType BlockType { get { return m_BlockType; } }


        BlockCandyType m_CandyType;   //�������Ǵ� �� ĳ����(��, �̹��� ����)
        public BlockCandyType CandyType { get { return m_CandyType; } }

        #endregion Ÿ�԰���(BlockDefine)




        Vector3 m_CellCordinate; //��ġ�ϰ� �ִ� ���� ��ǥ�� ����
        public Vector3 CellCordinate { get { return m_CellCordinate; } }


        HexBlockController m_Controller;
        GameObject hexBlockPrefab;
        Transform m_Transfom; // �����ڿ��� �Ҵ���� �������� Transform�� ������ ���̴�.
        SpriteRenderer m_SpriteRenderer;



        /// <summary>
        /// �������� : �� ���� ��� ����� ���� BlockCandyType ������ �� ������� �Ѵ�.
        /// </summary>
        /// <param name="candyType"> �� ���� ��� ������ ������ �� �ٷ� �����ϵ��� ����</param>
        /// <param name="prefab"></param>
        /// <param name="type">�⺻��Ÿ���� ���� ���� ���ϸ� BlockType.BASIC </param>
        public void Init(HexBlockController myCon, Vector3 initialCoordinate, BlockCandyType candyType, GameObject prefab, BlockType type = BlockType.BASIC)
        {

            Debug.Log("HexBlock ������");
            m_Controller = myCon;
            m_CellCordinate = initialCoordinate;
            m_BlockType = type;
            m_CandyType = candyType;
            hexBlockPrefab = prefab;
            m_Transfom = hexBlockPrefab.GetComponent<Transform>();

            var position = myCon.GetBlockposition(initialCoordinate);
            this.gameObject.name = $"{initialCoordinate} : {position}";

            m_Transfom.localPosition = position;
            m_SpriteRenderer = hexBlockPrefab.GetComponent<SpriteRenderer>();


            UpdateBlockView();
        }

        #region ���̵� �޼ҵ�

        public IEnumerator MoveTo(Vector2 purpose, float duration)
        {
            
             //���� ���� ������ �ִ� Tr
            Vector2 startPos = m_Transfom.transform.localPosition;

            float elapsed = 0.0f;
            while(elapsed < duration)
            {
                elapsed += Time.smoothDeltaTime;
                m_Transfom.transform.localPosition = Vector2.Lerp(startPos, purpose, elapsed / duration);

                yield return null;
            }

            m_Transfom.transform.localPosition = purpose;

            yield break;
        }

        public void ChangeCoordinate(Vector3 changeValue)
        {
            //��ǥ �����ϰ�
            m_CellCordinate = changeValue;

            //��ġ �´��� Ȯ���ϰ� �ƴϸ� ���� �޽��� ������
            
            Vector2 changedPosition = m_Controller.GetBlockposition(changeValue);
            Vector2 myPosition = m_Transfom.position;
            if (changedPosition != myPosition)
            {
                Debug.Log("��ǥ���� ���� ��ġ�� �˸��� �ʽ��ϴ�!");
            }

        }



        #endregion ���̵� �޼ҵ�

        #region �� ���� �޼ҵ� 

        /// <summary>
        /// BlockŸ�԰� ��ĵ��Ÿ���� �ݿ��Ͽ� Block GameObject�� �ݿ��Ѵ�
        /// ex) Block ������ ���� Sprite ���� ������Ʈ
        /// ������ �Ǵ� �÷��̵��߿� Block Type�� ����� �� ȣ��ȴ�.
        /// </summary>
        public void UpdateBlockView() //Ÿ���� �����ϴ� �Լ� �ȿ����� ȣ�����ָ� �ǁٴ�!
        {
            
            if (m_BlockType == BlockType.EMPTY)
            {
                m_SpriteRenderer.sprite = null;
            }
            else if (m_BlockType == BlockType.BASIC)
            {
                m_SpriteRenderer.sprite = m_Controller.GetNormalBlockSprite(m_CandyType);
            }
        }



        public IEnumerator DestroyBlock(float duration)
        {
            float elaped = 0.0f;

            //��Ʈ���� ȿ�� �ְ� �ʹ�! �Ф� 

            Vector3 myScale = m_Transfom.transform.localScale;

            while (elaped < duration)
            {
                elaped += Time.smoothDeltaTime;
                m_Transfom.localScale = Vector3.Lerp(myScale, Vector3.zero, elaped / duration);

                yield return null;
            }


            Destroy(this.gameObject, duration);

            yield break;
        }


        /// <summary>
        /// ��Ī���� ���� ������ ������ �˻��Ѵ�.
        /// </summary>
        /// <returns></returns>
        public bool IsMatchableBlock()
        {
            return !(m_BlockType == BlockType.EMPTY);
        }



        /// <summary>
        /// target Block�� ���� breed�� ������ �ִ��� �˻��Ѵ�.
        /// </summary>
        /// <param name="target">���� ��� Block</param>
        /// <returns>breed�� ������ true, �ٸ��� false</returns>
        public bool IsEqual(HexBlock target)
        {
            if (IsMatchableBlock() && this.CandyType == target.CandyType)
                return true;

            return false;
        }



        #endregion �� ���� �޼ҵ� 
    }

}

