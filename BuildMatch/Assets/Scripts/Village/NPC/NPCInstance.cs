using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInstance : MonoBehaviour, INPC
{
    public PlayerInstance _PlayerInstance = null;

    public FollowCamera _FollowCamera = null;

    [SerializeField] private GameObject _QuestBalloon { get; set; }

    [SerializeField] private GameObject _TempPlayer = null;

    public GameObject _ControllerUI = null;
    public GameObject _QuestUI = null;

    private void Awake()
    {
        _PlayerInstance = VillageGameManager.GetManagerClass<CharacterManager>().player;
        _FollowCamera = GameObject.Find("FollowCamera").GetComponent<FollowCamera>();
        _QuestBalloon = GetComponentInChildren<SpriteRenderer>().gameObject;
        _ControllerUI = GameObject.Find("Canvas");
    }

    private void Update()
    {

    }

    private void OnMouseDown()
    {
        // TODO : NPC 퀘스트 완성

        // 0. NPC와 거리 체크 후 충분히 가까우면 실행
        // 1. 컨트롤러 UI 창 비활성화
        _ControllerUI.SetActive(false);
        // 2. 대화 UI 창 활성화 -> 퀘스트 상태에 따라 다른 대화 나오게??
        _QuestUI.SetActive(true);
        // 3. NPC 앞 캐릭터 활성화 & 퀘스트말풍선 비활성화
        _TempPlayer.SetActive(true);
        _QuestBalloon.SetActive(false);
        // 4. 플레이어 캐릭터 비활성화
        _PlayerInstance.gameObject.SetActive(false);
        // 5. 카메라 NPC한테 위치 맞추기(카메라 확대 필요)
        _FollowCamera.SwitchTransformNPCCamera(_TempPlayer.transform);
        // 6. 나가기 클릭 시 원위치
        StartCoroutine(ReturnCamera());

        //_FollowCamera.SwitchTargetCamera(2);
    }

    private IEnumerator ReturnCamera()
    {
        yield return new WaitUntil(() => _PlayerInstance.isQuestEnd);

        _FollowCamera.SwitchTransformPlayerCamera(_PlayerInstance.transform);

        _PlayerInstance.gameObject.SetActive(true);
        _QuestBalloon.SetActive(true);

        _TempPlayer.SetActive(false);

        _QuestUI.SetActive(false);

        _ControllerUI.SetActive(true);

        _PlayerInstance.isQuestEnd = false;
    }
}
