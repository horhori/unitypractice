using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimInstance
{
    // 외부 애니메이터 접근 허용
    Animator animator { get; }

    // 애니메이터의 파라미터 변경 메서드 구현
    bool SetBool(string paramName, bool value);
    float SetFloat(string paramName, float value);
    int SetInt(string paramName, int value);
    void SetTrigger(string paramName);
}
