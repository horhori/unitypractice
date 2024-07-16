using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// 게임 스코어, 게임 승리/패배 조건, UI 설정

// TODO : 1. UI 변경
//        2. 게임 종료 시간 설정
//        3. 승리 / 패배 종료 변경
//        4. 레벨 설정
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // static reference;

    public GameObject backgroundPanel; // grey background 
    public GameObject victoryPanel; // 승리/패배 화면 클릭할 때 포션 동작 안되게 
    public GameObject losePanel;

    public int goal; // the amount of points you need to get to win.
    public int moves; // the number of turns you can take
    public int points; // the crrent points you have earned.

    // 남은 시간


    // 남은 시간이 종료되었을 때 
    public bool isGameEnded;

    public TMP_Text stageText;
    public TMP_Text pointsText;
    public TMP_Text timeText;

    // move, goal 삭제 예정
    public TMP_Text movesText;
    public TMP_Text goalText;
    

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(int _moves, int _goal)
    {
        // move, goal 삭제 예정
        moves = _moves;
        goal = _goal;
    }

    // Update is called once per frame
    void Update()
    {
        pointsText.text = "Points: " + points.ToString();
        // move, goal 삭제 예정
        movesText.text = "Moves: " + moves.ToString();
        goalText.text = "Points: " + goal.ToString();
    }

    public void ProcessTurn(int _pointsToGain, bool _subtractMoves)
    {
        points += _pointsToGain;

        // TODO : 횟수는 삭제
        if (_subtractMoves)
        {
            moves--;
        }

        // TODO : 남은 시간 내에 바구니에 필요한 블럭 모았을 때 승리
        if (points >= goal)
        {
            // win game
            isGameEnded = true;

            // Display a victory screen.
            backgroundPanel.SetActive(true);
            victoryPanel.SetActive(true);
            PotionBoard.Instance.potionParent.SetActive(false);
            return;
        }
        // TODO : 남은 시간이 다 지나면 패배
        if (moves == 0)
        {
            // lose game
            isGameEnded = true;
            backgroundPanel.SetActive(true);
            losePanel.SetActive(true);
            PotionBoard.Instance.potionParent.SetActive(false);

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
