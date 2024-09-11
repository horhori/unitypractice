using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// TODO : 1. PuzzleManager�� �ִ� Stage ���� �Űܿ���
//        2. �������� ���� -> �ӽ� DB���� ������ �����ͼ� �ش� ���������� ����

public class StageManager : MonoBehaviour, IManager
{
    public GameManager gameManager
    {
        get
        {
            return GameManager.gameManager;
        }
    }

    // ���� ��������
    public int stageNumber { get; set; } = 1;

    public StageData stageData { get; set; }

    private void Start()
    {
        StageSetup();
    }

    public void StageSetup()
    {
        int stageLevel = stageNumber;
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
                mapWidth = 7;
                mapHeight = 7;
                appearedBlockList = new PotionType[] { PotionType.RedBlock, PotionType.OrangeBlock, PotionType.YellowBlock, PotionType.GreenBlock, PotionType.BlueBlock };
                goalBags = new GoalBag[] { new GoalBag(PotionType.RedBlock, 15) };
                rewardGold = 300;
                break;
            case 4:
                mapWidth = 7;
                mapHeight = 7;
                appearedBlockList = new PotionType[] { PotionType.RedBlock, PotionType.OrangeBlock, PotionType.YellowBlock, PotionType.GreenBlock, PotionType.BlueBlock };
                goalBags = new GoalBag[] { new GoalBag(PotionType.RedBlock, 15) };
                rewardGold = 400;
                break;
            case 5:
                mapWidth = 7;
                mapHeight = 7;
                appearedBlockList = new PotionType[] { PotionType.RedBlock, PotionType.OrangeBlock, PotionType.YellowBlock, PotionType.GreenBlock, PotionType.BlueBlock };
                goalBags = new GoalBag[] { new GoalBag(PotionType.RedBlock, 15) };
                rewardGold = 500;
                break;
            default:
                mapWidth = 7;
                mapHeight = 7;
                appearedBlockList = new PotionType[] { PotionType.RedBlock, PotionType.OrangeBlock, PotionType.YellowBlock, PotionType.GreenBlock, PotionType.BlueBlock };
                goalBags = new GoalBag[] { new GoalBag(PotionType.RedBlock, 15) };
                rewardGold = 100;
                break;

        }

        stageData = new StageData(stageLevel, mapWidth, mapHeight, appearedBlockList, goalBags, rewardGold);
    }

    public void StageClearReward()
    {
        // ���� ������ x, y position ��
        // (-238.8, 122), (-79.6, 122), (79.6, 122), (238.8, 122)
        // (-238.8, -59), (-79.6, -59), (79.6, -59), (238.8, -59)
    }
}

public struct StageData
{
    public int stageLevel { get; set; }
    public int mapWidth { get; set; }
    public int mapHeight { get; set; }
    public PotionType[] appearedBlockList { get; set; }
    public GoalBag[] goalBags { get; set; }
    public int rewardGold { get; set; }

    public StageData(int _stageLevel, int _mapWidth, int _mapHeight, PotionType[] _appearedBlockList, GoalBag[] _goalBags, int _rewardGold)
    {
        stageLevel = _stageLevel;
        mapWidth = _mapWidth;
        mapHeight = _mapHeight;
        appearedBlockList = _appearedBlockList;
        goalBags = _goalBags;
        rewardGold = _rewardGold;
    }
}

public struct GoalBag
{
    PotionType targetBlock;
    int targetNumber;

    public GoalBag( PotionType _targetBlock, int _targetNumber )
    {
        targetBlock = _targetBlock;
        targetNumber = _targetNumber;
    }
}


// �������� ������
// ID       DifficultLevel  MapWidth    MapHeight
// a10001   1               7           7         
// a20001   2               7           7         
// a30001   3               8           8          
// a40001	4	            8           8
// a50001	5	            9           9

// �������� ���� ���� ����Ʈ
//ID	    Distraction     AppearedBlockList                                   DisappearedBlockList
//a10001    null            { red, orange, yellow, green, blue }                { purple, pink }
//a20001	�߾� ���� 1��    { red, orange, yellow, purple, pink }               { green, blue }
//a30001	���� �߾� 2��    { orange, yellow, green, blue, purple, pink }       { red }
//a40001	���� �߾� 2��	{ red, orange, yellow, green, blue, purple, pink }  null
//a50001	�߾� ���� 1��    { red, orange, yellow, green, blue, purple, pink }  null

// 3��ġ ��ǥ ������
//ID	    StageLevel	Priority_ID
//a10001    1	
//a20001	2	        a10001
//a30001	3	        a20001
//a40001	4	        a30001
//a50001	5	        a40001

// 3��ġ ��ǥ �䱸 ������
//ID	    Content	        ClearNum
//a10001	���� ���� ����	15
//a20001	���� ���� ����	15
//a20001	��Ȳ ���� ����	15
//a20001	��� ���� ����	15
//a30001	�ʷ� ���� ����	20
//a30001	�Ķ� ���� ����	20
//a30001	���� ���� ����	20
//a40001	��Ȳ ���� ����	10
//a40001	��� ���� ����	10
//a40001	�ʷ� ���� ����	10
//a40001	�Ķ� ���� ����	10
//a40001	���� ���� ����	10
//a50001	���� ���� ����	15
//a50001	��Ȳ ���� ����	15
//a50001	��� ���� ����	15
//a50001	�ʷ� ���� ����	15
//a50001	�Ķ� ���� ����	15
//a50001	���� ���� ����	15
//a50001	��ũ ���� ����	15

// 3��ġ ��ǥ ���� ������
//    ID	RewardInfo	    RewardNum
//a10001	2�������� �ر�	
//a10001	���      	    100
//a20001	3�������� �ر�	
//a20001	���	            200
//a30001	4�������� �ر�	
//a30001	���	            300
//a40001	5�������� �ر�	
//a40001	���	            400
//a50001	���	            500