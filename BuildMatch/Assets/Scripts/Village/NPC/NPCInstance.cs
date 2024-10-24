using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCInstance : MonoBehaviour, INPC
{
    public PlayerInstance _PlayerInstance = null;

    public FollowCamera _FollowCamera = null;

    public VillageUIManager _VillageUIManager = null;

    [SerializeField] private GameObject _QuestBalloon { get; set; }

    [SerializeField] private GameObject _TempPlayer = null;

    private const float ConversationDistance = 5.0f;

    private void Awake()
    {
        _PlayerInstance = GameManager.GetManagerClass<CharacterManager>().player;
        _FollowCamera = GameObject.Find("FollowCamera").GetComponent<FollowCamera>();
        _QuestBalloon = GetComponentInChildren<SpriteRenderer>().gameObject;
        _VillageUIManager = GameObject.Find("VillageUIManager").GetComponent<VillageUIManager>();
    }

    private void Start()
    {
        // build 시 Player 못 찾아와서 추가함
        if (_PlayerInstance == null)
        {
            _PlayerInstance = GameManager.GetManagerClass<CharacterManager>().player;
        }
    }

    private void Update()
    {
        CheckQuestBalloonOpacity();
    }

    private void OnMouseDown()
    {
        // TODO : NPC 퀘스트 완성(거리 체크, 대화 UI)

        // 0. NPC와 거리 체크(5 이상 : 불투명도 50%, 5 이하 : 불투명도 100%) 후 충분히 가까우면 실행
        if (CheckConversation())
        {
            // 1. 컨트롤러 UI 창 비활성화
            _VillageUIManager._ControllerUI.SetActive(false);
            // 2. 대화 UI 창 활성화 -> 퀘스트 상태에 따라 다른 대화 나오게??
            _VillageUIManager._QuestUI.SetActive(true);
            // 3. NPC 앞 캐릭터 활성화 & 퀘스트말풍선 비활성화
            _TempPlayer.SetActive(true);
            _QuestBalloon.SetActive(false);
            // 4. 플레이어 캐릭터 비활성화
            _PlayerInstance.gameObject.SetActive(false);
            // 5. 카메라 NPC한테 위치 맞추기(카메라 확대 필요)
            _FollowCamera.SwitchTransformNPCCamera(_TempPlayer.transform);
            // 6. 나가기 클릭 시 원위치
            StartCoroutine(ReturnCamera());
        }
    }

    // Update로 거리 5 이상일 때는 퀘스트마크 불투명도 50%, 5 이하(대화가능) 일때는 불투명도 100%
    private void CheckQuestBalloonOpacity()
    {
        float distance = Vector3.Distance(_PlayerInstance.gameObject.transform.position, gameObject.transform.position);
        if (distance <= ConversationDistance)
        {
            Color color = _QuestBalloon.GetComponentInChildren<SpriteRenderer>().color;
            color.a = 1f;
            _QuestBalloon.GetComponentInChildren<SpriteRenderer>().color = color;
        } else
        {
            Color color = _QuestBalloon.GetComponentInChildren<SpriteRenderer>().color;
            color.a = 0.5f;
            _QuestBalloon.GetComponentInChildren<SpriteRenderer>().color = color;
        }

    }

    // NPC와 플레이어 사이 거리가 5 이하일 때 대화 가능
    private bool CheckConversation()
    {
        float distance = Vector3.Distance(_PlayerInstance.gameObject.transform.position, gameObject.transform.position);

        return distance <= ConversationDistance ? true : false;
    }

    private IEnumerator ReturnCamera()
    {
        yield return new WaitUntil(() => _PlayerInstance.isQuestEnd);

        _FollowCamera.SwitchTransformPlayerCamera(_PlayerInstance.transform);

        _PlayerInstance.gameObject.SetActive(true);
        _QuestBalloon.SetActive(true);

        _TempPlayer.SetActive(false);

        _VillageUIManager._QuestUI.SetActive(false);

        _VillageUIManager._ControllerUI.SetActive(true);

        _PlayerInstance.isQuestEnd = false;
    }
}
