using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static StageManager;

// TODO : 1. PuzzleManager에 있는 Stage 관련 옮겨오기
//        2. 스테이지 세팅 -> 임시 DB에서 데이터 가져와서 해당 스테이지로 적용

public class StageManager : MonoBehaviour, IManager
{
    public GameManager gameManager
    {
        get
        {
            return GameManager.gameManager;
        }
    }

    // 현재 스테이지
    public int stageNumber { get; set; }

    public StageBoardData stageBoardData { get; set; }

    public StageTimeData stageTimeData { get; set; }

    private void Start()
    {
        //StageBoardSetup();
    }

    public void MakeStageBoardSetupData()
    {
        int mapWidth;
        int mapHeight;
        PotionType[] appearedBlockList;
        GoalBag[] goalBags;
        int rewardGold;

        switch (stageNumber)
        {
            case 1:
                mapWidth = 7;
                mapHeight = 7;
                appearedBlockList = new PotionType[] { PotionType.RedBlock, PotionType.OrangeBlock, PotionType.YellowBlock, PotionType.GreenBlock, PotionType.BlueBlock };
                goalBags = new GoalBag[] { new GoalBag(PotionType.RedBlock, 15) };
                rewardGold = 100;
                break;
            case 2:
                mapWidth = 7;
                mapHeight = 7;
                appearedBlockList = new PotionType[] { PotionType.RedBlock, PotionType.OrangeBlock, PotionType.YellowBlock, PotionType.PurpleBlock, PotionType.PinkBlock };
                goalBags = new GoalBag[] { new GoalBag(PotionType.RedBlock, 15), new GoalBag(PotionType.OrangeBlock, 15), new GoalBag(PotionType.YellowBlock, 15) };
                rewardGold = 200;
                break;
            case 3:
                mapWidth = 8;
                mapHeight = 8;
                appearedBlockList = new PotionType[] { PotionType.OrangeBlock, PotionType.YellowBlock, PotionType.GreenBlock, PotionType.BlueBlock, PotionType.PurpleBlock, PotionType.PinkBlock };
                goalBags = new GoalBag[] { new GoalBag(PotionType.GreenBlock, 20), new GoalBag(PotionType.BlueBlock, 20), new GoalBag(PotionType.PurpleBlock, 20) };
                rewardGold = 300;
                break;
            case 4:
                mapWidth = 8;
                mapHeight = 8;
                appearedBlockList = new PotionType[] { PotionType.RedBlock, PotionType.OrangeBlock, PotionType.YellowBlock, PotionType.GreenBlock, PotionType.BlueBlock, PotionType.PurpleBlock, PotionType.PinkBlock };
                goalBags = new GoalBag[] { new GoalBag(PotionType.OrangeBlock, 10), new GoalBag(PotionType.YellowBlock, 10), new GoalBag(PotionType.GreenBlock, 10), new GoalBag(PotionType.BlueBlock, 10), new GoalBag(PotionType.PurpleBlock, 10) };
                rewardGold = 400;
                break;
            case 5:
                mapWidth = 9;
                mapHeight = 9;
                appearedBlockList = new PotionType[] { PotionType.RedBlock, PotionType.OrangeBlock, PotionType.YellowBlock, PotionType.GreenBlock, PotionType.BlueBlock, PotionType.PurpleBlock, PotionType.PinkBlock };
                goalBags = new GoalBag[] { new GoalBag(PotionType.RedBlock, 15), new GoalBag(PotionType.OrangeBlock, 15), new GoalBag(PotionType.YellowBlock, 15), new GoalBag(PotionType.GreenBlock, 15), new GoalBag(PotionType.BlueBlock, 15), new GoalBag(PotionType.PurpleBlock, 15), new GoalBag(PotionType.PinkBlock, 15) };
                rewardGold = 500;
                break;
            // 스테이지 0(테스트, puzzleScene에서 바로 실행)인 경우 스테이지 1로 적용
            default:
                mapWidth = 7;
                mapHeight = 7;
                appearedBlockList = new PotionType[] { PotionType.RedBlock, PotionType.OrangeBlock, PotionType.YellowBlock, PotionType.GreenBlock, PotionType.BlueBlock };
                goalBags = new GoalBag[] { new GoalBag(PotionType.RedBlock, 15) };
                rewardGold = 100;
                break;

        }

        stageBoardData = new StageBoardData(stageNumber, mapWidth, mapHeight, appearedBlockList, goalBags, rewardGold);
    }

