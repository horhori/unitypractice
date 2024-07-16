using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PotionBoard : MonoBehaviour
{
    // 가로 세로 블럭 개수 설정
    public int width = 7;
    public int height = 7;
    // X축, Y축 간격
    public float spacingX;
    public float spacingY;
    // 블럭 목록
    public GameObject[] potionPrefabs;
    // 블럭보드
    public Node[,] potionBoard;
    public GameObject potionBoardGO;

    // 매칭되었을 때 제거할 블럭 목록
    // 매칭되는 블럭들을 추가후 제거하고 비우고 반복
    public List<GameObject> potionsToDestroy = new();

    [SerializeField]
    List<Potion> potionsToRemove = new();

    // 블럭 원래 위치
    // 블럭이 제거되고 새로 생성될 때 해당 위치 참조
    public GameObject potionParent;

    // unity 상에서 선택된 블럭 확인할 수 있게 SerializeField(직렬화) 사용
    // SerializeField : private이여도 unity에서 확인할 수 있음
    [SerializeField]
    private Potion selectedPotion;

    [SerializeField]
    private bool isProcessingMove;

    // Unity 상에서 쉽게 특정 위치 안 나오게 
    public ArrayLayout arrayLayout;
    // static Instance
    public static PotionBoard Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeBoard();
    }

    // TODO : 1. 현재 조작 클릭 -> 슬라이드 변경
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Potion>())
            {
                if (isProcessingMove)
                {
                    return;
                }

                Potion potion = hit.collider.gameObject.GetComponent<Potion>();
                Debug.Log("I have a clicked a potion it is : " + potion.gameObject);

                SelectPotion(potion);
            }
        }
    }

    // 보드 생성
    // width, height 값에 따라 보드 판 생성.
    // 각 보드 자리마다 Node를 가지고 있음
    // Node는 사용가능한 자리인지(isUsable), 사용 가능하면 해당 자리에 기물(Potion->Block)을 가짐
    // 랜덤하게 생성 후 매치되는 경우에는 다시 생성됨
    void InitializeBoard()
    {
        DestroyPotions();

        potionBoard = new Node[width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = (float)(height - 1) / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 블럭 보드 부모 위치를 반영하여 블럭 생성
                Vector2 position = new Vector2(x - spacingX, y - spacingY);
                if (arrayLayout.rows[y].row[x])
                {
                    potionBoard[x, y] = new Node(false, null);
                }
                else
                {
                    // 재료 등급 조정
                    int randomIndex = Random.Range(0, potionPrefabs.Length);

                    GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
                    potion.transform.SetParent(potionParent.transform);

                    potion.GetComponent<Potion>().SetIndicies(x, y);
                    potionBoard[x, y] = new Node(true, potion);
                    potionsToDestroy.Add(potion);
                }
            }
        }

        if (CheckBoard())
        {
            Debug.Log("We have matches let's re-create the board");
            InitializeBoard();
        }
    }

    private void DestroyPotions()
    {
        if (potionsToDestroy != null)
        {
            foreach (GameObject potion in potionsToDestroy)
            {
                Destroy(potion);
            }
            potionsToDestroy.Clear();
        }
    }

    // 보드에 매칭되어 있는게 있는지 체크
    // TODO : 1. 체크 시 매칭 경우의 수가 없는 경우 다시 섞여야 함
    //        2. 일정 시간이 지나고 조작이 없는 경우 매칭되는 블럭 표시
    public bool CheckBoard()
    {
        if (GameManager.Instance.isGameEnded)
        {
            return false;
        }
        Debug.Log("Checking Board");
        bool hasMatched = false;

        potionsToRemove.Clear();

        foreach(Node nodePotion in potionBoard)
        {
            if (nodePotion.potion != null)
            {
                nodePotion.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // checking if potion node is usable
                if (potionBoard[x, y].isUsable)
                {
                    // then proceed to get potion class in node.
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();

                    // ensure its not matched
                    if (!potion.isMatched)
                    {
                        // run some matching logic

                        MatchResult matchedPotions = IsConnected(potion);

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            // complex matching...
                            MatchResult superMatchedPotions = SuperMatch(matchedPotions);

                            potionsToRemove.AddRange(superMatchedPotions.connectedPotions);

                            foreach (Potion pot in superMatchedPotions.connectedPotions)
                            {
                                pot.isMatched = true;
                            }

                            hasMatched = true;
                        }
                    }
                }
            }
        }

        return hasMatched;
    }

    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
        foreach (Potion potionToRemove in potionsToRemove)
        {
            potionToRemove.isMatched = false;
        }

        RemoveAndRefill(potionsToRemove);

        // 현재 제거되는 블럭 당 1점으로 점수 카운트 됨
        GameManager.Instance.ProcessTurn(potionsToRemove.Count, _subtractMoves);
        yield return new WaitForSeconds(0.4f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }

    #region Cascading Potions

    // 블럭 지워지고 다시 생성
    // TODO : 1. 처음 보드 생성될 때 특정 갯수(7*14, 7*21...) 생성
    private void RemoveAndRefill(List<Potion> _potionsToRemove)
    {
        // Removing the potion and clearing the board at that location
        foreach (Potion potion in _potionsToRemove)
        {
            // getting it's x and y indicies and storing them
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            // Destroy the potion
            Destroy(potion.gameObject);

            // Create a blank node on the potion board
            potionBoard[_xIndex, _yIndex] = new Node(true, null);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x,y].potion ==null)
                {
                    Debug.Log("The location X: " + x + "Y: " + y + " is empty, attempting to refill it.");
                    RefillPotion(x, y);
                }
            }
        }
    }



    // RefillPotions
    private void RefillPotion(int x, int y)
    {
        // y offset
        int yOffset = 1;

        // while the cell above our current cell is null and we're below the height of the board
        while (y + yOffset < height && potionBoard[x,y + yOffset].potion == null)
        {
            // increment y offset
            Debug.Log("The potion above me is null, but i'm not at the top of the board yet, so add to my yOffset and try again, Current Offset is : " + yOffset + " I'm about to add 1.");
            yOffset++;
        }

        // we've either hit the top of the board or we found a potion

        if (y + yOffset < height && potionBoard[x, y+ yOffset].potion != null)
        {
            // we've found a potion

            Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();

            // Move it to the correct location
            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, potionAbove.transform.position.z);
            Debug.Log("I've found a potion when refilling the board and it was in the location: [" + x + "," + (y + yOffset) + "] we have moved it to the location: [" + x + "," + y + "]");

            //Move to location
            potionAbove.MoveToTarget(targetPos);

            // update incidices
            potionAbove.SetIndicies(x, y);
            // update our potionBoard
            potionBoard[x, y] = potionBoard[x, y + yOffset];

            // set the location the potion came from to null
            potionBoard[x, y + yOffset] = new Node(true, null);
        }

        // if we're hit the top of the board without finding a potion
        if (y + yOffset == height)
        {
            Debug.Log("I've reached the top of the board without finding a potion");
            SpawnPotionAtTop(x);
        }
    }

    // 현재 블럭을 새로 만들고 내림
    // TODO : 1. 미리 생성된 블럭이 내려오게 변경
    //        2. 모든 블럭이 일정한 속도로 내려오게(현재는 같은 시간에 한번에 내려오고 있음)
    private void SpawnPotionAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        int locationToMoveTo = height - index;
        Debug.Log("About to spawn a potion, ideally i'd like to put it in the index of : " + index);
        // get a random potion
        int randomIndex = Random.Range(0, potionPrefabs.Length);
        Vector2 position = new Vector2(x - spacingX, height - spacingY);

        GameObject newPotion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);

        // set indicies
        newPotion.GetComponent<Potion>().SetIndicies(x, index);
        // set it on the potion board
        potionBoard[x, index] = new Node(true, newPotion);
        // move it to that location
        Vector3 targetPosition = new Vector3(newPotion.transform.position.x, newPotion.transform.position.y - locationToMoveTo, newPotion.transform.position.z);
        newPotion.GetComponent<Potion>().MoveToTarget(targetPosition);
    }

    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;
        for (int y = height - 1; y >= 0; y--)
        {
            if (potionBoard[x, y].potion == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }

    #endregion

    // 가로 또는 세로 매칭이 일어났을 때 반대(가로이면 세로, 세로이면 가로 매칭이 일어났는지) 체크
    // 반대 방향도 매칭이 일어났을 경우 Super
    // TODO : 1. Super -> 족보 로직 세분화
    private MatchResult SuperMatch(MatchResult _matchedResults)
    {
        // if we have a horizontal or long horizontal match
        // loop through the potions in my match
        // create a new list of potions 'extra matches'
        // CheckDirection up
        // CheckDirection down
        // do we have 2 or more extra matches.
        // we've made a super match - return a new matchresult of type super
        // return extra matches

        if (_matchedResults.direction == MatchDirection.Horizontal || _matchedResults.direction == MatchDirection.LongHorizontal)
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();

                CheckDirection(pot, new Vector2Int(0, 1), extraConnectedPotions);
                CheckDirection(pot, new Vector2Int(0, -1), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    Debug.Log("I have a super Horizontal Match");
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);

                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }


        // if we have a vertical or long vertical match
        // loop through the potions in my match
        // create a new list of potions 'extra matches'
        // CheckDirection up
        // CheckDirection down
        // do we have 2 or more extra matches.
        // we've made a super match - return a new matchresult of type super
        // return extra matches
        if (_matchedResults.direction == MatchDirection.Vertical || _matchedResults.direction == MatchDirection.LongVertical)
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();

                CheckDirection(pot, new Vector2Int(1, 0), extraConnectedPotions);
                CheckDirection(pot, new Vector2Int(-1, 0), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    Debug.Log("I have a super Vertical Match");
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);

                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }
        return null;
    }

    // 블럭 타입이 일치하는지 확인 후 Match 결과 반환
    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new();

        connectedPotions.Add(potion);

        // check right
        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);

        // check left
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);


        // have we made a 3 match? (Horizontal match)
        if (connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal horizontal match, the color of my match is : " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }

        // checking for more than 3 (Long horizontal match)
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("I have a Long horizontal match, the color of my match is : " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };
        }

        // clear out the connectedpotions
        connectedPotions.Clear();

        // read our initial potion
        connectedPotions.Add(potion);

        // check up
        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);

        // check down
        CheckDirection(potion, new Vector2Int(0, -1), connectedPotions);


        // have we made a 3 match? (Vertical match)
        if (connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal Vertical match, the color of my match is : " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
            };
        }

        // checking for more than 3 (Long Vertical match)
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("I have a Long Vertical match, the color of my match is : " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongVertical
            };
        }
        else
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.None
            };
        }
    }


    // CheckDirection
    void CheckDirection(Potion pot, Vector2Int direction, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        // check that we're within the boundaries of the board
        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x, y].isUsable)
            {
                Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();

                // does our potionType Match? it must also not be matched
                if (!neighbourPotion.isMatched && neighbourPotion.potionType == potionType)
                {
                    connectedPotions.Add(neighbourPotion);

                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }

        }
    }

    #region Swapping Potions

    // 블럭 선택
    public void SelectPotion(Potion _potion)
    {
        // if we don't have a potion currently selected, then set the potion i just clicked to my selectedpotion
        if (selectedPotion == null)
        {
            Debug.Log(_potion);
            selectedPotion = _potion;
        }
        // if we select the same potion twice, then let's make selectedpotion null
        else if (selectedPotion == _potion)
        {
            selectedPotion = null;
        }

        // 블럭이 선택됐고 이후에 선택된 블럭이 이미 선택한 블럭이 아닌 경우
        // 선택된 블럭은 null
        else if (selectedPotion != _potion)
        {
            SwapPotion(selectedPotion, _potion);
            selectedPotion = null;
        }

    }

    // 블럭을 인접한 블럭과 위치 바꿈
    // TODO : 1. 벽에 부딫히는 경우 튕겨 돌아와야 함
    private void SwapPotion(Potion _currentPotion, Potion _targetPotion)
    {
        // 인접한 블럭을 클릭하지 않은 경우
        // 아무 일도 일어나지 않고 선택된 블럭 풀림
        if (!IsAdjacent(_currentPotion, _targetPotion))
        {
            return;
        }

        // 위치 바꾸기
        DoSwap(_currentPotion, _targetPotion);

        // 바꾼 다음에 매칭이 일어나고 블럭이 제거되는 동안 true
        isProcessingMove = true;

        // startCoroutine ProcessMatches.
        StartCoroutine(ProcessMatches(_currentPotion, _targetPotion));
    }
    
    // TODO : 1. 바꾸는 속도 조절
    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        GameObject temp = potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion;

        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion = potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion;
        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion = temp;

        // 위치 업데이트
        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;
        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;
        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;

        // 바꾸는 속도 조절
        _currentPotion.MoveToTarget(potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion.transform.position);
        _targetPotion.MoveToTarget(potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion.transform.position);

    }

    // 블럭 선택 후 인접한 블럭 선택했는지 체크
    private bool IsAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }

    private IEnumerator ProcessMatches(Potion _currentPotion, Potion _targetPotion)
    {
        yield return new WaitForSeconds(0.2f);

        if (CheckBoard())
        {
            // Start a coroutine that is going to process our matches in our turn.
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
        else
        {
            // 매칭이 일어나지 않은 경우 다시 스왑
            DoSwap(_currentPotion, _targetPotion);
        }

        isProcessingMove = false;
    }

    #endregion
}



public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}


// 족보 : 3배열, 4배열 직선, 4배열 네모, 5배열 직선, 5배열 L자 (시스템 기획서 27P)
//        
// 4배열 네모, 5배열 L자는 터지는 경우가 아님

// TODO : 1. 4배열 직선, 4배열 네모, 5배열 직선, 5배열 L자 족보 만들어야 함
//        2. Super 로직 변경하여 각각 로직 만들어야 함
//        3. 특수 블럭 로직

public enum MatchDirection
{
    Vertical, // 3 세로
    Horizontal, // 3 가로
    LongVertical, // 4 이상 세로
    LongHorizontal, // 4 이상 가로
    Super, // 가로 세로 합쳐서 작동중
    None
}
