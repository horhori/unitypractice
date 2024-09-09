using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInstance : MonoBehaviour, INPC
{
    public PlayerInstance player { get; private set; }

    public CharacterManager characterManager { get; private set; }

    public FollowCamera _FollowCamera = null;

    [SerializeField] private GameObject _QuestBalloon { get; set; }

    [SerializeField] private GameObject _TempPlayer = null;

    private void Awake()
    {
        player = GetComponent<PlayerInstance>();
        _FollowCamera = GameObject.Find("FollowCamera").GetComponent<FollowCamera>();
        _QuestBalloon = GetComponentInChildren<SpriteRenderer>().gameObject;
    }

    private void Update()
    {

    }

    private void OnMouseDown()
    {
        // TODO : NPC 퀘스트 완성

        // 0. NPC와 거리 체크 후 충분히 가까우면 실행
        // 1. 컨트롤러 UI 창 비활성화
        // 2. 대화 UI 창 활성화 -> 퀘스트 상태에 따라 다른 대화 나오게??
        // 3. NPC 앞 캐릭터 활성화 & 퀘스트말풍선 비활성화
        _TempPlayer.SetActive(true);
        _QuestBalloon.SetActive(false);
        // 4. 플레이어 캐릭터 비활성화
        player.gameObject.SetActive(false);
        // 5. 카메라 NPC한테 위치 맞추기
        // 6. 나가기 클릭 시 원위치
        _FollowCamera.SwitchTransformCamera(_TempPlayer.transform);

        //_FollowCamera.SwitchTargetCamera(2);
    }
}