    public void MakeStageTimeSetupData()
    {
        int min;
        float sec;

        switch (stageNumber)
        {


            case 1:
                min = 0;
                sec = 30.0f;
                break;
            case 2:
                min = 1;
                sec = 0f;
                break;
            case 3:
                min = 1;
                sec = 30.0f;
                break;
            case 4:
                min = 2;
                sec = 0f;
                break;
            case 5:
                min = 2;
                sec = 30.0f;
                break;
            // 스테이지 0(테스트, puzzleScene에서 바로 실행)인 경우 스테이지 1로 적용
            default:
                min = 0;
                sec = 30.0f;
                break;
        }

        stageTimeData = new StageTimeData(min, sec);
    }

    public void StageClearReward()
    {
        // 왼쪽 위부터 x, y position 값
        // (-238.8, 122), (-79.6, 122), (79.6, 122), (238.8, 122)
        // (-238.8, -59), (-79.6, -59), (79.6, -59), (238.8, -59)
    }
}

public struct StageBoardData
{
    public int stageNumber { get; set; }
    public int mapWidth { get; set; }
    public int mapHeight { get; set; }
    public PotionType[] appearedBlockList { get; set; }
    public GoalBag[] goalBags { get; set; }
    public int rewardGold { get; set; }

    public StageBoardData(int _stageNumber, int _mapWidth, int _mapHeight, PotionType[] _appearedBlockList, GoalBag[] _goalBags, int _rewardGold)
    {
        stageNumber = _stageNumber;
        mapWidth = _mapWidth;
        mapHeight = _mapHeight;
        appearedBlockList = _appearedBlockList;
        goalBags = _goalBags;
        rewardGold = _rewardGold;
    }
}

public struct StageTimeData
{
    public int min { get; set; }
    public float sec { get; set; }

    public StageTimeData(int _min, float _sec)
    {
        min = _min;
        sec = _sec;
    }
}

public struct GoalBag
{
    public PotionType targetBlock { get; set; }
    public int currentNumber { get; set; }
    public int goalNumber { get; set; }

    public GoalBag( PotionType _targetBlock, int _goalNumber )
    {
        targetBlock = _targetBlock;
        currentNumber = 0;
        goalNumber = _goalNumber;
    }
}

// TODO : 1. 클래스로 관리
public class BoardBag
{
    // 해당 스테이지 바구니 갯수
    public int stageBagLength;

    // 해당 스테이지 세팅 바구니 목록
    public GameObject[] stageBags;

    // 바구니 원래 위치
    public GameObject bagParent;
}


// 스테이지 데이터
// ID       DifficultLevel  MapWidth    MapHeight
// a10001   1               7           7         
// a20001   2               7           7         
// a30001   3               8           8          
// a40001	4	            8           8
// a50001	5	            9           9

// 스테이지 등장 블럭 리스트
//ID	    Distraction     AppearedBlockList                                   DisappearedBlockList
//a10001    null            { red, orange, yellow, green, blue }                { purple, pink }
//a20001	중앙 세로 1줄    { red, orange, yellow, purple, pink }               { green, blue }
//a30001	가로 중앙 2줄    { orange, yellow, green, blue, purple, pink }       { red }
//a40001	가로 중앙 2줄	{ red, orange, yellow, green, blue, purple, pink }  null
//a50001	중앙 십자 1줄    { red, orange, yellow, green, blue, purple, pink }  null

// 3매치 목표 데이터
//ID	    StageLevel	Priority_ID
//a10001    1	
//a20001	2	        a10001
//a30001	3	        a20001
//a40001	4	        a30001
//a50001	5	        a40001

// 3매치 목표 요구 데이터
//ID	    Content	        ClearNum
//a10001	빨강 블럭 제거	15
//a20001	빨강 블럭 제거	15
//a20001	주황 블럭 제거	15
//a20001	노랑 블럭 제거	15
//a30001	초록 블럭 제거	20
//a30001	파랑 블럭 제거	20
//a30001	보라 블럭 제거	20
//a40001	주황 블럭 제거	10
//a40001	노랑 블럭 제거	10
//a40001	초록 블럭 제거	10
//a40001	파랑 블럭 제거	10
//a40001	보라 블럭 제거	10
//a50001	빨강 블럭 제거	15
//a50001	주황 블럭 제거	15
//a50001	노랑 블럭 제거	15
//a50001	초록 블럭 제거	15
//a50001	파랑 블럭 제거	15
//a50001	보라 블럭 제거	15
//a50001	핑크 블럭 제거	15

// 3매치 목표 보상 데이터
//    ID	RewardInfo	    RewardNum
//a10001	2스테이지 해금	
//a10001	골드      	    100
//a20001	3스테이지 해금	
//a20001	골드	            200
//a30001	4스테이지 해금	
//a30001	골드	            300
//a40001	5스테이지 해금	
//a40001	골드	            400
//a50001	골드	            500

// 스테이지별 시간초
//
//
//