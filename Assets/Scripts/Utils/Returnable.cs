namespace Hyo.Util
{

    /// <summary>
    /// 주로 코루틴의 파라미터로 전달되어서 코루틴 실행 결과를 받고자 하는 경우에 사용.
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

