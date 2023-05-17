using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hyo.core;

namespace Hyo.Util
{
    //TODO �������� enum ���⼭ �����ؾ���!!! 
    //TODO �������� �ޱ� ��� �ٽ� �־���� �ؿ� EvalDragAngle()�Լ� �ٽ� �����ϱ�


    #region �簢�� ����� Swipe ��� 
    public enum Swipe
    {
        NA = -1,
        RIGHT = 0,
        UP = 1,
        LEFT = 2,
        DOWN = 3
    }


    public static class SwipeDirMethod
    {
        public static int GetTargetRow(this Swipe swipeDir)
        {
            switch (swipeDir)
            {
                case Swipe.DOWN: return -1; ;
                case Swipe.UP: return 1;
                default:
                    return 0;
            }
        }

        public static int GetTargetCol(this Swipe swipeDir)
        {
            switch (swipeDir)
            {
                case Swipe.LEFT: return -1; ;
                case Swipe.RIGHT: return 1;
                default:
                    return 0;
            }
        }
    }
    #endregion �簢�� ����� Swipe ��� 


    //Hex ���������� q,r,s 3���� ���� �̿��� ������ ���� / ���� �ڷ�  : https://www.redblobgames.com/grids/hexagons/#Cube-coordinate
    public enum HexSwipe
    {
        //�ð�������� ���� , �ݴ���� ���ϴ� ����
        // HexArchive�� ���� �����ʹ� �ٸ��� ����
        NA = -1,
        RIGHT_UP = 0,
        UP = 1,
        LEFT_UP = 2,
        LEFT_DOWN = 3,
        DOWN = 4,
        RIGHT_DOWN = 5

    }

    public static class SwipeHexDirMethod
    {   

        public static Vector3 GetTargetSwipeCor(this HexSwipe swipeDir)
        {
            switch (swipeDir)
            {
                case HexSwipe.RIGHT_UP: return HexArchive.HexDirections[0];
                case HexSwipe.UP: return HexArchive.HexDirections[1];
                case HexSwipe.LEFT_UP: return HexArchive.HexDirections[2];
                case HexSwipe.LEFT_DOWN: return HexArchive.HexDirections[3];
                case HexSwipe.DOWN: return HexArchive.HexDirections[4];
                case HexSwipe.RIGHT_DOWN: return HexArchive.HexDirections[5];
                default:
                    return Vector3.zero;
            }
        }

        public static Vector3 PlusCoodinate(this Vector3 mine, Vector3 target)
        {
            float myX, myY, myZ, targetX, targetY, targetZ;

            myX = mine.x;
            myY = mine.y;
            myZ = mine.z;

            targetX = target.x;
            targetY = target.y;
            targetZ = target.z;

            Vector3 newVector = new Vector3(myX + targetX, myY + targetY, myZ + targetZ);

            return newVector;

        }

        /// <summary>
        /// <Vector3> ����Ʈ ��ĥ������ �������! 3���̻� �ߺ� �� ���� ������� ���մϴ�. �߰��� ������� ����Ʈ�� ��ȯ!
        /// </summary>
        /// <param name="my">���� �߰��� ����Ʈ</param>
        /// <param name="tartget">�߰��� ����Ʈ</param>
        /// <returns></returns>
        public static List<Vector3> AddRangeNoDuplication(this List<Vector3> my, List<Vector3> tartget)
        {
            foreach( Vector3 item in tartget)
            {
                if (my.Contains(item))
                {
                    continue;
                }
                else //�ߺ��� �������� ������ �߰����ݴϴ�! 
                {
                    my.Add(item);
                }
            }

            return my;
        }
            

    }



    public static class TouchEvaluator
    {
        /*
         * �� ������ ����Ͽ� Swipe ������ ���Ѵ�.
         * UP : 45~ 135, LEFT : 135 ~ 225, DOWN : 225 ~ 315, RIGHT : 0 ~ 45, 0 ~ 315
         */
        public static Swipe EvalSwipeDir(Vector2 vtStart, Vector2 vtEnd)
        {
            float angle = EvalDragAngle(vtStart, vtEnd);
            if (angle < 0)
                return Swipe.NA;

            int swipe = (((int)angle + 45) % 360) / 90;

            switch (swipe)
            {
                case 0: return Swipe.RIGHT;
                case 1: return Swipe.UP;
                case 2: return Swipe.LEFT;
                case 3: return Swipe.DOWN;
            }

            return Swipe.NA;
        }

        //ź��Ʈ�� ���Լ� ��� / �����ڷ� : https://ninezmk2.blogspot.com/2019/11/swipe.html 
        //
        //    UP = 60~120, RIGHT_UP = 0~60, RiGHT_DOWN = 300~0, DOWN = 240~300, LEFT_DOWN = 180~240, LEFT_UP = 120~180

        public static HexSwipe EvalHexSwipeDir(Vector2 vtStart, Vector2 vtEnd)
        {
            float angle = EvalDragAngle(vtStart, vtEnd);
            if (angle < 0)
                return HexSwipe.NA;

            // 110 + 30 = 140 / 60 = 2. ...
            // 
            int swipe = (((int)angle) % 360) / 60;

            Debug.Log($"{vtStart} {vtEnd} �������� ������ = angle : {angle} , int�� : {swipe} ");
            switch (swipe)
            {

                case 0: return HexSwipe.RIGHT_UP;
                case 1: return HexSwipe.UP;
                case 2: return HexSwipe.LEFT_UP ;
                case 3: return HexSwipe.LEFT_DOWN;
                case 4: return HexSwipe.DOWN;
                case 5: return HexSwipe.RIGHT_DOWN;
                   
            }


            return HexSwipe.NA;
        }


        /*
         * �� ����Ʈ ������ ������ ���Ѵ�.
         * Input(���콺, ��ġ) ��ġ �巡�׽ÿ� �巡���� ������ ���ϴµ� Ȱ���Ѵ�.
         */
        static float EvalDragAngle(Vector2 vtStart, Vector2 vtEnd)
        {
            Vector2 dragDirection = vtEnd - vtStart;
            if (dragDirection.magnitude <= 0.2f)
                return -1f;

            //Debug.Log($"eval angle : {vtStart} , {vtEnd}, magnitude = {dragDirection.magnitude}");

            float aimAngle = Mathf.Atan2(dragDirection.y, dragDirection.x);
            if (aimAngle < 0f)
            {
                aimAngle = Mathf.PI * 2 + aimAngle;
            }

            return aimAngle * Mathf.Rad2Deg;
        }
    }
}
