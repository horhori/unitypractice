using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;
    // 맨 처음의 row, column을 기억하기 위해 사용
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    private Board board;
    private GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    private Vector2 tempPosition;

    [SerializeField]
    private float swipeAngle = 0;

    // 쉽게 스와이프되는거 방지용
    public float swipeResist = 1f;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //row = targetY;
        //column = targetX;
        //// 맨 처음의 row, column을 기억하기 위해 사용
        //previousRow = row;
        //previousColumn = column;
    }

    // Update is called once per frame
    void Update()
    {
        FindMatches();

        // 색깔 검정으로 바뀌는건 isMatched 되서임
        if (isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(0f, 0f, 0f, .2f);
        }

        targetX = column;
        targetY = row;

        // X축 이동
        // Dot이 원래 위치하고 있어야 할 x, y 제자리에 없으면 원래대로 이동함
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            // Move Towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
        }
        else
        {
            // Directly set the position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;

        }

        // Y축 이동
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            // Move Towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
        }
        else
        {
            // Directly set the position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    // 코루틴 사용해서 매칭 안되는 경우 제자리로
    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(.5f);

        if (otherDot != null)
        {
            if(!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            // match되지 않으면 다시 되돌아감
            {
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
            }
            else
            // match되면 Destroy 로직 실행
            {
                board.DestroyMatches();
            }
            otherDot = null;
        } 
          
    } 

    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
    }

    void CalculateAngle()
    {
        // arctan(yDiff-xDiff) = x 사용
        // 각도를 구함
        // 45도 ~ 135도 = 위
        // -45도 ~ 45도 = 오른쪽
        // -135도 ~ -45도 = 아래
        // -135도 ~ 135도 = 왼쪽

        // swipeResist 이상의 입력을 했을 때만 적용인데 잘 안먹는거 같은데..
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        }

        MovePieces();
    }

    void MovePieces()
    {
        // -1 주는 이유 : edge로 스와이프하는 경우 에러나서
        if(swipeAngle > -45 && swipeAngle <= 45 && column < board.width-1)
        {
            // Right Swipe
            otherDot = board.allDots[column + 1, row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().column -= 1;
            column += 1;
        } 
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height-1)
        {
            // Up Swipe
            otherDot = board.allDots[column, row + 1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().row -= 1;
            row += 1;
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            // Left Swipe
            otherDot = board.allDots[column - 1, row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().column += 1;
            column -= 1;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            // Down Swipe
            otherDot = board.allDots[column, row -1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().row += 1;
            row -= 1;
        }
        StartCoroutine(CheckMoveCo());
    }

    // 여기서 Dot들을 isMatched true로 바꿈
    // tag를 바꾸지 않아서 에러나는듯 함
    // 가운데 기준으로 좌우, 위아래 tag가 같으면 셋 다 true로 바꿈
    void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if(leftDot1 != null &&  rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }

        }

        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }

        }
    }
}
