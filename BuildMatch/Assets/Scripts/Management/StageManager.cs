using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO : 1. PuzzleManager에 있는 Stage 관련 옮겨오기
//        2. 스테이지 세팅 -> 임시 DB에서 데이터 가져와서 해당 스테이지로 적용

public class StageManager : MonoBehaviour
{
    public static StageManager Instance; // static reference;

    // 현재 스테이지
    public int stageNumber;

    public void StageClearReward()
    {
        // 왼쪽 위부터 x, y position 값
        // (-238.8, 122), (-79.6, 122), (79.6, 122), (238.8, 122)
        // (-238.8, -59), (-79.6, -59), (79.6, -59), (238.8, -59)
    }
}
