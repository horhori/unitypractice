using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExpansions
{
    // 게임 오브젝트를 지정한 방향으로 회전
    // normalizedDirection : 정규화된 벡터
    public static void RotateTo(this GameObject gameObject, Vector3 normalizedDirection)
    {
        gameObject.transform.rotation = Quaternion.LookRotation(normalizedDirection);
    }

    // 해당 오브젝트에서 지정한 목표를 바라볼 때의 방향 벡터
    public static Vector3 GetDirectionVector(this GameObject gameObject, GameObject targetObject)
    {
        return (targetObject.transform.position - gameObject.transform.position).normalized;
    }
}