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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        stageText.text = "Stage " + stageNumber;
    }

    // Update is called once per frame
    void Update()
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

    public void ProcessTurn(int _pointsToGain, bool _subtractMoves)
    {
        if (!isGameRunning)
        {
            isGameRunning = true;
        }

        points += _pointsToGain;

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
