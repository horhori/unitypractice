using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 게임 스코어, 게임 승리/패배 조건, UI 설정

// TODO : 1. 승리 / 패배 종료 변경
//           -> 남은 시간이 다 되었을 때 패배 처리 완료. 바구니가 다 채워지면 클리어되는 것으로는 진행해야 함.
//        2. 레벨 설정 레벨 당 남은 시간 및 목표 개수
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // static reference;

    public GameObject warningUI; // 10초 남았을 때 경고 UI
    private Image warningImage;
    public float warningSec; // 경고 뜨는 남은 기준 시간 (현재 10초)

    public GameObject backgroundPanel; // grey background 승리/패배 화면 클릭할 때 포션 동작 안되게 
    public GameObject clearPanel;
    public GameObject failedPanel;

    public int goal; // the amount of points you need to get to win.
    public int points; // 최대 숫자 9개까지 ex) 999999999

    // 현재 스테이지
    public int stageNumber;

    // 남은 시간
    public int min;
    public float sec;

    // 스왑 이후부터 true로 변경되면서 시간 체크
    // true일때만 시간이 지나감
    [SerializeField]
    private bool isGameRunning = false;

    // 남은 시간이 종료되었을 때 
    public bool isStageEnded;

    public TMP_Text stageText;
    public TMP_Text pointsText;
    public TMP_Text timeText;

    // TODO : 1. 나중에 따로 bag 컴포넌트로 관리 필요
    public Sprite[] bagSprites;

    public GameObject bag1;
    private TMP_Text bag1Text;
    private int bag1CurrentCount;
    private int bag1GoalCount; // 1 stage 15
    public PotionType bag1Type;
    private bool bag1Check; // currentCount == GoalCount 되면 check됨

    public GameObject bag2;
    private TMP_Text bag2Text;
    private int bag2CurrentCount;
    private int bag2GoalCount;
    public PotionType bag2Type;
    private bool bag2Check;

    public GameObject bag3;
    private TMP_Text bag3Text;
    private int bag3CurrentCount;
    private int bag3GoalCount;
    public PotionType bag3Type;
    private bool bag3Check;

    public GameObject bag4;
    private TMP_Text bag4Text;
    private int bag4CurrentCount;
    private int bag4GoalCount;
    public PotionType bag4Type;
    private bool bag4Check;


    // 경고 UI 설정 컬러값
    private Color originWarningColor;
    private Color fullWarningColor;

    // 결과창 Scale 설정값
    private Vector3 firstResultScale;
    private Vector3 middleResultScale;
    private Vector3 lastResultScale;

    float checkTime = 0;

    private void Awake()
    {
        Instance = this;
        warningImage = warningUI.GetComponent<Image>();
        originWarningColor = warningImage.GetComponent<Image>().color;
        fullWarningColor = new Color(1, 1, 1, 1);
            
        firstResultScale = Vector3.zero;
        middleResultScale = new Vector3(1.2f, 1.2f, 1);
        lastResultScale = new Vector3(1, 1, 1);
    }

    private void Start()
    {
        stageText.text = "Stage " + stageNumber;
        SetUpBag();
    }

    private void SetUpBag()
    {
        //Sprite[] sprite_1 = Resources.LoadAll<Sprite>("Sprites/Puzzle Blocks Icon Pack/png/blockBlueDimond");
        //Debug.Log("sprite" + sprite_1[0]);
        bag1Text = bag1.GetComponentInChildren<TMP_Text>();
        bag1Type = PotionType.BlueBlock;
        bag1GoalCount = 15;
        bag1CurrentCount = 0;
        bag1Text.text = bag1CurrentCount.ToString() + " / " + bag1GoalCount.ToString();
        bag1Check = false;

        bag2Text = bag2.GetComponentInChildren<TMP_Text>();
        bag2Type = PotionType.GreenBlock;
        bag2GoalCount = 30;
        bag2CurrentCount = 0;
        bag2Text.text = bag2CurrentCount.ToString() + " / " + bag2GoalCount.ToString();
        bag2Check = false;

        bag3Text = bag3.GetComponentInChildren<TMP_Text>();
        bag3Type = PotionType.PinkBlock;
        bag3GoalCount = 35;
        bag3CurrentCount = 0;
        bag3Text.text = bag3CurrentCount.ToString() + " / " + bag3GoalCount.ToString();
        bag3Check = false;

        bag4Text = bag4.GetComponentInChildren<TMP_Text>();
        bag4Type = PotionType.RedBlock;
        bag4GoalCount = 40;
        bag4CurrentCount = 0;
        bag4Text.text = bag4CurrentCount.ToString() + " / " + bag4GoalCount.ToString();
        bag4Check = false;

    }

    // Update is called once per frame
    void Update()
    {
        UpdatePoint();
        UpdateTime();
        UpdateBag();
        TestTime();
    }

    private void UpdatePoint()
    {
        pointsText.text = string.Format("{0:D9}", points);
        // move, goal 삭제 예정
    }

    private void UpdateTime()
    {
        if (isGameRunning)
        {
            CheckRemainTime();
        }

        // string.Format({0번째 매개변수:표시자리수}, {1번째 매개변수:표시자리수});
        // 00:30으로 표시됨

        timeText.text = string.Format("{0:D2} : {1:D2}", min, (int)sec);
    }

    private void CheckRemainTime()
    {
        sec -= Time.deltaTime;

        if (min != 0 && sec <= 0f)
        {
            min -= 1;
            sec = 59f;
        }
        else if (min == 0 && sec <= warningSec + 1 && sec >= warningSec)
        {
            WarningLeftTime();
        }
        else if (min == 0 && sec <= 0f)
        {
            StageFailed();
            return;
        }
    }

    private void UpdateBag()
    {
        if (bag1CurrentCount <= 0)
        {
            bag1CurrentCount = 0;
            bag1Text.color = Color.red;
        }

        if (bag2CurrentCount <= 0)
        {
            bag2CurrentCount = 0;
            bag2Text.color = Color.red;
        }

        if (bag3CurrentCount <= 0)
        {
            bag3CurrentCount = 0;
            bag3Text.color = Color.red;
        }

        if (bag4CurrentCount <= 0)
        {
            bag4CurrentCount = 0;
            bag4Text.color = Color.red;
        }

        bag1Text.text = bag1CurrentCount.ToString() + " / " + bag1GoalCount.ToString();
        bag2Text.text = bag2CurrentCount.ToString() + " / " + bag2GoalCount.ToString();
        bag3Text.text = bag3CurrentCount.ToString() + " / " + bag3GoalCount.ToString();
        bag4Text.text = bag4CurrentCount.ToString() + " / " + bag4GoalCount.ToString();
    }

    // TODO : 1. 매개변수 _subtractMoves 삭제 -> 스와이프 횟수 -로 종료조건일 때 했었음 
    public void ProcessTurn(int _pointsToGain, bool _subtractMoves, int _bag1AddCount, int _bag2AddCount, int _bag3AddCount, int _bag4AddCount)
    {
        if (!isGameRunning)
        {
            isGameRunning = true;
        }

        points += _pointsToGain;


        bag1CurrentCount += _bag1AddCount;
        if (bag1CurrentCount >= bag1GoalCount)
        {
            bag1CurrentCount = bag1GoalCount;
            bag1Check = true;
        }

        bag2CurrentCount += _bag2AddCount;
        bag3CurrentCount += _bag3AddCount;
        bag4CurrentCount += _bag4AddCount;

        // TODO : 1. 남은 시간 내에 바구니에 필요한 블럭 모았을 때 승리
        //if (points >= goal)
        //{
        //    // win game
        //    isGameEnded = true;

        //    // Display a victory screen.
        //    backgroundPanel.SetActive(true);
        //    clearPanel.SetActive(true);
        //    PotionBoard.Instance.potionParent.SetActive(false);
        //    isGameRunning = false;
        //    StartCoroutine(LerpClearPanelScale());
        //    return;
        //}

        if (bag1Check)
        {
            StageClear();
        }

    }

    private void WarningLeftTime()
    {
        timeText.color = Color.red;
        warningUI.SetActive(true);
        StartCoroutine(LerpWarningColor());
    }

    // 10초 되면 경고 UI 깜빡깜빡하도록(컬러의 a값(불투명도) 조절)
    private IEnumerator LerpWarningColor()
    {
        while (warningUI.GetComponent<Image>().color != fullWarningColor)
        {
            warningUI.GetComponent<Image>().color = Color.Lerp(originWarningColor, fullWarningColor, Mathf.PingPong(Time.time, 1));
            yield return null;
        }
    }

    // 결과창 크기 조절
    private IEnumerator LerpClearPanelScale()
    {
        bool firstCheck = false;
        bool lastCheck = false;
        float checkTime = 0;
        float lerpSpeed = 0.2f;

        while (!firstCheck || !lastCheck)
        {
            clearPanel.transform.localScale = Vector3.Lerp(firstResultScale, middleResultScale, Mathf.PingPong(checkTime += lerpSpeed, 1));
            Debug.Log(checkTime);
            if (clearPanel.transform.localScale.x >= middleResultScale.x - 0.1f)
            {
                firstCheck = true;
            }
            if (clearPanel.transform.localScale.x <= lastResultScale.x && firstCheck)
            {
                lastCheck = true;
            }
            yield return null;
        }
    }

    private IEnumerator LerpFailedPanelScale()
    {
        bool firstCheck = false;
        bool lastCheck = false;

        failedPanel.transform.localScale = Vector3.zero;

        while (!firstCheck || !lastCheck)
        {
            failedPanel.transform.localScale = Vector3.Lerp(firstResultScale, middleResultScale, Mathf.PingPong(Time.time, 1));
            Debug.Log(Mathf.PingPong(Time.time, 1));
            if (failedPanel.transform.localScale.x >= middleResultScale.x - 0.1f)
            {
                firstCheck = true;
            }
            if (failedPanel.transform.localScale.x <= lastResultScale.x && firstCheck)
            {
                lastCheck = true;
            }
            yield return null;
        }
    }

    private void StageClear()
    {
        isStageEnded = true;
        warningUI.SetActive(false);
        // Display a victory screen.
        backgroundPanel.SetActive(true);
        clearPanel.SetActive(true);
        PotionBoard.Instance.potionParent.SetActive(false);
        isGameRunning = false;
        StartCoroutine(LerpClearPanelScale());
    }

    private void StageFailed()
    {
        isStageEnded = true;
        warningUI.SetActive(false);
        backgroundPanel.SetActive(true);
        failedPanel.SetActive(true);
        PotionBoard.Instance.potionParent.SetActive(false);
        isGameRunning = false;
        StartCoroutine(LerpFailedPanelScale());
    }

    // 남은 시간 내에 바구니에 필요한 블럭 모았을 때 승리
    // 현재 결과창 모든 버튼 이거 사용중 : HomeButton, RestartButton, NextButton, MapButton
    // TODO : 1. 버튼마다 각각 메서드 만들어야 함
    private void WinGame()
    {
        //SceneManager.LoadScene("Main Menu");
        // string으로 할 수도 있고 인덱스 줘서 띄울 수도 있음
        SceneManager.LoadScene(0);
    }

    // 남은 시간이 다 지나면 패배
    private void LoseGame()
    {

        //SceneManager.LoadScene("Main Menu");
        // string으로 할 수도 있고 인덱스 줘서 띄울 수도 있음
        //SceneManager.LoadScene(0);
    }

    private void TestTime()
    {
        //float lerpSpeed = 0.2f;
        //Debug.Log(Vector3.Lerp(firstResultScale, middleResultScale, Mathf.PingPong(checkTime += lerpSpeed, 1)));
    }
}
