using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 게임 스코어, 게임 승리/패배 조건, UI 설정

// TODO : 1. 스테이지 설정 스테이지 당 남은 시간 및 목표 개수
//        
public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance; // static reference;

    private PuzzleUIManager _PuzzleUIManager = null;

    private StageManager _StageManager = null;

    private PotionBoard board;

    public float warningSec; // 경고 뜨는 남은 기준 시간 (현재 10초)

    //public int goal;
    public int points; // 최대 숫자 9개까지 ex) 999999999

    // 남은 시간
    public int min;
    public float sec;

    // 스왑 이후부터 true로 변경되면서 시간 체크
    // true일때만 시간이 지나감
    public bool isGameRunning = false;

    // 남은 시간이 종료되었을 때 
    public bool isStageEnded;

    // TODO : 1. 나중에 따로 bag 컴포넌트로 관리 필요
    public GameObject[] bagPrefabs;

    private void Awake()
    {
        Instance = this;

        board = FindObjectOfType<PotionBoard>();
        _PuzzleUIManager = GetComponentInChildren<PuzzleUIManager>();
        _StageManager = GameManager.GetManagerClass<StageManager>();
    }

    private void Start()
    {
        _StageManager.MakeStageTimeSetupData();
        StageTimeSetup();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePoint();
        UpdateTime();
    }

    private void StageTimeSetup()
    {
        StageTimeData stageData = _StageManager.stageTimeData;
        min = stageData.min;
        sec = stageData.sec;
    }

    private void UpdatePoint()
    {
        _PuzzleUIManager.pointsText.text = string.Format("{0:D9}", points);
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

        _PuzzleUIManager.timeText.text = string.Format("{0:D2}:{1:D2}", min, (int)sec);
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
            _PuzzleUIManager.WarningLeftTime();
        }
        else if (min == 0 && sec <= 0f)
        {
            sec = 0;
            StageFailed();
            return;
        }
    }

    // TODO : 1. 매개변수 _subtractMoves 삭제 -> 스와이프 횟수 -로 종료조건일 때 했었음 
    public void ProcessTurn(int _pointsToGain, bool _subtractMoves)
    {
        if (!isGameRunning)
        {
            isGameRunning = true;
        }

        points += _pointsToGain;

        for (int i = 0; i < board.stageBagLength; i++)
        {
            Bag bag = board.stageBags[i].GetComponent<Bag>();
            if (!bag.ClearCheck)
            {
                break;
            } 
            // 마지막 체크까지 true이면 스테이지 클리어
            else if (bag.ClearCheck && i == board.stageBagLength - 1)
            {
                StageClear();
            }
        }
    }

    private void StageClear()
    {
        StartCoroutine(_PuzzleUIManager.LerpClearPanelScale());
    }

    // 클리어 보상 세팅
    // TODO: 1. 스테이지에 따라서 보상 세팅
    //       -> 받는 자원, 골드, 각각의 양    

        
    private void StageFailed()
    {
        StartCoroutine(_PuzzleUIManager.LerpFailedPanelScale());
    }
}
