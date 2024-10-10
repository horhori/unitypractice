using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FindMatches : MonoBehaviour
{
    private PotionBoard board;

    // 지워지는 블럭 여기에 저장해서 제거
    public List<Potion> potionsToRemove = new();

    void Awake()
    {
        board = FindObjectOfType<PotionBoard>();
    }

    private void Update()
    {

    }

    // 테스트용
    public bool TestFindSpecialMatches()
    {
        if (PuzzleManager.Instance.isStageEnded)
        {
            return false;
        }

        bool hasMatched = false;

        potionsToRemove.Clear();

        foreach (Node nodePotion in board.potionBoard)
        {
            if (nodePotion.potion != null)
            {
                nodePotion.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        Potion currentPotion = board.selectedPotion;
        //Potion targetPotion = board.targetedPotion;

        // 특수블럭 체크
        // 선택, 타켓된 포션이 있는 경우(최초로 스와이프 했을 때) 특수 블럭 체크 후 이 메서드 맨 아래에서 board가 가진 currentPotion, targetPotion 없앰
        if (currentPotion != null)
        {
            // test here
            potionsToRemove.AddRange(Get3DiagonalPieces(currentPotion.xIndex, currentPotion.yIndex));
            hasMatched = true;
        }

        // 특수블럭 체크때문에 여기에서 특수블럭 체크 후 선택, 타켓 블럭 해제 넣음
        // 최초의 경우에만 특수블럭 있는지 체크
        board.selectedPotion = null;

        return hasMatched;
    }

    // 제자리 클릭 시 특수블럭 효과 처리
    public bool FindSpecialMatches()
    {
        if (PuzzleManager.Instance.isStageEnded)
        {
            return false;
        }

        bool hasMatched = false;

        potionsToRemove.Clear();

        foreach (Node nodePotion in board.potionBoard)
        {
            if (nodePotion.potion != null)
            {
                nodePotion.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        Potion currentPotion = board.selectedPotion;

        // 특수블럭 체크
        // 선택, 타켓된 포션이 있는 경우(최초로 스와이프 했을 때) 특수 블럭 체크 후 이 메서드 맨 아래에서 board가 가진 currentPotion 없앰
        if (currentPotion != null)
        {
            if (IsSpecialBlock(currentPotion.potionType))
            {
                potionsToRemove.AddRange(RunSpecialBlock(currentPotion));
                hasMatched = true;
            }
        }

        // 특수블럭 체크때문에 여기에서 특수블럭 체크 후 선택, 타켓 블럭 해제 넣음
        // 최초의 경우에만 특수블럭 있는지 체크
        board.selectedPotion = null;

        return hasMatched;
    }

    // 보드에 매칭되어 있는게 있는지 체크
    // TODO : 1. 체크 시 매칭 경우의 수가 없는 경우 다시 섞여야 함
    //        2. 일정 시간이 지나고 조작이 없는 경우 매칭되는 블럭 표시
    public bool FindAllMatches()
    {
        if (PuzzleManager.Instance.isStageEnded)
        {
            return false;
        }

        bool hasMatched = false;

        potionsToRemove.Clear();

        foreach (Node nodePotion in board.potionBoard)
        {
            if (nodePotion.potion != null)
            {
                nodePotion.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        Potion currentPotion = board.selectedPotion;
        Potion targetPotion = board.targetedPotion;

        // 특수블럭 체크
        // 선택, 타켓된 포션이 있는 경우(최초로 스와이프 했을 때) 특수 블럭 체크 후 이 메서드 맨 아래에서 board가 가진 currentPotion, targetPotion 없앰
        if (currentPotion != null && targetPotion != null)
        {
            // 둘 다 특수블럭일 때 효과 조합
            if (IsSpecialBlock(currentPotion.potionType) && IsSpecialBlock(targetPotion.potionType))
            {
                List<Potion> blocks = RunCombineSpecialBlocks(currentPotion, targetPotion);
                if (blocks != null)
                {
                    potionsToRemove.AddRange(blocks);
                    hasMatched = true;
                }
            }

            // 선택된 블럭만 특수블럭인 경우
            if (IsSpecialBlock(currentPotion.potionType) && !IsSpecialBlock(targetPotion.potionType))
            {
                potionsToRemove.AddRange(RunSpecialBlock(currentPotion, targetPotion));
                hasMatched = true;
            }

            // 타겟 블럭만 특수블럭인 경우
            if (IsSpecialBlock(targetPotion.potionType) && !IsSpecialBlock(currentPotion.potionType))
            {
                potionsToRemove.AddRange(RunSpecialBlock(targetPotion, currentPotion));
                hasMatched = true;
            }
        }


        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                // checking if potion node is usable
                if (board.potionBoard[x, y].isUsable)
                {
                    // then proceed to get potion class in node.
                    Potion potion = board.potionBoard[x, y].potion.GetComponent<Potion>();

                    // ensure its not matched
                    if (!potion.isMatched)
                    {
                        // run some matching logic

                        MatchResult matchedPotions = IsConnected(potion);

                        // TODO : 1. 
                        //         -> IsConnected 매치 결과에 따라 SuperMatch 할지 말지 결정
                        //Vertical_3, // 3 세로
                        //Horizontal_3, // 3 가로
                        //Vertical_4, // 4 세로 : 드릴(세로)
                        //Horizontal_4, // 4 가로 : 드릴(가로)
                        //LongVertical, // 5 이상 세로 : 프리즘
                        //LongHorizontal, // 5 이상 가로 -> 프리즘
                        //Super, // 가로 세로 합쳐서 작동중 -> 5배열 L자 추가 로직 적용 : 폭탄
                        //Square, // 4배열 네모 : 곡괭이(대각), 곡괭이(역대각)
                        //None

                        if (matchedPotions.direction != MatchDirection.None)
                        {
                            // MatchDirection Vertical_3, Horizontal_3, Vertical_4, Horizontal_4 인 경우 SuperMatch인지 체크
                            MatchResult superMatchedPotions = FindSuperMatch(matchedPotions);

                            potionsToRemove.AddRange(superMatchedPotions.connectedPotions);

                            foreach (Potion pot in superMatchedPotions.connectedPotions)
                            {
                                pot.isMatched = true;

                                // MatchDirection에 따라서 바뀔 특수블럭 정보 Potion에 저장
                                if (pot.isChangedBlock)
                                {
                                    pot.SetChangedSpecialBlockType(superMatchedPotions.direction);

                                    // 두개 이상 블럭 움직였을 때 중복 특수블럭 생성 방지용 isChangedBlock false로 만듬
                                    foreach (Potion pot2 in superMatchedPotions.connectedPotions)
                                    {
                                        pot2.isChangedBlock = false;
                                    }
                                }
                            }

                            hasMatched = true;
                        }
                    }

                    // 매칭 체크 후 블럭 제거로 인해 체크한 isChangedBlock false로 만듬
                    potion.isChangedBlock = false;
                }
            }
        }

        // 특수블럭 체크때문에 여기에서 특수블럭 체크 후 선택, 타켓 블럭 해제 넣음
        // 최초의 경우에만 특수블럭 있는지 체크
        board.selectedPotion = null;
        board.targetedPotion = null;

        return hasMatched;
    }

    // 가로 또는 세로 매칭이 일어났을 때 반대(가로이면 세로, 세로이면 가로 매칭이 일어났는지) 체크
    // 반대 방향도 매칭이 일어났을 경우 Super

    private MatchResult FindSuperMatch(MatchResult _matchedResults)
    {
        // TODO : 1. 족보 우선순위 적용
        if (_matchedResults.direction == MatchDirection.Horizontal_3 || _matchedResults.direction == MatchDirection.Horizontal_4 || _matchedResults.direction == MatchDirection.LongHorizontal)
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();

                CheckSuperDirection(pot, new Vector2Int(0, 1), extraConnectedPotions);
                CheckSuperDirection(pot, new Vector2Int(0, -1), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    //isCheckedSuper = true;
                    // 폭탄 생성

                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);

                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = _matchedResults.direction == MatchDirection.LongHorizontal ? MatchDirection.LongHorizontal : MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }

        if (_matchedResults.direction == MatchDirection.Vertical_3 || _matchedResults.direction == MatchDirection.Vertical_4 || _matchedResults.direction == MatchDirection.LongVertical)
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();

                CheckSuperDirection(pot, new Vector2Int(1, 0), extraConnectedPotions);
                CheckSuperDirection(pot, new Vector2Int(-1, 0), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    //isCheckedSuper = true;
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);

                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = _matchedResults.direction == MatchDirection.LongVertical ? MatchDirection.LongVertical : MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }

        // _matchedResult.direction == MatchDirection.Square인 경우 그대로 넘김
        return new MatchResult
        {
            connectedPotions = _matchedResults.connectedPotions,
            direction = _matchedResults.direction
        };
    }

    // 블럭 타입이 일치하는지 확인 후 Match 결과 반환
    // 기존 IsConnected에서는 특수 블럭 생성때문에 각 경우를 체크해서 반환했는데
    // 3개 이상 있는 경우 보드 초기화되므로 간단하게 처리하도록 따로 함수 만들었음
    public MatchResult IsInitializeConnected(Potion potion)
    {
        List<Potion> connectedPotions = new()
        {
            potion
        };

        // 가로 체크
        CheckHorizontalMatch(potion, connectedPotions);

        if (connectedPotions.Count >= 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal_3
            };
        }

        connectedPotions.Clear();

        connectedPotions.Add(potion);

        // 세로 체크
        CheckVerticalMatch(potion, connectedPotions);

        if (connectedPotions.Count >= 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical_3
            };
        }

        connectedPotions.Clear();

        connectedPotions.Add(potion);

        // 4 이상 네모 : 곡괭이 체크
        CheckSquareMatch(potion, connectedPotions);

        if (connectedPotions.Count >= 4)
        {
            //isCheckedSquare = true;

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Square
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

    // 블럭 타입이 일치하는지 확인 후 Match 결과 반환
    public MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new()
        {
            potion
        };

        // 우선 순위 적용 :
        // 1. 5개 이상 가로, 세로(프리즘)
        // 2. L자, T자(폭탄) -> 이 메서드 벗어나서 체크
        // 3. 4개 이상 가로, 세로(드릴)
        // 4. 네모(곡괭이) -> 2개 이상 가로인 경우 체크
        // 5. 3개 이상 가로, 세로

        // 가로 체크
        CheckHorizontalMatch(potion, connectedPotions);

        // 1. 5 이상 가로 매치 : 프리즘
        if (connectedPotions.Count >= 5)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };
        }

        // 3. 4 가로 매치 : 드릴 가로

        else if (connectedPotions.Count == 4)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal_4
            };
        }

        // 4. 네모

        else if (connectedPotions.Count >= 2)
        {
            List<Potion> newConnectedPotions = new()
            {
                potion
            };

            // 4 이상 네모 : 곡괭이
            CheckSquareMatch(potion, newConnectedPotions);

            if (newConnectedPotions.Count >= 4)
            {
                return new MatchResult
                {
                    connectedPotions = newConnectedPotions,
                    direction = MatchDirection.Square
                };
            } 

            // 5. 3 가로 매치
            if (connectedPotions.Count == 3)
            {
                // XOO
                // OOO 인 경우 아래만 사라짐 -> 예외처리

                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.Horizontal_3
                };
            }
        }

        // 5. 3 가로 매치
        //else if (connectedPotions.Count == 3)
        //{
        //    // 3줄일때 곡괭이 체크 따로 해야함
        //    return new MatchResult
        //    {
        //        connectedPotions = connectedPotions,
        //        direction = MatchDirection.Horizontal_3
        //    };

        //}

        connectedPotions.Clear();

        connectedPotions.Add(potion);

        // 세로 체크
        CheckVerticalMatch(potion, connectedPotions);

        // 우선 순위 적용
        // 1. 5 이상 세로 매치 : 프리즘
        if (connectedPotions.Count >= 5)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongVertical
            };
        }
        // 2. 4 세로 매치 : 드릴 세로
        else if (connectedPotions.Count == 4)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical_4
            };
        }
        // 4. 네모 체크
        else if (connectedPotions.Count >= 2)
        {
            List<Potion> newConnectedPotions = new()
            {
                potion
            };

            // 4 이상 네모 : 곡괭이
            CheckSquareMatch(potion, newConnectedPotions);

            if (newConnectedPotions.Count >= 4)
            {
                return new MatchResult
                {
                    connectedPotions = newConnectedPotions,
                    direction = MatchDirection.Square
                };
            }

            // 5. 3 세로 매치
            if (connectedPotions.Count == 3)
            {
                // OO
                // OO
                // OX인 경우 왼쪽만 사라짐 -> 예외처리
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.Vertical_3
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

        // 5. 3 세로 매치
        //if (connectedPotions.Count == 3)
        //{
        //    return new MatchResult
        //    {
        //        connectedPotions = connectedPotions,
        //        direction = MatchDirection.Vertical_3
        //    };
        //}

        //// 네모 체크
        //connectedPotions.Clear();

        //connectedPotions.Add(potion);

        //CheckSquareMatch(potion, connectedPotions);

        //if (connectedPotions.Count >= 4)
        //{
        //    return new MatchResult
        //    {
        //        connectedPotions = connectedPotions,
        //        direction = MatchDirection.Square
        //    };
        //}

        else
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.None
            };
        }
    }

    void CheckHorizontalMatch(Potion pot, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + 1;
        int y = pot.yIndex;

        // check that we're within the boundaries of the board
        while (x >= 0 && x < board.width)
        {
            if (IsUsableAndNotBeforeMatched(x, y))
            {
                Potion neighbourPotion = board.potionBoard[x, y].potion.GetComponent<Potion>();

                // does our potionType Match? it must also not be matched
                if (neighbourPotion.potionType == potionType)
                {
                    connectedPotions.Add(neighbourPotion);
                    //neighbourPotion.isMatched = true;

                    x += 1;
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

    void CheckVerticalMatch(Potion pot, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex;
        int y = pot.yIndex + 1;

        // check that we're within the boundaries of the board
        while (y >= 0 && y < board.height)
        {
            if (IsUsableAndNotBeforeMatched(x, y))
            {
                Potion neighbourPotion = board.potionBoard[x, y].potion.GetComponent<Potion>();

                // does our potionType Match? it must also not be matched
                if (neighbourPotion.potionType == potionType)
                {
                    connectedPotions.Add(neighbourPotion);
                    //neighbourPotion.isMatched = true;

                    y += 1;
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

    // TODO : 1. XOO
    //           OOO 블록같은 경우 왼쪽 column부터 체크를 하기 때문에 3매치 직선만 처리되는중..
    void CheckSquareMatch(Potion pot, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex;
        int y = pot.yIndex;

        if (x < board.width - 1 && y < board.height - 1 && IsUsableAndNotBeforeMatched(x + 1, y))
        {
            Potion rightneighbourPotion = board.potionBoard[x + 1, y].potion.GetComponent<Potion>();

            if (!rightneighbourPotion.isMatched && rightneighbourPotion.potionType == potionType)
            {
                // 위, 오른족 위 블럭 같은지 체크
                if (IsUsableAndNotBeforeMatched(x, y + 1) && IsUsableAndNotBeforeMatched(x + 1, y + 1))
                {
                    Potion upNeighbourPotion = board.potionBoard[x, y + 1].potion.GetComponent<Potion>();
                    Potion rightupNeighbourPotion = board.potionBoard[x + 1, y + 1].potion.GetComponent<Potion>();

                    if (rightneighbourPotion.potionType == upNeighbourPotion.potionType && rightneighbourPotion.potionType == rightupNeighbourPotion.potionType)
                    {
                        connectedPotions.Add(rightneighbourPotion);
                        connectedPotions.Add(upNeighbourPotion);
                        connectedPotions.Add(rightupNeighbourPotion);
                        //rightneighbourPotion.isMatched = true;
                        //upNeighbourPotion.isMatched = true;
                        //rightupNeighbourPotion.isMatched = true;

                        // 사각형을 발견하면 추가 연결 체크 = 추가 연결은 OtherPotion으로 일단 명명했음

                        // 아래 왼쪽
                        if (y > 0 && IsUsableAndNotBeforeMatched(x, y - 1) && board.potionBoard[x, y - 1].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion leftdownOtherPotion = board.potionBoard[x, y - 1].potion.GetComponent<Potion>();

                            connectedPotions.Add(leftdownOtherPotion);
                            //leftdownOtherPotion.isMatched = true;
                        }

                        // 아래 오른쪽
                        if (y > 0 && IsUsableAndNotBeforeMatched(x + 1, y - 1) && board.potionBoard[x + 1, y - 1].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion rightdownOtherPotion = board.potionBoard[x + 1, y - 1].potion.GetComponent<Potion>();

                            connectedPotions.Add(rightdownOtherPotion);
                            //rightdownOtherPotion.isMatched = true;
                        }

                        // 왼쪽
                        if (x > 0 && IsUsableAndNotBeforeMatched(x - 1, y) && board.potionBoard[x - 1, y].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion leftOtherPotion = board.potionBoard[x - 1, y].potion.GetComponent<Potion>();

                            connectedPotions.Add(leftOtherPotion);
                            //leftOtherPotion.isMatched = true;
                        }

                        // 왼쪽 위
                        if (x > 0 && IsUsableAndNotBeforeMatched(x - 1, y + 1) && board.potionBoard[x - 1, y + 1].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion leftupOtherPotion = board.potionBoard[x - 1, y + 1].potion.GetComponent<Potion>();

                            connectedPotions.Add(leftupOtherPotion);
                            //leftupOtherPotion.isMatched = true;
                        }

                        // 먼 위 왼쪽
                        if (y < board.height - 2 && IsUsableAndNotBeforeMatched(x, y + 2) && board.potionBoard[x, y + 2].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion farupleftOtherPotion = board.potionBoard[x, y + 2].potion.GetComponent<Potion>();

                            connectedPotions.Add(farupleftOtherPotion);
                            //farupleftOtherPotion.isMatched = true;
                        }

                        // 먼 위 오른쪽
                        if (y < board.height - 2 && IsUsableAndNotBeforeMatched(x + 1, y + 2) && board.potionBoard[x + 1, y + 2].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion faruprightOtherPotion = board.potionBoard[x + 1, y + 2].potion.GetComponent<Potion>();

                            connectedPotions.Add(faruprightOtherPotion);
                            //faruprightOtherPotion.isMatched = true;
                        }

                        // 먼 오른쪽
                        if (x < board.width - 2 && IsUsableAndNotBeforeMatched(x + 2, y) && board.potionBoard[x + 2, y].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion farrightOtherPotion = board.potionBoard[x + 2, y].potion.GetComponent<Potion>();

                            connectedPotions.Add(farrightOtherPotion);
                            //farrightOtherPotion.isMatched = true;
                        }

                        // 먼 오른쪽 위
                        if (x < board.width - 2 && IsUsableAndNotBeforeMatched(x + 2, y + 1) && board.potionBoard[x + 2, y + 1].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion farrightupOtherPotion = board.potionBoard[x + 2, y + 1].potion.GetComponent<Potion>();

                            connectedPotions.Add(farrightupOtherPotion);
                            //farrightupOtherPotion.isMatched = true;
                        }

                    }
                }
            }
        } 
    }

    void CheckLine3SquareMatch()
    {

    }

    // 세로, 가로 방향(direction) Super 매칭(L자)되나 체크
    // 매개 변수로 준 direction 방향으로 같으면 계속 체크함
    void CheckSuperDirection(Potion pot, Vector2Int direction, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        // check that we're within the boundaries of the board
        while (x >= 0 && x < board.width && y >= 0 && y < board.height)
        {
            if (IsUsableAndNotBeforeMatched(x, y))
            {
                Potion neighbourPotion = board.potionBoard[x, y].potion.GetComponent<Potion>();

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

    // 제자리 특수 블럭 기능
    public List<Potion> RunSpecialBlock(Potion _potion)
    {
        switch (_potion.potionType)
        {
            case PotionType.DrillVertical:
                return GetColumnPieces(_potion.xIndex);

            case PotionType.DrillHorizontal:
                return GetRowPieces(_potion.yIndex);

            case PotionType.Bomb:
                return Get2DistancePieces(_potion.xIndex, _potion.yIndex);

            case PotionType.Prism:
                return MatchPiecesOfColor(_potion.xIndex, _potion.yIndex);

            case PotionType.PickLeft:
                return GetReverseDiagonalPieces(_potion.xIndex, _potion.yIndex);

            case PotionType.PickRight:
                return GetDiagonalPieces(_potion.xIndex, _potion.yIndex);
        }

        return null;
    }

    // 스와이프 특수 블럭 기능(프리즘때문에 첫번째 매개변수가 selectedPotion일수도 있고 targetPotion일수도 있음)
    public List<Potion> RunSpecialBlock(Potion _potion, Potion _anotherPotion)
    {
        switch (_potion.potionType)
        {
            case PotionType.DrillVertical :
                return GetColumnPieces(_potion.xIndex);

            case PotionType.DrillHorizontal:
                return GetRowPieces(_potion.yIndex);

            case PotionType.Bomb:
                return Get2DistancePieces(_potion.xIndex, _potion.yIndex);

            case PotionType.Prism:
                return MatchPiecesOfColor(_potion.xIndex, _potion.yIndex, _anotherPotion.potionType);

            case PotionType.PickLeft:
                return GetReverseDiagonalPieces(_potion.xIndex, _potion.yIndex);

            case PotionType.PickRight:
                return GetDiagonalPieces(_potion.xIndex, _potion.yIndex);
        }

        return null;
    }

    // currentPotion, targetPotion 둘다 특수 블럭일 때 발동
    public List<Potion> RunCombineSpecialBlocks(Potion _selectedPotion, Potion _targetedPotion)
    {
        switch (_selectedPotion.potionType)
        {
            // 드릴(세로) 기능
            // 조합 효과
            // 1. + 드릴(가로, 세로 상관없음) : 조합된 부분에서 십자 방향에 있는 모든 블럭 제거
            // 2. + 곡괭이 : 조합된 곡괭이 블럭 모양에 맞춘 방향의 모든 블럭 제거(기획서 P63 참고)
            // 3. + 프리즘 : 필드에 존재하는 블럭 중 가장 많은 수의 블럭을 조합된 드릴로 바꿈
            case PotionType.DrillVertical:
                switch (_targetedPotion.potionType)
                {
                    case PotionType.DrillVertical:
                    case PotionType.DrillHorizontal:
                        return GetColumnAndRowPieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                    case PotionType.PickLeft:
                        return GetRowAndDiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex, _targetedPotion.xIndex, _targetedPotion.yIndex);
                    case PotionType.PickRight:
                        return GetRowAndReverseDiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex, _targetedPotion.xIndex, _targetedPotion.yIndex);
                    case PotionType.Prism:
                        SwitchDrillVertical();
                        return GetCurrentPieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                }

                // 이 경우가 아닐 때 처리 -> 안되면 if문
                return null;

            // 드릴(가로) 기능
            // 조합 효과
            // 1. + 드릴(가로, 세로 상관없음) : 조합된 부분에서 십자 방향에 있는 모든 블럭 제거
            // 2. + 곡괭이 : 조합된 곡괭이 블럭 모양에 맞춘 방향의 모든 블럭 제거(기획서 P63 참고)
            // 3. + 프리즘 : 필드에 존재하는 블럭 중 가장 많은 수의 블럭을 조합된 드릴로 바꿈
            case PotionType.DrillHorizontal:
                switch (_targetedPotion.potionType)
                {
                    case PotionType.DrillVertical:
                    case PotionType.DrillHorizontal:
                        return GetColumnAndRowPieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                    case PotionType.PickLeft:
                        return GetColumnAndDiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex, _targetedPotion.xIndex, _targetedPotion.yIndex);
                    case PotionType.PickRight:
                        return GetColumnAndReverseDiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex, _targetedPotion.xIndex, _targetedPotion.yIndex);
                    case PotionType.Prism:
                        SwitchDrillHorizontal();
                        return GetCurrentPieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                }

                // 이 경우가 아닐 때 처리 -> 안되면 if문
                return null;

            // 폭탄 기능
            // 조합 효과
            // 1. + 폭탄 : 조작하여 조합된 부분에서 3칸 떨어진 모든 블럭 제거
            // 2. + 드릴 : 드릴 블럭과 같은 가로 or 세로줄에 있는 모든 블럭을 3칸 너비로 제거
            // 3. + 곡괭이 : 곡괭이 블럭과 같은 대각 or 역대각줄에 있는 모든 블럭을 3칸 너비로 제거
            // 4. + 프리즘 : 필드에 존재하는 블럭 중 가장 많은 수의 블럭을 폭탄으로 바꿈
            //               바뀐 폭탄 효과는 왼쪽 위부터 발생
            case PotionType.Bomb:
                switch (_targetedPotion.potionType)
                {
                    case PotionType.Bomb:
                        return Get3DistancePieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                    case PotionType.DrillVertical:
                        return Get3ColumnPieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                    case PotionType.DrillHorizontal:
                        return Get3RowPieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                    case PotionType.PickLeft:
                        return Get3DiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                    case PotionType.PickRight:
                        // 에러남
                        return Get3ReverseDiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                    case PotionType.Prism:
                        SwitchBomb();
                        return GetCurrentPieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                }

                // 이 경우가 아닐 때 처리 -> 안되면 if문
                return null;
            // 조합 효과
            // 1. + 프리즘 : 화면에 보이는 모든 블럭을 제거
            case PotionType.Prism:
                switch (_targetedPotion.potionType)
                {
                    case PotionType.Prism:
                        return GetAllPieces();
                }

                // 이 경우가 아닐 때 처리 -> 안되면 if문
                return null;
            // 곡괭이 역대각(왼쪽) 기능
            // 조합 효과
            // 1. + 드릴 : 조합된 드릴 블럭 모양에 맞춘 방향의 모든 블럭 제거(기획서 P63 참고)
            // 2. + 곡괭이(대각, 역대각 상관없음) : X 방향 블럭 제거
            // 3. + 프리즘 : 필드에 존재하는 블럭 중 가장 많은 수의 블럭을 조합된 곡괭이로 바꿈
            case PotionType.PickLeft:
                switch (_targetedPotion.potionType)
                {
                    case PotionType.DrillHorizontal:
                        return GetRowAndReverseDiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex, _targetedPotion.xIndex, _targetedPotion.yIndex);
                    case PotionType.DrillVertical:
                        return GetColumnAndReverseDiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex, _targetedPotion.xIndex, _targetedPotion.yIndex);
                    case PotionType.PickLeft:
                    case PotionType.PickRight:
                        return GetDiagonalAndReverseDiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex, _targetedPotion.xIndex, _targetedPotion.yIndex);
                    case PotionType.Prism:
                        SwitchPickLeft();
                        return GetCurrentPieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                }

                // 이 경우가 아닐 때 처리 -> 안되면 if문
                return null;
            // 곡괭이 대각(오른쪽) 기능
            // 조합 효과
            // 1. + 드릴 : 조합된 드릴 블럭 모양에 맞춘 방향의 모든 블럭 제거(기획서 P63 참고)
            // 2. + 곡괭이(대각, 역대각 상관없음) : X 방향 블럭 제거
            // 3. + 프리즘 : 필드에 존재하는 블럭 중 가장 많은 수의 블럭을 조합된 곡괭이로 바꿈
            case PotionType.PickRight:
                switch (_targetedPotion.potionType)
                {
                    case PotionType.DrillHorizontal:
                        return GetRowAndDiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex, _targetedPotion.xIndex, _targetedPotion.yIndex);
                    case PotionType.DrillVertical:
                        return GetColumnAndDiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex, _targetedPotion.xIndex, _targetedPotion.yIndex);
                    case PotionType.PickLeft:
                    case PotionType.PickRight:
                        return GetDiagonalAndReverseDiagonalPieces(_selectedPotion.xIndex, _selectedPotion.yIndex, _targetedPotion.xIndex, _targetedPotion.yIndex);
                    case PotionType.Prism:
                        SwitchPickRight();
                        return GetCurrentPieces(_selectedPotion.xIndex, _selectedPotion.yIndex);
                }

                // 이 경우가 아닐 때 처리 -> 안되면 if문
                return null;
        }

        return null;
    }

    // 드릴(세로) 기능
    List<Potion> GetColumnPieces(int _xIndex)
    {
        List<Potion> blocks = new List<Potion>();

        for (int i = 0; i < board.height; i++)
        {
            if (IsUsableAndNotBeforeMatched(_xIndex, i))
            {
                CheckSpecialChainMatch(_xIndex, i, blocks);
            }
        }

        return blocks;
    }


    // 드릴(가로) 기능
    List<Potion> GetRowPieces(int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();
        
        for (int i=0; i<board.width; i++)
        {
            if (IsUsableAndNotBeforeMatched(i, _yIndex))
            {
                CheckSpecialChainMatch(i, _yIndex, blocks);
            }
        }

        return blocks;
    }

    // 폭탄 기능
    List<Potion> Get2DistancePieces(int _xIndex, int  _yIndex)
    {
        List<Potion> blocks = new List<Potion>();

        // 3x3 가운데 dot 기준으로 왼쪽 아래 점부터 오른쪽 위 점까지만 루프
        for (int i = _xIndex - 1; i <= _xIndex + 1; i++)
        {
            for (int j = _yIndex - 1; j <= _yIndex + 1; j++)
            {

                // Check if the piece is inside the board 모서리 체크
                if (i >= 0 && i < board.width && j >= 0 && j < board.height && IsUsableAndNotBeforeMatched(i, j))
                {
                    CheckSpecialChainMatch(i, j, blocks);
                }
                
            }
        }

        if (_xIndex >= 2 && board.potionBoard[_xIndex - 2, _yIndex] != null && IsUsableAndNotBeforeMatched(_xIndex - 2, _yIndex))
        {
            CheckSpecialChainMatch(_xIndex - 2, _yIndex, blocks);
        }
        if (_xIndex < board.width - 2 && board.potionBoard[_xIndex + 2, _yIndex] != null && IsUsableAndNotBeforeMatched(_xIndex + 2, _yIndex))
        {
            CheckSpecialChainMatch(_xIndex + 2, _yIndex, blocks);
        }
        if (_yIndex >= 2 && board.potionBoard[_xIndex, _yIndex - 2] != null && IsUsableAndNotBeforeMatched(_xIndex, _yIndex - 2))
        {
            CheckSpecialChainMatch(_xIndex, _yIndex - 2, blocks);
        }
        if (_yIndex < board.height - 2 && board.potionBoard[_xIndex, _yIndex + 2] != null && IsUsableAndNotBeforeMatched(_xIndex, _yIndex + 2))
        {
            CheckSpecialChainMatch(_xIndex, _yIndex + 2, blocks);
        }

        return blocks;
    }

    // 곡괭이 대각(오른쪽) 기능
    List<Potion> GetDiagonalPieces(int _xIndex, int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();

        int index = 0;

        while (_xIndex - index >= 0 && _yIndex - index >= 0)
        {
            if (_xIndex - index < board.width && IsUsableAndNotBeforeMatched(_xIndex - index, _yIndex - index))
            {
                CheckSpecialChainMatch(_xIndex - index, _yIndex - index, blocks);
            }
            index++;
        }

        index = 0;

        while (_xIndex + index < board.width && _yIndex + index < board.height)
        {
            if (_xIndex + index >= 0 && IsUsableAndNotBeforeMatched(_xIndex + index, _yIndex + index))
            {
                CheckSpecialChainMatch(_xIndex + index, _yIndex + index, blocks);
            }
            index++;
        }

        return blocks;
    }

    // 곡괭이 역대각(왼쪽) 기능
    List<Potion> GetReverseDiagonalPieces(int _xIndex, int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();

        int index = 0;

        while (_xIndex - index >= 0 && _yIndex + index < board.height)
        {
            if (_xIndex - index < board.width && IsUsableAndNotBeforeMatched(_xIndex - index, _yIndex + index))
            {
                CheckSpecialChainMatch(_xIndex - index, _yIndex + index, blocks);
            }
            index++;
        }

        index = 0;

        while (_xIndex + index < board.width && _yIndex - index >= 0)
        {
            if (_xIndex + index >= 0 && IsUsableAndNotBeforeMatched(_xIndex + index, _yIndex - index))
            {
                CheckSpecialChainMatch(_xIndex + index, _yIndex - index, blocks);
            }
            index++;
        }

        return blocks;
    }

    // 제자리 프리즘 기능
    List<Potion> MatchPiecesOfColor(int _xIndex, int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();

        // Enumerable.Repeat<int>(0, board.normalBlockLength).ToArray<int>()
        // 선언하면서 0으로 length만큼 초기화
        // (초기화하고 싶은 값, 길이)
        int[] blockCounts = Enumerable.Repeat<int>(0, board.stageNormalBlockLength).ToArray<int>();

        // 가장 많은 색상 찾아주는 
        for (int i=0; i<board.width; i++)
        {
            for (int j=0; j<board.height; j++)
            {
                if (board.potionBoard[i, j] != null && IsUsableAndNotBeforeMatched(i, j))
                {
                    if (!IsSpecialBlock(board.potionBoard[i, j].potion.GetComponent<Potion>().potionType))
                    blockCounts[(int)board.potionBoard[i, j].potion.GetComponent<Potion>().potionType]++;
                }

            }
        }

        // 가장 많은 색상 potionType 찾음
        int maxIndex = 0;
        int max = 0;

        for (int i=0; i<blockCounts.Length; i++)
        {
            if (max < blockCounts[i])
            {
                max = blockCounts[i];
                maxIndex = i;
            }
        }

        PotionType _targetPotionType = (PotionType)maxIndex;

        // Prism 블럭 추가
        if (board.potionBoard[_xIndex, _yIndex] != null && IsUsableAndNotBeforeMatched(_xIndex, _yIndex))
        {
            blocks.Add(board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>());
            //board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>().isMatched = true;
        }

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.potionBoard[i, j] != null && IsUsableAndNotBeforeMatched(i, j))
                {
                    if (board.potionBoard[i, j].potion.GetComponent<Potion>().potionType == _targetPotionType)
                    {
                        // Set that dot to be matched
                        blocks.Add(board.potionBoard[i, j].potion.GetComponent<Potion>());
                        //board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched = true;
                    }
                }
            }
        }

        return blocks;
    }

    // 프리즘 기능
    List<Potion> MatchPiecesOfColor(int _xIndex, int _yIndex, PotionType _targetPotionType)
    {
        List<Potion> blocks = new List<Potion>();

        // Prism 블럭 추가
        if (board.potionBoard[_xIndex, _yIndex] != null && IsUsableAndNotBeforeMatched(_xIndex, _yIndex))
        {
            blocks.Add(board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>());
        }

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                // Check if that piece exists
                if (board.potionBoard[i, j] != null && IsUsableAndNotBeforeMatched(i, j))
                {
                    // Check the tag on that dot
                    if (board.potionBoard[i, j].potion.GetComponent<Potion>().potionType == _targetPotionType)
                    {
                        // Set that dot to be matched
                        blocks.Add(board.potionBoard[i, j].potion.GetComponent<Potion>());
                        //board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched = true;
                    }
                }
            }
        }

        return blocks;
    }

    // 특수블럭 조합 효과
    // 테스트 여부 : O
    public List<Potion> GetRowAndDiagonalPieces(int _xIndex, int _yIndex, int _targetXIndex, int _targetYIndex)
    {
        List<Potion> blocks = new List<Potion>();

        blocks.AddRange(GetRowPieces(_yIndex));
        blocks.AddRange(GetDiagonalPieces(_xIndex, _yIndex));
        blocks.AddRange(GetCurrentPieces(_targetXIndex, _targetYIndex));

        return blocks;
    }

    // 테스트 여부 : O
    public List<Potion> GetRowAndReverseDiagonalPieces(int _xIndex, int _yIndex, int _targetXIndex, int _targetYIndex)
    {
        List<Potion> blocks = new List<Potion>();

        blocks.AddRange(GetRowPieces(_yIndex));
        blocks.AddRange(GetReverseDiagonalPieces(_xIndex, _yIndex));
        blocks.AddRange(GetCurrentPieces(_targetXIndex, _targetYIndex));

        return blocks;
    }

    // 테스트 여부 : O
    public List<Potion> GetColumnAndRowPieces(int _xIndex, int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();

        blocks.AddRange(GetColumnPieces(_xIndex));
        blocks.AddRange(GetRowPieces(_yIndex));

        return blocks;
    }

    // 테스트 여부 : O
    public List<Potion> GetColumnAndDiagonalPieces(int _xIndex, int _yIndex, int _targetXIndex, int _targetYIndex)
    {
        List<Potion> blocks = new List<Potion>();

        blocks.AddRange(GetColumnPieces(_xIndex));
        blocks.AddRange(GetDiagonalPieces(_xIndex, _yIndex));
        blocks.AddRange(GetCurrentPieces(_targetXIndex, _targetYIndex));

        return blocks;
    }

    // 테스트 여부 : O
    public List<Potion> GetColumnAndReverseDiagonalPieces(int _xIndex, int _yIndex, int _targetXIndex, int _targetYIndex)
    {
        List<Potion> blocks = new List<Potion>();

        blocks.AddRange(GetColumnPieces(_xIndex));
        blocks.AddRange(GetReverseDiagonalPieces(_xIndex, _yIndex));
        blocks.AddRange(GetCurrentPieces(_targetXIndex, _targetYIndex));

        return blocks;
    }

    // 테스트 여부 : O
    public List<Potion> Get3DistancePieces(int _xIndex, int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();

        // 5x5 가운데 dot 기준으로 왼쪽 아래 점부터 오른쪽 위 점까지만 루프
        for (int i = _xIndex - 2; i <= _xIndex + 2; i++)
        {
            for (int j = _yIndex - 2; j <= _yIndex + 2; j++)
            {
                // Check if the piece is inside the board 모서리 체크
                if (i >= 0 && i < board.width && j >= 0 && j < board.height && IsUsableAndNotBeforeMatched(i, j))
                {
                    CheckSpecialChainMatch(i, j, blocks);
                }
            }
        }

        if (_xIndex >= 3 && board.potionBoard[_xIndex - 3, _yIndex] != null && IsUsableAndNotBeforeMatched(_xIndex - 3, _yIndex))
        {
            CheckSpecialChainMatch(_xIndex - 3, _yIndex, blocks);
        }
        if (_xIndex < board.width - 3 && board.potionBoard[_xIndex + 3, _yIndex] != null && IsUsableAndNotBeforeMatched(_xIndex + 3, _yIndex))
        {
            CheckSpecialChainMatch(_xIndex + 3, _yIndex, blocks);

        }
        if (_yIndex >= 3 && board.potionBoard[_xIndex, _yIndex - 3] != null && IsUsableAndNotBeforeMatched(_xIndex, _yIndex - 3))
        {
            CheckSpecialChainMatch(_xIndex, _yIndex - 3, blocks);
        }
        if (_yIndex < board.height - 3 && board.potionBoard[_xIndex, _yIndex + 3] != null && IsUsableAndNotBeforeMatched(_xIndex, _yIndex + 3))
        {
            CheckSpecialChainMatch(_xIndex, _yIndex + 3, blocks);
        }

        return blocks;
    }

    // 테스트 여부 : O
    public List<Potion> Get3ColumnPieces(int _xIndex, int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();
        
        for (int i = _xIndex - 1; i <= _xIndex + 1; i++ )
        {
            for (int j = 0; j < board.height; j++)
            {
                if (i >= 0 && i < board.width)
                {
                    blocks.AddRange(GetColumnPieces(i));

                }
            }
        }

        return blocks;
    }

    // 테스트 여부 : O
    public List<Potion> Get3RowPieces(int _xIndex, int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();

        for (int i = _yIndex - 1; i <= _yIndex + 1; i++)
        {
            for (int j = 0; j < board.width; j++)
            {
                if (i >= 0 && i < board.height)
                {
                    blocks.AddRange(GetRowPieces(i));
                }
            }
        }

        return blocks;
    }

    // 테스트 여부 : O
    public List<Potion> Get3DiagonalPieces(int _xIndex, int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();

        for (int i = -1; i<=1; i++)
        {
            blocks.AddRange(GetDiagonalPieces(_xIndex + i, _yIndex));
        }

        return blocks;
    }

    // 테스트 여부 : O
    public List<Potion> Get3ReverseDiagonalPieces(int _xIndex, int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();

        for (int i = -1; i<=1; i++)
        {
            blocks.AddRange(GetReverseDiagonalPieces(_xIndex + i, _yIndex));
        }

        return blocks;
    }

    // 테스트 여부 : O
    // 이걸로 테스트하여 속도 조절
    public List<Potion> GetAllPieces()
    {
        List<Potion> blocks = new List<Potion>();

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (IsUsableAndNotBeforeMatched(i, j))
                {
                    blocks.Add(board.potionBoard[i, j].potion.GetComponent<Potion>());
                    //board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched = true;
                }
            }
        }

        return blocks;
    }

    // 테스트 여부 : O
    public List<Potion> GetDiagonalAndReverseDiagonalPieces(int _xIndex, int _yIndex, int _targetXIndex, int _targetYIndex)
    {
        List<Potion> blocks = new List<Potion>();

        blocks.AddRange(GetDiagonalPieces(_xIndex, _yIndex));
        blocks.AddRange(GetReverseDiagonalPieces(_xIndex, _yIndex));
        blocks.AddRange(GetCurrentPieces(_targetXIndex, _targetYIndex));

        return blocks;
    }

    public List<Potion> GetCurrentPieces(int _xIndex, int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();

        if (IsUsableAndNotBeforeMatched(_xIndex, _yIndex))
        {
            blocks.Add(board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>());
            board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>().isMatched = true;
        }

        return blocks;
    }

    private void SwitchDrillVertical()
    {
        Debug.Log("가장 많은 일반 블럭 드릴(가로)로 교체");
        throw new NotImplementedException();
    }

    private void SwitchDrillHorizontal()
    {
        Debug.Log("가장 많은 일반 블럭 드릴(세로)로 교체");
        throw new NotImplementedException();
    }

    private void SwitchBomb()
    {
        Debug.Log("가장 많은 일반 블럭 폭탄으로 교체");
        throw new NotImplementedException();
    }

    private void SwitchPickLeft()
    {
        Debug.Log("가장 많은 일반 블럭 곡괭이(역대각)으로 교체");
        throw new NotImplementedException();
    }

    private void SwitchPickRight()
    {
        Debug.Log("가장 많은 일반 블럭 곡괭이(대각)으로 교체");
        throw new NotImplementedException();
    }

    private bool IsUsableAndNotBeforeMatched(int _xIndex, int _yIndex)
    {
        return board.potionBoard[_xIndex, _yIndex].isUsable && !board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>().isMatched ? true : false;
    }

    public bool IsSpecialBlock(PotionType potiontype)
    {
        switch (potiontype)
        {
            case PotionType.DrillVertical:
                return true;
            case PotionType.DrillHorizontal:
                return true;
            case PotionType.Bomb:
                return true;
            case PotionType.PickLeft:
                return true;
            case PotionType.PickRight:
                return true;
            case PotionType.Prism:
                return true;
            default:
                return false;
        }
    }

    // 특수 블럭 효과의 경우 제거되는 블럭 리스트에 추가 후 추가로 특수블럭 효과 발동되는지 체크
    void CheckSpecialChainMatch(int _xIndex, int _yIndex, List<Potion> _blocks)
    {
        Potion potion = board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>();
        _blocks.Add(potion);
        potion.isMatched = true;

        if (IsSpecialBlock(potion.potionType))
        {
            _blocks.AddRange(RunSpecialBlock(potion));
        }
    }
}

public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}

// 족보 : 3배열, 4배열 직선, 4배열 네모, 5배열 직선, 5배열 L자 (시스템 기획서 27P)

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