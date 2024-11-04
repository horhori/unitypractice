using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NPCInstance : MonoBehaviour, INPC
{
    public PlayerInstance _PlayerInstance = null;

    public FollowCamera _FollowCamera = null;

    public VillageUIManager _VillageUIManager = null;

    [SerializeField] private GameObject _QuestBalloon { get; set; }

    [SerializeField] private GameObject _TempPlayer = null;

    private const float ConversationDistance = 5.0f;

    // NPC 이름
    public Text NPCNameText;

    // 대화창 스크립트 저장
    public Text ConversationText;

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
            // 2-1. 클릭 시 NPC 이름 결정
            NPCNameText.text = ReturnNPCName();
            // 2-2. 클릭 시 NPC에 따른 캐릭터 스크립트 결정
            ConversationText.text = ReturnConversationScript();
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

    private string ReturnNPCName()
    {
        string name = "";

        if (gameObject.CompareTag("NPCbear"))
        {
            name = "곰돌이";
        }
        else if (gameObject.CompareTag("NPCotter"))
        {
            name = "수달";
        }
        else if (gameObject.CompareTag("NPCdog"))
        {
            name = "강아지";
        }
        else
        {
            name = "NPC";
        }

        return name;
    }

    // 첫 대사만 우선 작업했음
    // TODO : 1. 대사 스킵기능
    //        2. 대사 클릭하면 다음 대사로 넘어가게
    private string ReturnConversationScript()
    {
        string script = "";

        if (gameObject.CompareTag("NPCbear"))
        {
            script = "부탁이네! 마을을 도와주게!";
        } else if (gameObject.CompareTag("NPCotter"))
        {
            script = "곰 아저씨한테는 항상 도움만 받고 있어";
        } else if (gameObject.CompareTag("NPCdog"))
        {
            script = "나랑 놀아달라!";
        } else
        {
            script = "대화창 개발중입니다.";
        }

        return script;
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

// NPC 스크립트
//b10001	line	000	bear	부탁이네! 마을을 도와주게!
//b10001	line	001	bear	지도를 눌러 퍼즐을 클리어해 주면 되네
//b10002	line	000	bear	지난번에는 도와줘서 고마웠네!
//b10002	line	001	bear	미안하지만 다시 도와줄 수 있겠나?
//b10002	line	002	bear	저번처럼 퍼즐을 클리어 해주게나
//b10003	line	000	bear	계속 민폐를 끼치는 구먼
//b10003	line	001	bear	이번에는 다른 부탁을 해도 되겠나?
//b10003	line	002	bear	농담이네! 똑같이 퍼즐을 클리어 해주게
//b10004	line	000	bear	매번 고맙네! 이번이 마지막 부탁일세!
//b10004	line	001	bear	퍼즐 클리어를 부탁하네
//b10005	line	000	bear	수고가 많구먼!
//b10005	line	001	bear	정말 마지막으로 한번만 더 도와주게나
//b10005	line	002	bear	마지막 퍼즐일세
//b10006	line	000	bear	저기 수달 친구가 쑥스러움이 좀 많네
//b10006	line	001	bear	헌데 도움이 필요한 모양이야.
//b10006	line	002	bear	가서 말이라도 좀 걸어주겠나?
//b10007	line	000	bear	혹시 강아지 친구를 본 적 있는가?
//b10007	line	001	bear	항상 바쁘게 마을을 돌아다니는 친구일세
//b10007	line	002	bear	혹시 발견하게 된다면 그 친구 일 좀 도와주게나
//b20001	line	000	otter	곰 아저씨한테는 항상 도움만 받고 있어
//b20001	line	001	otter	그래서 은혜를 갚으려고 하는데 도와줄래?
//b20001	line	002	otter	빨강 보석을 모아줘
//b20002	line	000	otter	지난번엔 도와줘서 고마워!
//b20002	line	001	otter	곰아저씨가 기뻐하는거 같아서 나도 기분이 좋아!
//b20002	line	002	otter	혹시 이번에도 도와줄 수 있을까?
//b20002	line	003	otter	주황, 노랑 보석을 모아줘
//b20003	line	000	otter	계속 실례하게 되네
//b20003	line	001	otter	주변에 도움을 주니까 나도 점점 욕심이 생겨서 말이야
//b20003	line	002	otter	초록, 파랑, 보라 보석을 모아줘
//b20011	line	000	otter	우리 마을에 강아지 친구가 있는거 봤니?
//b20011	line	001	otter	실은 그 친구한테 선물을 줄까해서
//b20011	line	002	otter	주황 보석을 좀 모아줄 수 있을까?
//b20012	line	000	otter	강아지 친구가 기뻐하는 모습이 너무 귀엽다
//b20012	line	001	otter	소심한 나한테도 항상 말을 걸어주는 고마운 친구야
//b20012	line	002	otter	이번에는 주황, 노랑, 분홍 보석을 모아줄래?
//b20013	line	000	otter	계속 도와줘서 고마워
//b20013	line	001	otter	이제 강아지 친구한테 더 큰 선물을 할 때야
//b20013	line	002	otter	나는 받은 은혜는 잊지 않거든 후후
//b20013	line	003	otter	빨강, 주황, 노랑, 초록 보석을 모아줘
//b20021	line	000	otter	안녕. 마을 사람들이 모두 보기 예뻐졌어.
//b20021	line	001	otter	나만 빼고
//b20021	line	002	otter	그래서 말인데 내가 입을 옷도 만들어볼까해
//b20021	line	003	otter	파랑, 보라 보석을 모아줄래?
//b20022	line	000	otter	헤헤 고마워
//b20022	line	001	otter	나한테 이런 재능이 있을줄은 몰랐는데
//b20022	line	002	otter	네 덕분에 많은걸 할 수 있엇던 것 같아
//b20022	line	003	otter	이번에는 빨강, 노랑, 초록 보석을 모아줄래?
//b20023	line	000	otter	지금 이 상황이 꿈만 같아
//b20023	line	001	otter	정말 고마워
//b20023	line	002	otter	이제 마지막 부탁이야
//b20023	line	003	otter	빨강, 초록, 파랑, 보라, 분홍 보석을 부탁할께
//b20031	line	000	dog	나랑 놀아달라!
//b20031	line	001	dog	숨바꼭질이다! 숨바꼭질!
//b20031	line	002	dog	한번 찾아보시지
//b20032	line	000	dog	으잇! 나를 찾다니!
//b20032	line	001	dog	이번에는 못 찾을거다!
//b20033	line	000	dog	헥...또 들킬 줄이야
//b20033	line	001	dog	재밌다!
//b20033	line	002	dog	계속하자! 숨바꼭질!
//b20034	line	000	dog	헥헥...재밌다!
//b20034	line	001	dog	나랑 재밌게 놀아주는 사람은 곰 아저씨 말고 처음이다!
//b20034	line	002	dog	너 좋은 사람이다!
//b20034	line	003	dog	그럼 또 놀자! 또!
//b20035	line	000	dog	헥헥헥... 너 체력 좋다!
//b20035	line	001	dog	마지막으로 날 찾아봐!
//b20035	line	002	dog	계속 놀아줘서 고마워!