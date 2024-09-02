using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 없다면 자동으로 넣어줌
[RequireComponent(typeof(CharacterController))]
public class PlayerInstance : MonoBehaviour
{
    // CharacterManager 참조 변수
    private CharacterManager _CharacterManager = null;

    // PlayerMovement에 대한 프로퍼티
    public PlayerMovement playerMovement { get; private set; }

    // CharacterController 프로퍼티
    public CharacterController controller { get; private set; }

    private void Awake()
    {
        _CharacterManager = VillageGameManager.GetManagerClass<CharacterManager>();
        _CharacterManager.player = this;

        playerMovement = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
    }
}
