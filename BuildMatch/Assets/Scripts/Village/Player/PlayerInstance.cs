using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 없다면 자동으로 넣어줌
[RequireComponent(typeof(CharacterController))]
public class PlayerInstance : MonoBehaviour
{
    // CharacterManager 참조 변수
    private CharacterManager _CharacterManager = null;

    // SoundManager 참조 변수
    private SoundManager _SoundManager = null;

    // PlayerMovement에 대한 프로퍼티
    public PlayerMovement playerMovement { get; private set; }

    // CharacterController 프로퍼티
    public CharacterController controller { get; private set; }

    // 퀘스트 대화 진행 여부
    public bool isQuestEnd { get; set; } = false;

    private void Awake()
    {
        _CharacterManager = GameManager.GetManagerClass<CharacterManager>();
        _CharacterManager.player = this;

        _SoundManager = GameManager.GetManagerClass<SoundManager>();

        playerMovement = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {

    }
}
