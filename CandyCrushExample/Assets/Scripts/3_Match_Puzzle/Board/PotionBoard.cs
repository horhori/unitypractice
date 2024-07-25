using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PotionBoard : MonoBehaviour
{
    // 가로 세로 블럭 개수 설정
    // 스테이지 따라 width, height 달라짐
    public int width;
    public int height;
    // X축, Y축 간격
    public float spacingX;
    public float spacingY;
    // 일반 블럭 목록
    public GameObject[] potionPrefabs;
    // 일반 블럭 갯수
    public int normalBlockLength = 7;
    // 블럭보드
    public Node[,] potionBoard;
    public GameObject potionBoardGO;

    // 매칭되었을 때 제거할 블럭 목록
    // 매칭되는 블럭들을 추가후 제거하고 비우고 반복
    // 초기 세팅시에서밖에 사용 안하는중
    public List<GameObject> potionsToDestroy = new();

    // 블럭 원래 위치
    // 블럭이 제거되고 새로 생성될 때 해당 위치 참조
    public GameObject potionParent;

    // unity 상에서 선택된 블럭 확인할 수 있게 SerializeField(직렬화) 사용
    // SerializeField : private이여도 unity에서 확인할 수 있음
    public Potion selectedPotion;
    public Potion targetedPotion;

    [SerializeField]
    private bool isProcessingMove;

    //public GameObject drillHorizontalBlock;
    //public GameObject drillVerticalBlock;
    //public GameObject pickBlock;
    //public GameObject prismBlock;
    //public GameObject bombBlock;


    // 추가해야 할 곡괭이 수
    //public int pick;

    // Unity 상에서 쉽게 특정 위치 안 나오게 
    public ArrayLayout arrayLayout;
    // static Instance
    public static PotionBoard Instance;

    // 매칭 로직 담당
    public FindMatches findMatches;

    private void Awake()
    {
        Instance = this;
        findMatches = FindObjectOfType<FindMatches>();
    }

    void Start()
    {
        InitializeBoard();
        // 초기 생성 때 매칭 로직에서 나중에 할거랑 겹쳐서 임시로 처리
        findMatches.isCheckedVertical_4 = false;
        findMatches.isCheckedHorizontal_4 = false;
        findMatches.isCheckedSquare = false;
        findMatches.isCheckedSuper = false;
        findMatches.isCheckedMatched_5 = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectPotion();
        }

        if (Input.GetMouseButtonUp(0) && selectedPotion)
        {
            // 입력값(각도)을 계산하여 Run
            RunInput();
        }
    }

    #region Setup

    // 보드 생성
    // width, height 값에 따라 보드 판 생성.
    // 각 보드 자리마다 Node를 가지고 있음
    // Node는 사용가능한 자리인지(isUsable), 사용 가능하면 해당 자리에 기물(Potion->Block)을 가짐
    // 랜덤하게 생성 후 매치되는 경우에는 다시 생성됨
    // TODO : 1. 처음 보드 생성될 때 특정 갯수(7*14, 7*21...) 생성
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
                    int randomIndex = Random.Range(0, normalBlockLength);

                    GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
                    potion.transform.SetParent(potionParent.transform);
                    potion.transform.name = "[" + x + ", " + y + "]" + potion.name;
                    potion.GetComponent<Potion>().potionType = (PotionType)randomIndex;

                    potion.GetComponent<Potion>().SetIndicies(x, y);
                    potionBoard[x, y] = new Node(true, potion);
                    potionsToDestroy.Add(potion);
                }
            }
        }

        if (CheckInitializeBoard())
        {
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

    // InitailizeBoard()에서 사용하는 최초 매칭 확인하여 초기화하는 메서드
    public bool CheckInitializeBoard()
    {
        bool hasMatched = false;

        foreach (Node nodePotion in potionBoard)
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

                        // TODO : 1. 초기 생성 IsConnected랑 게임 도중이랑 분리 필요 -> 폭탄 때문에
                        MatchResult matchedPotions = findMatches.IsConnected(potion);

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            hasMatched = true;
                        }
                    }
                }
            }
        }

        return hasMatched;
    }

    #endregion

    #region Swapping Potions

    // Update에서 마우스 클릭 시 SelectPotion() 메서드로 선택한 블럭 저장
    private void SelectPotion()
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
            //Debug.Log("I have a clicked a potion it is : " + potion.gameObject);

            selectedPotion = potion;
        }
    }

    // Update에서 마우스 클릭 뗐을 때 각도 계산하여 해당 위치 블럭과 스왑 진행 후 선택한 블럭 해제
    // TODO : 1. 클릭만 했을 때(스와이프 X) Potion이 갖고 있는 swipeAngle을 어떻게 처리할지
    //          -> 처음에 swipeAngle이 0이라서 오른쪽으로 스와이프됨. 0인 경우 처리하기
    //      : 2. swipe 처리 후 swipeAngle 초기화 필요 -> 0으로 하고 0인 경우에는 클릭된 것으로 하는게 좋을 거 같긴 함
    //          -> 아니면 초기값을 45도(거의 조작이 안일어날 확률이 높은 값)로 하는게 좋을지?
    private void RunInput()
    {
        float swipeAngle = selectedPotion.swipeAngle;
        int originX = selectedPotion.xIndex;
        int originY = selectedPotion.yIndex;

        if (swipeAngle > -45 && swipeAngle <= 45 && originX != width - 1)
        {
            // Right Swipe
            targetedPotion = potionBoard[originX + 1, originY].potion.GetComponent<Potion>();
            SwapPotion(selectedPotion, targetedPotion);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && originY != height - 1)
        {
            // Up Swipe
            targetedPotion = potionBoard[originX, originY + 1].potion.GetComponent<Potion>();
            SwapPotion(selectedPotion, targetedPotion);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && originX != 0)
        {
            // Left Swipe
            targetedPotion = potionBoard[originX - 1, originY].potion.GetComponent<Potion>();
            SwapPotion(selectedPotion, targetedPotion);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && originY != 0)
        {
            // Down Swipe
            targetedPotion = potionBoard[originX, originY - 1].potion.GetComponent<Potion>();
            SwapPotion(selectedPotion, targetedPotion);
        }
        // 벽에 부딫히는 경우
        //else if (swipeAngle > -45 && swipeAngle <= 45 && originX == width - 1)
        //{
        //    // 오른쪽
        //    Vector3 tempDirection = new Vector3(1, 0);
        //    BounceEdge(selectedPotion, tempDirection);
        //}
        //else if (swipeAngle > 45 && swipeAngle <= 135 && originY == height - 1)
        //{
        //    // 위쪽
        //    Vector3 tempDirection = new Vector3(0, 1);
        //    BounceEdge(selectedPotion, tempDirection);
        //}
        //else if ((swipeAngle > 135 || swipeAngle <= -135) && originX == 0)
        //{
        //    // 왼쪽
        //    Vector3 tempDirection = new Vector3(-1, 0);
        //    BounceEdge(selectedPotion, tempDirection);
        //}
        //else if (swipeAngle < -45 && swipeAngle >= -135 && originY == 0)
        //{
        //    // 아래쪽
        //    Vector3 tempDirection = new Vector3(0, -1);
        //    BounceEdge(selectedPotion, tempDirection);
        //}

        //selectedPotion = null;
        //targetedPotion = null;

        //// 특수블럭 체크때문에 여기에서 특수블럭 체크 후 선택, 타켓 블럭 해제 넣음
        //board.selectedPotion = null;
        //board.targetedPotion = null;
    }

    // 벽에 부딫히는 경우
    // TODO : 1. 벽에 부딫히는 경우 튕겨 돌아와야 함
    //           -> 240718 부분 완료. 부딫혔을 때 속도, 거리 조절 필요
    //private void BounceEdge(Potion _currentPotion, Vector3 _targetDirection)
    //{
    //    Vector2 originPosition = _currentPotion.transform.position;
    //    Vector2 targetPosition = _currentPotion.transform.position + _targetDirection;

    //    isProcessingMove = true;

    //    _currentPotion.MoveToTarget(targetPosition);

    //    StartCoroutine(ProcessBounce(_currentPotion, originPosition));
    //}

    //private IEnumerator ProcessBounce(Potion _currentPotion, Vector2 _originPosition)
    //{
    //    yield return new WaitForSeconds(0.2f);

    //    _currentPotion.MoveToTarget(_originPosition);

    //    isProcessingMove = false;
    //}

    // 블럭을 인접한 블럭과 위치 바꿈
    private void SwapPotion(Potion _currentPotion, Potion _targetPotion)
    {
        // 위치 바꾸기
        DoSwap(_currentPotion, _targetPotion);

        // 바꾼 다음에 매칭이 일어나고 블럭이 제거되는 동안 true
        isProcessingMove = true;

        // TODO : 1. 선택한 블럭이 특수 블럭이면 매칭 체크 하면서 특수블럭 효과 발동

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

    private IEnumerator ProcessMatches(Potion _currentPotion, Potion _targetPotion)
    {
        yield return new WaitForSeconds(0.2f);

        // 매칭 체크 후 3매치 이상이 일어나면 제거 시작
        if (findMatches.FindAllMatches())
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

    #region Cascading Potions

    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
        foreach (Potion potionToRemove in findMatches.potionsToRemove)
        {
            potionToRemove.isMatched = false;
        }

        RemoveAndRefill(findMatches.potionsToRemove);

        // 현재 제거되는 블럭 당 1점으로 점수 카운트 됨
        GameManager.Instance.ProcessTurn(findMatches.potionsToRemove.Count, _subtractMoves);
        yield return new WaitForSeconds(0.4f);

        if (findMatches.FindAllMatches())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }

    // 블럭 지워지고 다시 생성

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
                if (potionBoard[x, y].potion == null)
                {
                    //Debug.Log("The location X: " + x + "Y: " + y + " is empty, attempting to refill it.");
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
        while (y + yOffset < height && potionBoard[x, y + yOffset].potion == null)
        {
            // increment y offset
            //Debug.Log("The potion above me is null, but i'm not at the top of the board yet, so add to my yOffset and try again, Current Offset is : " + yOffset + " I'm about to add 1.");
            yOffset++;
        }

        // we've either hit the top of the board or we found a potion

        if (y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
        {
            // we've found a potion

            Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();

            // Move it to the correct location
            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, potionAbove.transform.position.z);
            //Debug.Log("I've found a potion when refilling the board and it was in the location: [" + x + "," + (y + yOffset) + "] we have moved it to the location: [" + x + "," + y + "]");

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
            //Debug.Log("I've reached the top of the board without finding a potion");
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
        //Debug.Log("About to spawn a potion, ideally i'd like to put it in the index of : " + index);

        Vector2 position = new Vector2(x - spacingX, height - spacingY);

        // TODO : 1. 특수블럭 생성되어야 할 경우 우선 생성
        //          -> 
        int makeBlockTypeIndex = MakeBlock();

        GameObject newPotion = Instantiate(potionPrefabs[makeBlockTypeIndex], position, Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);

        newPotion.GetComponent<Potion>().potionType = (PotionType)makeBlockTypeIndex;

        // set indicies
        newPotion.GetComponent<Potion>().SetIndicies(x, index);
        // set it on the potion board
        potionBoard[x, index] = new Node(true, newPotion);
        // move it to that location
        Vector3 targetPosition = new Vector3(newPotion.transform.position.x, newPotion.transform.position.y - locationToMoveTo, newPotion.transform.position.z);
        // 아래로 떨어지는 부분
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
    
    // FindMatches가 가지고 있는 특수 블록 생성 여부에 따라서 해당 블럭 우선 생성
    private int MakeBlock()
    {
        // 특수 블럭인 경우
        if (findMatches.isCheckedHorizontal_4)
        {
            return MakeDrillHorizontal();
        }
        else if (findMatches.isCheckedVertical_4)
        {
            return MakeDrillVertical();
        }
        else if (findMatches.isCheckedSquare)
        {
            return MakePick();
        }
        else if (findMatches.isCheckedMatched_5)
        {
            return MakePrism();
        }
        else if (findMatches.isCheckedSuper)
        {
            return MakeBomb();
        }
        // 일반 블럭인 경우

        return Random.Range(0, normalBlockLength);
    }

    private int MakeDrillHorizontal()
    {
        Debug.Log("가로 드릴 생성");
        findMatches.isCheckedHorizontal_4 = false;
        return (int)PotionType.DrillHorizontal;
    }

    private int MakeDrillVertical()
    {
        Debug.Log("세로 드릴 생성");
        findMatches.isCheckedVertical_4 = false;
        return (int)PotionType.DrillVertical;
    }

    // 곡괭이 생성 위치는 논의
    private int MakePick()
    {
        Debug.Log("곡괭이 생성");
        findMatches.isCheckedSquare = false;
        return (int)PotionType.Pick;
    }

    private int MakePrism()
    {
        Debug.Log("프리즘 생성");
        findMatches.isCheckedMatched_5 = false;
        return (int)PotionType.Prism;
    }

    private int MakeBomb()
    {
        Debug.Log("폭탄 생성");
        findMatches.isCheckedSuper = false;
        return (int)PotionType.Bomb;
    }


}

public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}


// 족보 : 3배열, 4배열 직선, 4배열 네모, 5배열 직선, 5배열 L자 (시스템 기획서 27P)
//        
// 4배열 네모는 터지는 경우가 아님

// TODO : 1. 4배열 직선, 4배열 네모, 5배열 직선, 5배열 L자 족보 만들어야 함
//        2. 특수 블럭 로직 추가

public enum MatchDirection
{
    Vertical_3, // 3 세로
    Horizontal_3, // 3 가로
    Vertical_4, // 4 세로 : 드릴(세로)
    Horizontal_4, // 4 가로 : 드릴(가로)
    LongVertical, // 5 이상 세로 : 프리즘
    LongHorizontal, // 5 이상 가로 -> 프리즘
    Super, // 가로 세로 합쳐서 작동중 -> 5배열 L자 추가 로직 적용 : 폭탄
    Square, // 4배열 네모 : 곡괭이(대각), 곡괭이(역대각)
    None
}