using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public int normalBlockLength;
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

    //public bool isSwipeable;

    public bool isProcessMoving; // 동작 이후 블럭 제거 완료될때까지 true, 동작 중이지 않을때 false

    // 바구니 임시 세팅
    // TODO : 1. 바구니 컴포넌트 및 랜덤하게 세팅 해결 필요
    private int bag1SubtractCount = 0; // BlueBlock
    private int bag2SubtractCount = 0; // GreenBlock
    private int bag3SubtractCount = 0; // PinkBlock
    private int bag4SubtractCount = 0; // RedBlock

    // Unity 상에서 쉽게 특정 위치 안 나오게 
    public ArrayLayout arrayLayout;
    // static Instance
    public static PotionBoard Instance;

    // 매칭 로직 담당
    public FindMatches findMatches;

    // 제거 이펙트 애니메이션 풀
    private RemoveBlockEffectPool _RemoveBlockEffect = null;

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
            if (selectedPotion.currentSwipeable)
            {
                RunInput();
            }
            else
            {
                if (findMatches.IsSpecialBlock(selectedPotion.potionType))
                {
                    isProcessMoving = true;
                    StartCoroutine(ProcessOriginMatches(selectedPotion));
                }
            }
        }

        // 특수 블럭 조합 효과 테스트용
        if (Input.GetMouseButtonDown(1))
        {
            SelectPotion();
        }

        if (Input.GetMouseButtonUp(1) && selectedPotion)
        {
            StartCoroutine(TestProcessOriginSpecialMatches(selectedPotion));
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

        // 세팅 시 3개 이상 매칭이 있으면 초기화
        if (CheckInitializeBoard())
        {
            InitializeBoard();
        }
        // 매칭 없으면 세팅 끝나고 블럭 크기 조절
        else
        {
            // 블럭 크기 배경이랑 딱 맞게
            potionParent.transform.localScale = new Vector3(1.016f, 1.016f, 1);
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
            if (isProcessMoving)
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
    //        -> 240730 완료
    //        드래그 시점에서의 마우스 거리 계산해서 임계값 0.2f가 넘으면 selectPotion한테 currentSwipeable true로 줘서 
    //        드래그 많이 움직인 경우에만 스왑처리,
    private void RunInput()
    {
        float swipeAngle = selectedPotion.swipeAngle;
        selectedPotion.currentSwipeable = false;
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
    }

    // 블럭을 인접한 블럭과 위치 바꿈
    private void SwapPotion(Potion _currentPotion, Potion _targetPotion)
    {
        // 위치 바꾸기
        DoSwap(_currentPotion, _targetPotion);

        // 바꾼 다음에 매칭이 일어나고 블럭이 제거되는 동안 true
        isProcessMoving = true;

        // startCoroutine ProcessMatches.
        StartCoroutine(ProcessSwapMatches(_currentPotion, _targetPotion));
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

    private IEnumerator ProcessSwapMatches(Potion _currentPotion, Potion _targetPotion)
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
            isProcessMoving = false;
        }

        //isProcessingMove = false;
    }

    private IEnumerator ProcessOriginMatches(Potion _currentPotion)
    {
        yield return new WaitForSeconds(0.2f);

        // 매칭 체크 후 3매치 이상이 일어나면 제거 시작
        if (findMatches.FindSpecialMatches())
        {
            // Start a coroutine that is going to process our matches in our turn.
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
    }

    #endregion

    // 특수 블럭 효과 테스트용 메서드
    private IEnumerator TestProcessOriginSpecialMatches(Potion _currentPotion)
    {
        yield return new WaitForSeconds(0.2f);

        // 매칭 체크 후 3매치 이상이 일어나면 제거 시작
        if (findMatches.TestFindSpecialMatches())
        {
            // Start a coroutine that is going to process our matches in our turn.
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
    }

    #region 블럭 제거
    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
        // 클리어했을 때 에러나서 if문 삭제
        if (findMatches.potionsToRemove.Count > 0)
        {
            // TODO : 1. isProcessingMove 깔끔하게 리팩토링(마우스 뗐을 때 한번, 매칭됐을때 한번 체크하고 해제하고 하는중)
            //          -> 동작 시작 -> isProcessingMove 쭉 true -> 제거되고 생성되고 매칭 체크하고 완전히 동작이 끝났을 때 isProcessingMove false
            isProcessMoving = true;

            foreach (Potion potionToRemove in findMatches.potionsToRemove)
            {
                potionToRemove.isMatched = false;
            }

            RemoveBlock(findMatches.potionsToRemove);

            yield return new WaitForSeconds(0.6f);

            RefillBlock();

            // 현재 제거되는 블럭 당 1점으로 점수 카운트 됨
            GameManager.Instance.ProcessTurn(findMatches.potionsToRemove.Count, _subtractMoves, bag1SubtractCount, bag2SubtractCount, bag3SubtractCount, bag4SubtractCount);
            bag1SubtractCount = 0;
            bag2SubtractCount = 0;
            bag3SubtractCount = 0;
            bag4SubtractCount = 0;

            yield return new WaitForSeconds(0.6f);
        }

        isProcessMoving = false;

        if (findMatches.FindAllMatches())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }

    // 블럭 지워지고 다시 생성

    private void RemoveBlock(List<Potion> _potionsToRemove)
    {
        // Removing the potion and clearing the board at that location
        foreach (Potion potion in _potionsToRemove)
        {
            if (potion.potionType == PotionType.BlueBlock)
            {
                bag1SubtractCount++;
            }
            if (potion.potionType == PotionType.GreenBlock)
            {
                bag2SubtractCount++;
            }
            if (potion.potionType == PotionType.PinkBlock)
            {
                bag3SubtractCount++;
            }
            if (potion.potionType == PotionType.RedBlock)
            {
                bag4SubtractCount++;
            }
            // getting it's x and y indicies and storing them
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            // Destroy the potion
            Destroy(potion.gameObject);

            _RemoveBlockEffect = GameObject.Find("RemoveBlockEffectPool").GetComponent<RemoveBlockEffectPool>();

            if (_RemoveBlockEffect != null)
            {
                _RemoveBlockEffect.PlayEffect(potion.transform.position);
            }

            // Create a blank node on the potion board
            potionBoard[_xIndex, _yIndex] = new Node(true, null);
        }
    }

    private void RefillBlock()
    {
        int[] xIndexNullCounts = Enumerable.Repeat<int>(0, width).ToArray<int>();

        // 채우기 전에 x 인덱스마다 비어있는 개수 찾음
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].potion == null)
                {
                    xIndexNullCounts[x]++;
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 해당 보드의 블럭이 비어있으면
                if (potionBoard[x, y].potion == null)
                {
                    //Debug.Log("The location X: " + x + "Y: " + y + " is empty, attempting to refill it.");
                    // y offset 
                    // 비어있는 자리 위의 블럭을 지정
                    int yOffset = 1;

                    // while the cell above our current cell is null and we're below the height of the board
                    // 그 위의 블럭이 비어있으면 그 위를 지정
                    while (y + yOffset < height && potionBoard[x, y + yOffset].potion == null)
                    {
                        // increment y offset
                        //Debug.Log("The potion above me is null, but i'm not at the top of the board yet, so add to my yOffset and try again, Current Offset is : " + yOffset + " I'm about to add 1.");
                        yOffset++;
                    }

                    // we've either hit the top of the board or we found a potion
                    // 비어있는 블럭 위의 블럭을 찾았으면 빈 위치로 이동시킴
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
                    // 비어있는 자리 위 블럭이 없었다면 블럭 생성
                    if (y + yOffset == height)
                    {
                        //Debug.Log("I've reached the top of the board without finding a potion");
                        // 비어있는 자리 개수 값 넘겨줌
                        SpawnPotionAtTop(x, y, ref xIndexNullCounts);
                    }
                }
            }
        }
    }

    // 현재 블럭을 새로 만들고 내림
    // TODO : 1. 미리 생성된 블럭이 내려오게 변경
    //        2. 모든 블럭이 일정한 속도로 내려오게(현재는 같은 시간에 한번에 내려오고 있음)
    private void SpawnPotionAtTop(int _x, int _y, ref int[] _xIndexNullCounts)
    {
        int index = FindIndexOfLowestNull(_x); // TODO : _y로 대체가능한듯??

        // index 6              = y 7 nullcount 1
        // index 5 6            = y 7 8 nullcount 2
        // index 4 5 6          = y 7 8 9 nullcount 3
        // index 3 4 5 6        = y 7 8 9 10 nullcount 4
        // index 2 3 4 5 6      = y 7 8 9 10 11 nullcount 5
        // index 1 2 3 4 5 6    = y 7 8 9 10 11 12 nullcount 6
        // index 0 1 2 3 4 5 6  = y 7 8 9 10 11 12 13 nullcount 7
        int positionY = height;

        // 비어있는 자리 개수에 따라서 생성되는 y위치 값 설정
        // TODO : 1. 해당 부분 리팩토링
        switch (_xIndexNullCounts[_x])
        {
            case 1:
                break;
            case 2:
                if (index == 6)
                {
                    positionY = height + 1;
                }
                break;
            case 3:
                if (index == 5)
                {
                    positionY = height + 1;
                }
                else if (index == 6)
                {
                    positionY = height + 2;
                }
                break;
            case 4:

                if (index == 4)
                {
                    positionY = height + 1;
                }
                else if (index == 5)
                {
                    positionY = height + 2;
                }
                else if (index == 6)
                {
                    positionY = height + 3;
                }
                break;
            case 5:

                if (index == 3)
                {
                    positionY = height + 1;
                }
                else if (index == 4)
                {
                    positionY = height + 2;
                }
                else if (index == 5)
                {
                    positionY = height + 3;
                }
                else if (index == 6)
                {
                    positionY = height + 4;
                }
                break;
            case 6:

                if (index == 2)
                {
                    positionY = height + 1;
                }
                else if (index == 3)
                {
                    positionY = height + 2;
                }
                else if (index == 4)
                {
                    positionY = height + 3;
                }
                else if (index == 5)
                {
                    positionY = height + 4;
                }
                else if (index == 6)
                {
                    positionY = height + 5;
                }
                break;
            case 7:

                if (index == 1)
                {
                    positionY = height + 1;
                }
                else if (index == 2)
                {
                    positionY = height + 2;
                }
                else if (index == 3)
                {
                    positionY = height + 3;
                }
                else if (index == 4)
                {
                    positionY = height + 4;
                }
                else if (index == 5)
                {
                    positionY = height + 5;
                }
                else if (index == 6)
                {
                    positionY = height + 6;
                }
                break;
        }

        int locationToMoveTo = positionY - index;
        //Debug.Log("About to spawn a potion, ideally i'd like to put it in the index of : " + index);

        Vector2 position = new Vector2(_x - spacingX, positionY - spacingY);

        // TODO : 1. 특수블럭 생성되어야 할 경우 우선 생성
        //          -> 조합된 후 생성될 해당 블럭을 생성되는 특수 블럭으로 교체
        int makeBlockTypeIndex = MakeBlock();

        GameObject newPotion = Instantiate(potionPrefabs[makeBlockTypeIndex], position, Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);

        newPotion.GetComponent<Potion>().potionType = (PotionType)makeBlockTypeIndex;

        // set indicies
        newPotion.GetComponent<Potion>().SetIndicies(_x, index);

        // set it on the potion board
        potionBoard[_x, index] = new Node(true, newPotion);
        // move it to that location
        Vector3 targetPosition = new Vector3(newPotion.transform.position.x, newPotion.transform.position.y - locationToMoveTo, newPotion.transform.position.z);
        // 아래로 떨어지는 부분
        // TODO : 1. 채워야할 개수에 따라 속도가 다름 일정하게 필요
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

    #region 특수블럭 생성

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
            return MakePickRight();
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
        findMatches.isCheckedHorizontal_4 = false;
        return (int)PotionType.DrillHorizontal;
    }

    private int MakeDrillVertical()
    {
        findMatches.isCheckedVertical_4 = false;
        return (int)PotionType.DrillVertical;
    }

    private int MakePickLeft()
    {
        findMatches.isCheckedSquare = false;
        return (int)PotionType.PickLeft;
    }

    // 곡괭이 생성 위치는 논의
    private int MakePickRight()
    {
        findMatches.isCheckedSquare = false;
        return (int)PotionType.PickRight;
    }

    private int MakePrism()
    {
        findMatches.isCheckedMatched_5 = false;
        return (int)PotionType.Prism;
    }

    private int MakeBomb()
    {
        findMatches.isCheckedSuper = false;
        return (int)PotionType.Bomb;
    }

    #endregion
}

