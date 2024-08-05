using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

// Potion -> Block
// 이름 변경 시 메모리 부족 발생

// TODO : 1. Potion -> Block 리팩토링
public class Potion : MonoBehaviour
{
    public PotionType potionType;

    // 좌표를 의미하는게 아니라 번째임
    public int xIndex;
    public int yIndex;

    // TODO : 1. PotionBoard의 해당 값을 가져와야 함(현재는 임시로 unity GUI로 3 설정(7x7이니까 3임)
    //        2. 스테이지 세팅하면서 PotionBoard와 여기서의 spacingX, Y 값 Manager로 관리
    public int spacingX = 3;
    public int spacingY = 3;

    // 현재 사용안하는중 -> 사용하도록 바꾸는중
    // isMatched 체크되면 무조건 지워지는 블럭됨
    public bool isMatched;

    // TODO : 1. 해당 블럭이 스왑되면 매칭이 되는지 여부 (true면 발광, 위아래로 살짝 움직임 적용)
    //public bool isMatchable;

    private Vector2 currentPos; // firstTouchPosition
    private Vector2 targetPos; // finalTouchPosition
    public bool currentSwipeable;
    public float swipeAngle = 0;
    public float swipeResist = 0.2f;

    public bool isMoving;

    // 클릭했을 때 이미지 변경하기 위한 Sprite 모음
    [SerializeField]
    private Sprite[] sprites = new Sprite[2];

    // 특수블럭 체크
    //public bool isBomb;
    //public bool isDrillVertical;
    //public bool isDrillVertical;
    //public bool isDrillVertical;  
    //public bool isDrillVertical;

    public Potion(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
        currentSwipeable = false;
        isMoving = false;
    }

    public void SetIndicies(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    public void OnMouseDown()
    {
        if (!isMoving)
        {
            currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public void OnMouseDrag()
    {
        if (!isMoving)
        {
            if (sprites.Length == 2)
            {
                GetComponent<SpriteRenderer>().sprite = sprites[1];
            }
        }
    }

    public void OnMouseUp()
    {
        if (!isMoving)
        {
            if (sprites.Length == 2)
            {
                GetComponent<SpriteRenderer>().sprite = sprites[0];
            }
            targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentSwipeable = CalculateAngle();
        }
    }

    bool CalculateAngle()
    {
        // swipeResist 이상의 입력을 했을 때만 적용
        // swipeResist : 1f = 보석 전체 거리 0.5f = 보석 절반 거리
        // swipeResist : 0.2f로 설정했음.
        if (Mathf.Abs(targetPos.y - currentPos.y) > swipeResist || Mathf.Abs(targetPos.x - currentPos.x) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(targetPos.y - currentPos.y, targetPos.x - currentPos.x) * 180 / Mathf.PI;
        }

        return Mathf.Abs(targetPos.y - currentPos.y) > swipeResist || Mathf.Abs(targetPos.x - currentPos.x) > swipeResist;
    }

    // 스테이지 클리어 시 에러 발생했는데 원인 아직 못찾음?
    // TODO : 1. 제거 & 생성 코루틴 도중 게임 클리어되면 여기서 에러남
    //        2. 제거 중간에 조작하면 되버림
    public void MoveToTarget(Vector2 _targetPos)
    {
        //Debug.Log("move : [" + transform.position.x + ", " + transform.position.y + "] -> [" + _targetPos.x + ", " + _targetPos.y + "]");
        StartCoroutine(MoveCoroutine(_targetPos));
    }

    // TODO : 1. 블럭 바뀌는 시간 자연스럽게 조절
    //        2. 새로 블럭 생성되어 빈 자리에 떨어지는 것도 여기서 처리하는데 따로 메서드 만들어야 할듯
    private IEnumerator MoveCoroutine(Vector2 _targetPos)
    {
        isMoving = true;

        Vector2 startPosition = transform.position;

        float distance = Vector2.Distance(startPosition, _targetPos);
        // 스왑 애니메이션 지속 시간임
        // 속력 = 거리 / 0.1f * distance 하여 속력 일정하게
        // 기존에 시간 0.2f였는데 느려서 0.06f * distance로 조정
        float duration = 0.06f * distance;

        float elaspedTime = 0f;

        // 움직이는 속도 일정하게 여기서
        while (elaspedTime < duration)
        {
            float t = elaspedTime / duration;

            //Debug.Log("potion : " + this);
            //Debug.Log("time : " + t);

            transform.position = Vector2.Lerp(startPosition, _targetPos, t);

            elaspedTime += Time.deltaTime;

            yield return null;
            //yield return new WaitForSeconds(0.01f);
        }

        transform.position = _targetPos;
        isMoving = false;
    }
}

// TODO : 1. 특수 블럭
public enum PotionType
{
    // 기본 블럭
    BlueBlock,  
    GreenBlock,
    OrangeBlock,
    PinkBlock,
    PurpleBlock,
    RedBlock,
    YellowBlock,
    // 특수 블럭
    Bomb, // 폭탄
    DrillVertical, // 드릴 세로
    DrillHorizontal, // 드릴 가로
    //Pick, // 곡괭이
    PickLeft, // 곡괭이 역대각(왼쪽 기울임)
    PickRight, // 곡괭이 대각(오른쪽 기울임)
    Prism, // 프리즘
    
}