namespace Hyo.Util
{

    /// <summary>
    /// �ַ� �ڷ�ƾ�� �Ķ���ͷ� ���޵Ǿ �ڷ�ƾ ���� ����� �ް��� �ϴ� ��쿡 ���.
    /// ex) yield return StartCoroutine(MyCoroutine( r ))
    /// </summary>
    public class Returnable<T>
    {
        public T value { get; set; }

        public Returnable(T value)
        {
            this.value = value;
        }
    }

}

