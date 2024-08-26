using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 스테이지 데이터

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

[Serializable]
public class StageData : ScriptableObject
{
    public string id;
    public int level;
    public string priority_id;
    public Bag[] objectBagList;
    public bool rewardCheckStage;
    public int rewardGold;
}