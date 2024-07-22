using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public class Board : MonoBehaviour
{
    // currentState가 move 상태일때만 마우스 클릭 동작함
    public GameState currentState = GameState.move;

    public int width;
    public int height;
    // 새로 생성된 Dot을 떨어뜨리기 위해 offSet만큼 위로 올려서 생성 
    public int offSet;
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject destroyEffect;
    private BackgroundTile[,] allTiles;
    public GameObject[,] allDots;
    private FindMatches findMatches;


    // Start is called before the first frame update
    void Start()
    {
        findMatches = FindObjectOfType<FindMatches>();
        allTiles = new BackgroundTile[width, height];
        allDots = new GameObject[width, height];
        SetUp();
    }

    private void SetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j + offSet);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity);
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + i + ". " + j + " )";

                // 다시 생성하는 횟수 100까지 하려고 함
                int maxIterations = 0;

                int dotToUse = Random.Range(0, dots.Length);

                while(MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                {
                    dotToUse = Random.Range(0, dots.Length);
                }
                maxIterations = 0;

                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.GetComponent<Dot>().row = j;
                dot.GetComponent<Dot>().column = i;

                dot.transform.parent = this.transform;
                dot.name = "( " + i + ". " + j + " )";
                allDots[i, j] = dot;
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        // column이 1보다 크면 왼쪽에 컬럼이 두개 있다는 거기 때문에 왼쪽으로 한번 가서 체크하고 또 한번 왼쪽으로 가서 체크
        // row도 마찬가지 1보다 크면 아래에 로우가 두개 있다는 뜻이기 때문에 아래로 한번 가서 체크하고 한번 더 아래로 체크
        // 세팅 시에 미리 매칭이 일어나지 않도록 하기 위함
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
            {
                return true;
            }

            if (allDots[column, row -1].tag == piece.tag && allDots[column, row-2].tag == piece.tag)
            {
                return true;
            }
        } 
        // 위 조건만 있으면 0, 1 컬럼과 0, 1 로우에서 매칭 발생함. 해당 컬럼 로우에 대한 조건
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }

            if (column > 1)
            {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column -2, row].tag == piece.tag)
                {
                    return true;
                }
            }

        }

        return false;
    }

    // isMatched true로 체크된 Dot Destroy
    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {
            findMatches.currentMatches.Remove(allDots[column, row]);
            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            // Instantiate만 하면 unity에서 메모리 삭제하지 않아서 GameObject로 받아서 Destroy 필요
            // Destroy 2번째 매개 변수로 초를 주면 해당 시간 지나서 Destroy됨
            Destroy(particle, .5f);
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int i=0; i<width; i++)
        {
            for (int j=0; j<height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCo());
    }

    // coroutine으로 빈 자리 row를 감소
    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for (int i=0; i<width; i++)
        {
            for (int j=0; j<height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                } else if(nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
                
            }
            nullCount = 0;
        }

        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    // 매칭 후 새로 채우기
    // 1. Dot 새로 생성

    private void RefillBoard()
    {
        for (int i=0; i<width; i++)
        {
            for (int j=0; j<height; j++)
            {
                if (allDots[i,j] == null)
                {
                    // 생성되는 곳 y에 offSet만큼 더함
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                }
            }
        }
    }

    // 2. 생성 후 매칭 체크
    private bool MatchesOnBoard()
    {
        for (int i=0; i<width; i++)
        {
            for (int j=0; j<height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while(MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }

        // 보드 채운 다음 pause 주고 move 상태로 변경
        // // 모든 블럭이 다시 제자리로 돌아간 후(0.5초 기다린 후) move 상태로 변경
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }
}
