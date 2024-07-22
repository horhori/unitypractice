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

    private FindMatches findMatches;
    private Board board;
    public GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    private Vector2 tempPosition;

    [Header("Swipe stuff")]
    public float swipeAngle = 0;
    // 쉽게 스와이프되는거 방지용
    public float swipeResist = 1f;

    [Header("Powerup stuff")]
    // 매칭 5 될때 한가지 색 다 지움
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBomb;
    public GameObject adjacentMarker;

    // Start is called before the first frame update
    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //row = targetY;
        //column = targetX;
        //// 맨 처음의 row, column을 기억하기 위해 사용
        //previousRow = row;
        //previousColumn = column;
    }

    // This is for testing and Debug only.
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true;
            GameObject color = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            color.transform.parent = this.transform;

        }
    }

    // Update is called once per frame
    void Update()
    {

        // 색깔 검정으로 바뀌는건 isMatched 되서임
        //if (isMatched)
        //{
        //    SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
        //    Color currentColor = mySprite.color;
        //    mySprite.color = new Color(currentColor.r, currentColor.g, currentColor.b, .5f);
        //}

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
            findMatches.FindAllMatches();
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
            findMatches.FindAllMatches();
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
        // 폭탄인지 체크
        if (isColorBomb)
        {
            // This piece is a color bomb, and the other piece is the color to destroy
            findMatches.MatchPiecesOfColor(otherDot.tag);
            isMatched = true;
        } else if(otherDot.GetComponent<Dot>().isColorBomb)
        {
            // The other piece is a color bomb, and this piece has the color to destroy
            findMatches.MatchPiecesOfColor(this.gameObject.tag);
            otherDot.GetComponent<Dot>().isMatched = true;
        }

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
                // 모든 블럭이 다시 제자리로 돌아간 후(0.5초 기다린 후) move 상태로 변경
                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.move;
            }
            else
            // match되면 Destroy 로직 실행
            {
                board.DestroyMatches();
            }
            //otherDot = null;
        } 
          
    } 

    private void OnMouseDown()
    {
        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        }
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }

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
            MovePieces();

            board.currentState = GameState.wait;
            board.currentDot = this;
        } else
        {
            board.currentState = GameState.move;

        }


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



    public void MakeRowBomb()
    {
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColumnBomb()
    {
        isColumnBomb = true;
        GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColorBomb()
    {
        isColorBomb = true;
        GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
        color.transform.parent = this.transform;
    }

    public void MakeAdjacentBomb()
    {
        isAdjacentBomb = true;
        GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
        marker.transform.parent = this.transform;
    }


}
