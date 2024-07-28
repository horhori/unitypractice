using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// 게임 스코어, 게임 승리/패배 조건, UI 설정

// TODO : 1. 승리 / 패배 종료 변경
//           -> 남은 시간이 다 되었을 때 패배 처리 완료. 바구니가 다 채워지면 클리어되는 것으로는 진행해야 함.
//        2. 레벨 설정 레벨 당 남은 시간 및 목표 개수
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // static reference;

    public GameObject backgroundPanel; // grey background 승리/패배 화면 클릭할 때 포션 동작 안되게 
    public GameObject victoryPanel; 
    public GameObject losePanel;

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
    public bool isGameEnded;

    public TMP_Text stageText;
    public TMP_Text pointsText;
    public TMP_Text timeText;

    // 나중에 따로 bag 컴포넌트로 관리 필오
    public Sprite[] bagSprites;

    public GameObject bag1;
    private TMP_Text bag1Text;
    private int bag1CurrentCount;
    private int bag1GoalCount;
    public PotionType bag1Type;
    public GameObject bag2;
    private TMP_Text bag2Text;
    private int bag2CurrentCount;
    private int bag2GoalCount;
    public PotionType bag2Type;
    public GameObject bag3;
    private TMP_Text bag3Text;
    private int bag3CurrentCount;
    private int bag3GoalCount;
    public PotionType bag3Type;
    public GameObject bag4;
    private TMP_Text bag4Text;
    private int bag4CurrentCount;
    private int bag4GoalCount;
    public PotionType bag4Type;


    private void Awake()
    {
        Instance = this;
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
        bag2Text = bag2.GetComponentInChildren<TMP_Text>();
        bag3Text = bag3.GetComponentInChildren<TMP_Text>();
        bag4Text = bag4.GetComponentInChildren<TMP_Text>();
        bag1Type = PotionType.BlueBlock;
        bag1GoalCount = 25;
        bag1CurrentCount = bag1GoalCount;
        bag2Type = PotionType.GreenBlock;
        bag2GoalCount = 30;
        bag2CurrentCount = bag2GoalCount;
        bag3Type = PotionType.PinkBlock;
        bag3GoalCount = 35;
        bag3CurrentCount = bag3GoalCount;
        bag4Type = PotionType.RedBlock;
        bag4GoalCount = 40;
        bag4CurrentCount = bag4GoalCount;

        bag1Text.text = bag1CurrentCount.ToString() + " / " + bag1GoalCount.ToString();
        bag2Text.text = bag2CurrentCount.ToString() + " / " + bag2GoalCount.ToString();
        bag3Text.text = bag3CurrentCount.ToString() + " / " + bag3GoalCount.ToString();
        bag4Text.text = bag4CurrentCount.ToString() + " / " + bag4GoalCount.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePoint();
        UpdateBag();
    }

    private void UpdatePoint()
    {
        pointsText.text = string.Format("{0:D9}", points);
        // move, goal 삭제 예정
        //movesText.text = "Moves: " + moves.ToString();
        //goalText.text = "Points: " + goal.ToString();
        // string.Format({0번째 매개변수:표시자리수}, {1번째 매개변수:표시자리수});
        // 00:30으로 표시됨

        if (isGameRunning)
        {
            CheckRemainTime();
        }

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
        else if (min == 0 && sec <= 11f && sec >= 10f)
        {
            timeText.color = Color.red;
            // TODO : 1. 위험 효과 추가
        }
        else if (min == 0 && sec <= 0f)
        {
            // lose game
            isGameEnded = true;
            backgroundPanel.SetActive(true);
            losePanel.SetActive(true);
            PotionBoard.Instance.potionParent.SetActive(false);
            isGameRunning = false;
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
    public void ProcessTurn(int _pointsToGain, bool _subtractMoves, int _bag1SubtractCount, int _bag2SubtractCount, int _bag3SubtractCount, int _bag4SubtractCount)
    {
        if (!isGameRunning)
        {
            isGameRunning = true;
        }

        points += _pointsToGain;

        bag1CurrentCount -= _bag1SubtractCount;
        bag2CurrentCount -= _bag2SubtractCount;
        bag3CurrentCount -= _bag3SubtractCount;
        bag4CurrentCount -= _bag4SubtractCount;

        // TODO : 남은 시간 내에 바구니에 필요한 블럭 모았을 때 승리
        if (points >= goal)
        {
            // win game
            isGameEnded = true;

            // Display a victory screen.
            backgroundPanel.SetActive(true);
            victoryPanel.SetActive(true);
            PotionBoard.Instance.potionParent.SetActive(false);
            isGameRunning = false;
            return;
        }

    }

    // 남은 시간 내에 바구니에 필요한 블럭 모았을 때 승리
    public void WinGame()
    {
        //SceneManager.LoadScene("Main Menu");
        // string으로 할 수도 있고 인덱스 줘서 띄울 수도 있음
        SceneManager.LoadScene(0);
    }

    // 남은 시간이 다 지나면 패배
    public void LoseGame()
    {
        //SceneManager.LoadScene("Main Menu");
        // string으로 할 수도 있고 인덱스 줘서 띄울 수도 있음
        SceneManager.LoadScene(0);
    }
}
