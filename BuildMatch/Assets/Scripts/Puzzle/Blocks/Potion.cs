using System.Collections;
using UnityEngine;

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
    public int spacingX;
    public int spacingY;

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

    // 유저가 드래그해서 움직인 블럭(해당 블럭과 스와이프로 바뀌는 블럭) -> 현재 클릭한 위치 블럭 애니메이션 체크
    public bool isSwipeMoving;

    // 현재 움직여온 블럭 -> 조합 체크 시 특수 블럭 생성될 위치 체크용
    public bool isChangedBlock;

    public PotionType changedSpecialBlockType;

    // 클릭했을 때 이미지 변경하기 위한 Sprite 모음
    [SerializeField]
    private Sprite[] sprites = new Sprite[2];

    public Potion(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
        currentSwipeable = false;
        isSwipeMoving = false;
        isChangedBlock = false;
        changedSpecialBlockType = PotionType.None;
    }

    public void SetIndicies(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    public void OnMouseDown()
    {
        if (!isSwipeMoving)
        {
            currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public void OnMouseDrag()
    {
        // 블럭 제거 끝날 때까지 마우스 클릭해도 블럭 빛나는 효과 적용 X
        if (!isSwipeMoving && !PotionBoard.Instance.isProcessMoving)
        {
            if (sprites.Length == 2)
            {
                if (potionType == PotionType.DrillHorizontal || potionType == PotionType.PickLeft || potionType == PotionType.PickRight)
                {
                    SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                    spriteRenderer.sprite = sprites[1];
                }
                else
                {
                    GetComponent<SpriteRenderer>().sprite = sprites[1];
                }
            }
        }
    }

    public void OnMouseUp()
    {
        if (!isSwipeMoving)
        {
            if (sprites.Length == 2)
            {
                if (potionType == PotionType.DrillHorizontal || potionType == PotionType.PickLeft || potionType == PotionType.PickRight)
                {
                    SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                    spriteRenderer.sprite = sprites[0];
                }
                else
                {
                    GetComponent<SpriteRenderer>().sprite = sprites[0];
                }
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

    //   TODO : 2. 제거 중간에 조작하면 되버림
    public void MoveToTarget(Vector2 _targetPos)
    {
        //Debug.Log("move : [" + transform.position.x + ", " + transform.position.y + "] -> [" + _targetPos.x + ", " + _targetPos.y + "]");
        StartCoroutine(MoveCoroutine(_targetPos));
    }
 
    private IEnumerator MoveCoroutine(Vector2 _targetPos)
    {
        isSwipeMoving = true;
        isChangedBlock = true;

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

            transform.position = Vector2.Lerp(startPosition, _targetPos, t);

            elaspedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _targetPos;
        isSwipeMoving = false;
    }

    public void SetChangedSpecialBlockType(MatchDirection _matchDirection)
    {
        switch (_matchDirection)
        {
            case MatchDirection.Vertical_4:
                changedSpecialBlockType = PotionType.DrillVertical; 
                break;
            case MatchDirection.Horizontal_4:
                changedSpecialBlockType= PotionType.DrillHorizontal;
                break;
            case MatchDirection.LongVertical:
                changedSpecialBlockType = PotionType.Prism;
                break;
            case MatchDirection.LongHorizontal:
                changedSpecialBlockType= PotionType.Prism;
                break;
            case MatchDirection.Super:
                changedSpecialBlockType = PotionType.Bomb;
                break;
            case MatchDirection.Square:
                changedSpecialBlockType = PotionType.PickRight;
                break;
        }
    }
}

public enum PotionType
{
    // 기본 블럭
    RedBlock,
    OrangeBlock,
    YellowBlock,
    GreenBlock,
    BlueBlock,
    PurpleBlock,
    PinkBlock,
    // 특수 블럭
    Bomb, // 폭탄
    DrillVertical, // 드릴 세로
    DrillHorizontal, // 드릴 가로
    PickLeft, // 곡괭이 역대각(왼쪽 기울임)
    PickRight, // 곡괭이 대각(오른쪽 기울임)
    Prism, // 프리즘
    // None : null 대신 활용
    None
}