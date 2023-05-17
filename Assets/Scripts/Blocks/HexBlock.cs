using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hyo.Scriptable;

namespace Hyo.HexItems
{
    public class HexBlock : MonoBehaviour
    {

        #region 타입관련(BlockDefine)

        BlockType m_BlockType;
        public BlockType BlockType { get { return m_BlockType; } }


        BlockCandyType m_CandyType;   //렌더링되는 블럭 캐린터(즉, 이미지 종류)
        public BlockCandyType CandyType { get { return m_CandyType; } }

        #endregion 타입관련(BlockDefine)




        Vector3 m_CellCordinate; //위치하고 있는 셀의 좌표값 저장
        public Vector3 CellCordinate { get { return m_CellCordinate; } }


        HexBlockController m_Controller;
        GameObject hexBlockPrefab;
        Transform m_Transfom; // 생성자에서 할당받은 프리팹의 Transform을 참조할 것이다.
        SpriteRenderer m_SpriteRenderer;



        /// <summary>
        /// 블럭생성자 : 블럭 셔플 기능 사용을 위해 BlockCandyType 생성할 때 지정토록 한다.
        /// </summary>
        /// <param name="candyType"> 블럭 셔플 기능 때문에 생성할 때 바로 지정하도록 만듦</param>
        /// <param name="prefab"></param>
        /// <param name="type">기본블럭타입은 따로 지정 안하면 BlockType.BASIC </param>
        public void Init(HexBlockController myCon, Vector3 initialCoordinate, BlockCandyType candyType, GameObject prefab, BlockType type = BlockType.BASIC)
        {

            Debug.Log("HexBlock 생성자");
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

        #region 블럭이동 메소드

        public IEnumerator MoveTo(Vector2 purpose, float duration)
        {
            
             //현재 블럭이 가지고 있는 Tr
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
            //좌표 수정하고
            m_CellCordinate = changeValue;

            //위치 맞는지 확인하고 아니면 오류 메시지 보내기
            
            Vector2 changedPosition = m_Controller.GetBlockposition(changeValue);
            Vector2 myPosition = m_Transfom.position;
            if (changedPosition != myPosition)
            {
                Debug.Log("좌표값에 따른 위치가 알맞지 않습니다!");
            }

        }



        #endregion 블럭이동 메소드

        #region 블럭 상태 메소드 

        /// <summary>
        /// Block타입과 블럭캔디타입을 반영하여 Block GameObject에 반영한다
        /// ex) Block 종류에 따른 Sprite 종류 업데이트
        /// 생성자 또는 플레이도중에 Block Type이 변경될 때 호출된다.
        /// </summary>
        public void UpdateBlockView() //타입을 변경하는 함수 안에서도 호출해주면 되곘다!
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

            //터트리는 효과 넣고 싶다! ㅠㅠ 

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
        /// 매칭으로 제거 가능한 블럭인지 검사한다.
        /// </summary>
        /// <returns></returns>
        public bool IsMatchableBlock()
        {
            return !(m_BlockType == BlockType.EMPTY);
        }



        /// <summary>
        /// target Block과 같은 breed를 가지고 있는지 검사한다.
        /// </summary>
        /// <param name="target">비교할 대상 Block</param>
        /// <returns>breed가 같으면 true, 다르면 false</returns>
        public bool IsEqual(HexBlock target)
        {
            if (IsMatchableBlock() && this.CandyType == target.CandyType)
                return true;

            return false;
        }



        #endregion 블럭 상태 메소드 
    }

}

