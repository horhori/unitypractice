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

    private void Awake()
    {
        _CharacterManager = VillageGameManager.GetManagerClass<CharacterManager>();
        _CharacterManager.player = this;

        _SoundManager = VillageGameManager.GetManagerClass<SoundManager>();

        playerMovement = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        // 마우스로 클릭해서 인식 후 대화
        // NPCInstance OnMouseDown으로 NPC 클릭 이벤트 처리
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;

        //if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit, 10.0f))
        //{
        //    Debug.Log(hit.transform.gameObject);
        //}
    }
}
