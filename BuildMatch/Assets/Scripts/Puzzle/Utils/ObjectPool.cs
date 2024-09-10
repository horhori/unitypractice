using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// null이 전달되었을 때 발생시킬 예외 클래스 정의
public class ParamIsNullException : System.Exception
{
    private string _ParamName;

    public ParamIsNullException(string paramName)
    {
        _ParamName = paramName;
    }

    public override string Message
    {
        get { return "전달된 " + _ParamName + "의 값이 null 입니다."; }
    } 
}

// > 오브젝트 풀링을 사용할 수 있는 오브젝트에 들어가는 컴포넌트에
// 필수적인 인터페이스

public interface IRecyclableGameObject
{
    bool isActive { get; } 
}

public class ObjectPool<T> where T : IRecyclableGameObject
{
    // 재활용 가능한 오브젝트를 저장할 리스트
    private List<T> _ObjectPool = null;

    public ObjectPool()
    {
        _ObjectPool = new List<T>();
    }

    // > 재활용 가능한 오브젝트를 찾아줌
    public T GetRecyclableObject()
    {
        // 재활용 가능한 오브젝트 중 [isActive] 속성이 false인 오브젝트를 찾음
        return _ObjectPool.Find(
            (T recyclableObj) => !recyclableObj.isActive );
    }

    // 재활용 가능한 오브젝트를 등록
    // 리턴값 : 등록시킨 오브젝트

    public T RegisterRecyclableObject(T recyclableObj)
    {
        // 예외
        if (recyclableObj == null) throw new ParamIsNullException("recyclableObj");

        _ObjectPool.Add(recyclableObj);
        return recyclableObj;
    }
}
