using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class PotionBoard : MonoBehaviour
{
    // static Instance
    public static PotionBoard Instance;

    private StageManager _StageManager = null;

    // 가로 세로 블럭 개수 설정
    // 스테이지 따라 width, height 달라짐
    public int width;
    public int height;
    // X축, Y축 간격
    public float spacingX;
    public float spacingY;

    // 세팅

    // 일반 블럭 전체 목록
    public GameObject[] potionPrefabs;
    // 해당 스테이지에서 사용하는 일반 블럭 갯수
    public int stageNormalBlockLength;
    // 해당 스테이지 세팅되는 블럭 목록
    public GameObject[] stagePotionPrefabs;
    // 블럭 원래 위치
    // 블럭이 제거되고 새로 생성될 때 해당 위치 참조
    public GameObject potionParent;

    // 전체 바구니 목록
    public GameObject[] bagPrefabs;

    // 바구니 원래 위치
    public GameObject bagParent;

    // 보드 바구니들 정보 클래스
    public BoardBag boardBag;

    // 해당 스테이지 바구니 갯수
    public int stageBagLength;

    // 해당 스테이지 세팅 바구니 목록
    public GameObject[] stageBags;

    // 바구니 색깔 별 제거한 갯수 증가값
    public int[] bagsSubtractCounts;

    // 블럭보드
    public Node[,] potionBoard;
    public GameObject potionBoardGO;

    // 매칭되었을 때 제거할 블럭 목록
    // 매칭되는 블럭들을 추가후 제거하고 비우고 반복
    // 초기 세팅시에서밖에 사용 안하는중
    // 게임 중에는 FindMatches의 potionsToRemove 사용중
    public List<GameObject> potionsToDestroy = new();

    // unity 상에서 선택된 블럭 확인할 수 있게 SerializeField(직렬화) 사용
    // SerializeField : private이여도 unity에서 확인할 수 있음
    public Potion selectedPotion;
    public Potion targetedPotion;

    public bool isProcessMoving; // 동작 이후 블럭 제거 완료될때까지 true, 동작 중이지 않을때 false

    // Unity 상에서 쉽게 특정 위치 Inspeector UI로 확인 가능하도록 사용하는
    public ArrayLayout arrayLayout;

    // 매칭 로직 담당
    public FindMatches findMatches;

    // 제거 이펙트 애니메이션 풀
    private RemoveBlockEffectPool _RemoveBlockEffect = null;

    private void Awake()
    {
        Instance = this;
        findMatches = FindObjectOfType<FindMatches>();

        _StageManager = GameManager.GetManagerClass<StageManager>();
    }

    void Start()
    {
        // Setup
        _StageManager.MakeStageBoardSetupData();
        StageSetup();

        // 초기 보드 세팅
        InitializeBoard();
    }

    private void Update()
    {
        // Swapping Potions
        UpdateInputMouse();
    }

    #region Setup

    void StageSetup()
    {
        // 스테이지 매니저에서 세팅값 받아옴
        StageBoardData stageData = _StageManager.stageBoardData;
        width = stageData.mapWidth;
        height = stageData.mapHeight;
        stageNormalBlockLength = stageData.appearedBlockList.Length;
        stagePotionPrefabs = new GameObject[stageNormalBlockLength];

        for (int i = 0; i < stageNormalBlockLength; i++)
        {
            stagePotionPrefabs[i] = potionPrefabs[(int)stageData.appearedBlockList[i]];
        }

        // 목표 바구니 세팅
        GoalBag[] goalBags = stageData.goalBags;
        stageBagLength = goalBags.Length;
        stageBags = new GameObject[stageBagLength];
        bagsSubtractCounts = new int[stageBagLength];

        for (int i = 0; i < stageBagLength; i++)
        {
            // 바구니 생성될때 y축 100 위치로 생성됨
            Vector2 position = Vector2.zero;

            GameObject bag = Instantiate(bagPrefabs[(int)goalBags[i].targetBlock], position, Quaternion.identity);
            bag.transform.SetParent(bagParent.transform);
            bag.transform.localScale = new Vector3(1, 1, 1);

            // TODO : 1. 생성 갯수에 따른 위치 조정
            // 로컬포지션으로 이렇게 y축 -100 해야 최종적으로 0,0 되서 이렇게 사용
            // length == 3일 때 0, 1, 2 0 -> -140 2 -> 140
            // 일단 임시로 함
            if (stageBagLength == 3)
            {
                bag.transform.localPosition = new Vector2(-140 + i*140, -100);
            } 
            // length == 1일 때
            else
            {
                bag.transform.localPosition = new Vector2(0, -100);
            }

            bag.GetComponent<Bag>().SetGoalCount(goalBags[i].goalNumber);
            // stageBag 리스트에 생성된 bag 넣어서 나중에 bag check 목록으로 체크
            stageBags[i] = bag;
        }
    }

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
                    int randomIndex = Random.Range(0, stageNormalBlockLength);

                    GameObject potion = Instantiate(stagePotionPrefabs[randomIndex], position, Quaternion.identity);
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

                        MatchResult matchedPotions = findMatches.IsInitializeConnected(potion);

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

    // Update마다 Input 체크
    private void UpdateInputMouse()
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
                    //
                    selectedPotion.isChangedBlock = true;
                    //
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

        //
        _currentPotion.isChangedBlock = true;
        _targetPotion.isChangedBlock = true;
        //

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
            //
            _currentPotion.isChangedBlock = false;
            _targetPotion.isChangedBlock = false;
            //
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
            isProcessMoving = true;
            // 여기서 블럭들 제거되고 움직일 때 움직인 블럭들 체킹해야 됨

            //

            foreach (Potion potionToRemove in findMatches.potionsToRemove)
            {
                potionToRemove.isMatched = false;
            }

            RemoveBlock(findMatches.potionsToRemove);

            yield return new WaitForSeconds(0.6f);

            RefillBlock();

            // 현재 제거되는 블럭 당 1점으로 점수 카운트 됨
            PuzzleManager.Instance.ProcessTurn(findMatches.potionsToRemove.Count, _subtractMoves);

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
        foreach (Potion potion in _potionsToRemove)
        {
            for (int i=0; i<stageBagLength; i++)
            {
                Bag bag = stageBags[i].GetComponent<Bag>();
                if (potion.potionType == bag._PotionType)
                {
                    bag.UpdateCount();
                }
            }

            // getting it's x and y indicies and storing them
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            PotionType changedSpecialPotionType = potion.changedSpecialBlockType;

            // Destroy the potion
            Destroy(potion.gameObject);

            _RemoveBlockEffect = GameObject.Find("RemoveBlockEffectPool").GetComponent<RemoveBlockEffectPool>();

            // 애니메이션 끝까지 실행
            if (_RemoveBlockEffect != null)
            {
                _RemoveBlockEffect.PlayEffect(potion.transform.position);
            }

            // Create a blank node on the potion board
            potionBoard[_xIndex, _yIndex] = new Node(true, null);

            // 특수 블럭 생성되어야 하는 경우 생성
            if (changedSpecialPotionType != PotionType.None)
            {
                Vector2 position = new Vector2(_xIndex - spacingX, _yIndex - spacingY);
                GameObject newPotion = Instantiate(potionPrefabs[(int)changedSpecialPotionType], position, Quaternion.identity);

                newPotion.transform.SetParent(potionParent.transform);
                newPotion.name = "[" + _xIndex + ", " + _yIndex + "]" + newPotion.name;

                newPotion.GetComponent<Potion>().potionType = changedSpecialPotionType;

                // set indicies
                newPotion.GetComponent<Potion>().SetIndicies(_xIndex, _yIndex);

                // set it on the potion board
                potionBoard[_xIndex, _yIndex] = new Node(true, newPotion);
            }
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
    // TODO : 1. 일반 블럭만 해당 기능 사용
    //        2. 특수 블럭은 다른 기능으로 분리
    private void SpawnPotionAtTop(int _x, int _y, ref int[] _xIndexNullCounts)
    {
        int index = FindIndexOfLowestNull(_x);

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

        Vector2 position = new Vector2(_x - spacingX, positionY - spacingY);

        // TODO : 1. 특수블럭 생성되어야 할 경우 우선 생성
        //          -> 조합된 후 생성될 해당 블럭을 생성되는 특수 블럭으로 교체
        int makeBlockTypeIndex = MakeBlock();

        GameObject newPotion;
        // 특수 블럭 생성인 경우
        if(makeBlockTypeIndex >= 7)
        {
            newPotion = Instantiate(potionPrefabs[makeBlockTypeIndex], position, Quaternion.identity);
        } 
        // 일반 블럭 생성인 경우
        else
        {
            newPotion = Instantiate(stagePotionPrefabs[makeBlockTypeIndex], position, Quaternion.identity);
        }
        newPotion.transform.SetParent(potionParent.transform);
        newPotion.name = "[" + _x + ", " + (positionY) + "]" + newPotion.name;

        newPotion.GetComponent<Potion>().potionType = (PotionType)makeBlockTypeIndex;

        // set indicies
        newPotion.GetComponent<Potion>().SetIndicies(_x, index);

        // set it on the potion board
        potionBoard[_x, index] = new Node(true, newPotion);
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

    #region 특수블럭 생성

    // FindMatches가 가지고 있는 특수 블록 생성 여부에 따라서 해당 블럭 우선 생성
    // -> 특수 블럭은 따로 뺐음
    private int MakeBlock()
    {
        return Random.Range(0, stageNormalBlockLength);
    }

    #endregion
}

