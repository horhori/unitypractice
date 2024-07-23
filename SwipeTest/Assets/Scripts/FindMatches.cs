using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using Unity.VisualScripting;

// 1 frame에 매칭 로직 하기 위해 만듬
// 기존에 Dot 개별에서 처리하던 것을 보드 체킹 전용으로 따로 만듬
public class FindMatches : MonoBehaviour
{

    private Board board;
    // 매칭 체크된 Dot들을 리스트에 저장하기 위한
    public List<GameObject> currentMatches = new List<GameObject>();

    void Awake()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();

        if (dot1.isAdjacentBomb)
        {
            // List<>.Union() : System.Linq using(import)하여 사용. 간단하게 리스트에 추가해줌
            currentMatches.Union(GetAdjacentPieces(dot1.column, dot1.row));
        }

        if (dot2.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot2.column, dot2.row));
        }

        if (dot3.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot3.column, dot3.row));
        }

        return currentDots;
    }

    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();

        if (dot1.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot1.row));
        }

        if (dot2.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot2.row));
        }

        if (dot3.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot3.row));
        }

        return currentDots;
    }

    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();

        if (dot1.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot1.column));
        }

        if (dot2.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot2.column));
        }

        if (dot3.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot3.column));
        }

        return currentDots;
    }

    private void AddToListAndMatch(GameObject dot)
    {
        if (!currentMatches.Contains(dot))
        {
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    // 위에랑 다르게 GameObject로 매개변수 하는 이유는 리스트 추가 쉽게하려고
    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }

    // 여기서는 매칭만 체크하고 리스트에 담은 Dot을 Board에서 제거함
    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(.2f);
        for (int i=0; i<board.width; i++)
        {
            for (int j=0; j<board.height; j++)
            {
                GameObject currentDot = board.allDots[i, j];

                // 제거되는거 때문에 생성되기 전에 null일 수가 있음
                if (currentDot != null)
                {
                    Dot currentDotDot = currentDot.GetComponent<Dot>();

                    // 모서리에 붙어있는거 제외하고 매칭 체크(i가 0이거나 width인 경우 모서리 바로 옆에 있는거)
                    if (i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.allDots[i - 1, j];
                        GameObject rightDot = board.allDots[i + 1, j];

                        if (leftDot != null && rightDot != null)
                        {
                            Dot leftDotDot = leftDot.GetComponent<Dot>();
                            Dot rightDotDot = rightDot.GetComponent<Dot>();

                            if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {

                                currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot));

                                currentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot));

                                currentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rightDotDot));

                                GetNearbyPieces(leftDot, currentDot, rightDot);
                            }

                        }
                    }

                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject upDot = board.allDots[i, j + 1];
                        GameObject downDot = board.allDots[i, j - 1];
                        if (upDot != null && downDot != null)
                        {
                            Dot upDotDot = upDot.GetComponent<Dot>();
                            Dot downDotDot = downDot.GetComponent<Dot>();

                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {
                                currentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot));

                                currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot));

                                currentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot));

                                GetNearbyPieces(upDot, currentDot, downDot);
                            }
                        }    
                    }
                }
            }
        }
    }



    public void MatchPiecesOfColor(string color)
    {
        for (int i=0; i<board.width; i++)
        {
            for (int j=0; j<board.height; j++)
            {
                // Check if that piece exists
                if (board.allDots[i, j] != null)
                {
                    // Check the tag on that dot
                    if (board.allDots[i, j].tag == color)
                    {
                        // Set that dot to be matched
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetAdjacentPieces(int column, int row)
    {
        List<GameObject> dots = new List<GameObject>();

        // 3x3 가운데 dot 기준으로 왼쪽 아래 점부터 오른쪽 위 점까지만 루프
        for (int i = column - 1; i <= column + 1; i++) 
        {
            for (int j = row - 1; j <= row + 1; j++)
            {
                // Check if the piece is inside the board 모서리 체크
                if(i >= 0 && i < board.width && j >= 0 && j < board.height) {
                    dots.Add(board.allDots[i, j]);
                    board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                }
            }
        }

        return dots;
    }

    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();

        for (int i = 0; i< board.height; i++)
        {
            if (board.allDots[column , i] != null)
            {
                dots.Add(board.allDots[column, i]);
                board.allDots[column, i].GetComponent<Dot>().isMatched = true;
            }
        }

        return dots;
    }

    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();

        for (int i = 0; i < board.width; i++)
        {
            if (board.allDots[i, row] != null)
            {
                dots.Add(board.allDots[i, row]);
                board.allDots[i, row].GetComponent<Dot>().isMatched = true;
            }
        }

        return dots;
    }

    public void CheckBombs()
    {
        // Did the player move something?
        if (board.currentDot != null)
        {
            // Is the piece they moved matched?
            if (board.currentDot.isMatched)
            {
                // make it unmatched
                board.currentDot.isMatched = false;
                // Decide what kind of bomb to make
                // 50% 확률로 세로 드릴 or 가로 드릴
                //int typeOfBomb = Random.Range(0, 100);
                //if (typeOfBomb < 50)
                //{
                //    // Make a row bomb
                //    board.currentDot.MakeRowBomb();
                //} else if (typeOfBomb >= 50)
                //{
                //    // make a column bomb
                //    board.currentDot.MakeColumnBomb();

                // 왼쪽 or 오른쪽 스와이프인 경우
                if((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45) || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                {
                    board.currentDot.MakeRowBomb();
                } else
                {
                    board.currentDot.MakeColumnBomb();
                }
            }
        } 
        // Is the other piece matched?
        else if(board.currentDot.otherDot != null) {
            // make it
            Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>();
            // Is the other Dot matched?
            if(otherDot.isMatched)
            {
                // Make it unmatched
                otherDot.isMatched = false;
                // Decide what kind of bomb to make
                //int typeOfBomb = Random.Range(0, 100);
                //if (typeOfBomb < 50)
                //{
                //    // Make a row bomb
                //    otherDot.otherDot.GetComponent<Dot>().MakeRowBomb();
                //}
                //else if (typeOfBomb >= 50)
                //{
                //    // make a column bomb
                //    otherDot.otherDot.GetComponent<Dot>().MakeRowBomb();
                //}

                // 왼쪽 or 오른쪽 스와이프인 경우
                if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45) || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                {
                    otherDot.MakeRowBomb();
                }
                else
                {
                    otherDot.MakeColumnBomb();
                }
            }
        }
        }
    }


