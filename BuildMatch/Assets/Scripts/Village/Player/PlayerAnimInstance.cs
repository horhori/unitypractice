using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimInstance : MonoBehaviour, IAnimInstance
{
    public Animator animator {  get; private set; }

    private float _Speed = 0.0f;

    // 캐릭터 매니저 변수 참조
    private CharacterManager _CharacterManager = null;

    private void Awake()
    {
        _CharacterManager = VillageGameManager.GetManagerClass<CharacterManager>();
        animator = transform.GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        _Speed = _CharacterManager.inputVector.magnitude;
        SetFloat("_Speed", _Speed);
    }

    #region Implemented IAnimInstace

    public bool SetBool(string paramName, bool value)
    {
        animator.SetBool(paramName, value);
        return value;
    }

    public float SetFloat(string paramName, float value)
    {
        animator.SetFloat(paramName, value);

        return value;
    }

    public int SetInt(string paramName, int value)
    {
        animator.SetInteger(paramName, value);
        return value;
    }

    public void SetTrigger(string paramName)
    {
        animator.SetTrigger(paramName);

    }
    #endregion
}
