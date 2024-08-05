using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Collections.AllocatorManager;


public class FindMatches : MonoBehaviour
{
    private PotionBoard board;

    // 지워지는 블럭 여기에 저장해서 제거
    public List<Potion> potionsToRemove = new();

    // 특수블럭 생성 여부
    // 4 가로 체크 후 드릴(가로) 생성
    public bool isCheckedHorizontal_4 = false;
    // 4 세로 체크 후 드릴(가로) 생성
    public bool isCheckedVertical_4 = false;
    // 네모 체크 후 곡괭이 생성
    public bool isCheckedSquare = false;
    // 5 가로, 세로 체크 후 프리즘 생성
    public bool isCheckedMatched_5 = false;
    // 5 L자 체크 후 폭탄 생성
    public bool isCheckedSuper = false;


    void Awake()
    {
        board = FindObjectOfType<PotionBoard>();
    }

    // 테스트용
    public bool TestFindSpecialMatches()
    {
        if (GameManager.Instance.isGameEnded)
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
            // here
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
        if (GameManager.Instance.isGameEnded)
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
            if (IsSpecialBlock(currentPotion.potionType))
            {
                potionsToRemove.AddRange(RunSpecialBlock(currentPotion));
                hasMatched = true;
            }
        }


        //for (int x = 0; x < board.width; x++)
        //{
        //    for (int y = 0; y < board.height; y++)
        //    {
        //        // checking if potion node is usable
        //        if (board.potionBoard[x, y].isUsable)
        //        {
        //            // then proceed to get potion class in node.
        //            Potion potion = board.potionBoard[x, y].potion.GetComponent<Potion>();

        //            // ensure its not matched
        //            if (!potion.isMatched)
        //            {
        //                // run some matching logic

        //                MatchResult matchedPotions = IsConnected(potion);

        //                // 네모인 경우 4라서 되는중 -> 네모인 경우는 FindSuperMatch 할 필요 없음
        //                if (matchedPotions.connectedPotions.Count >= 3)
        //                {
        //                    // complex matching...
        //                    MatchResult superMatchedPotions = FindSuperMatch(matchedPotions);

        //                    potionsToRemove.AddRange(superMatchedPotions.connectedPotions);

        //                    foreach (Potion pot in superMatchedPotions.connectedPotions)
        //                    {
        //                        pot.isMatched = true;
        //                    }

        //                    hasMatched = true;
        //                }
        //            }
        //        }
        //    }
        //}

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
        if (GameManager.Instance.isGameEnded)
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
                if (RunCombineSpecialBlocks(currentPotion, targetPotion) != null)
                {
                    potionsToRemove.AddRange(RunCombineSpecialBlocks(currentPotion, targetPotion));
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

                        MatchResult matchedPotions = TestIsConnected(potion);

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            MatchResult superMatchedPotions = FindSuperMatch(matchedPotions);

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
        // if we have a horizontal or long horizontal match
        // loop through the potions in my match
        // create a new list of potions 'extra matches'
        // CheckDirection up
        // CheckDirection down
        // do we have 2 or more extra matches.
        // we've made a super match - return a new matchresult of type super
        // return extra matches
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
                    // TODO : 1. L자 족보 매칭 시 폭탄 생성
                    isCheckedSuper = true;
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

        // if we have a vertical or long vertical match
        // loop through the potions in my match
        // create a new list of potions 'extra matches'
        // CheckDirection up
        // CheckDirection down
        // do we have 2 or more extra matches.
        // we've made a super match - return a new matchresult of type super
        // return extra matches
        if (_matchedResults.direction == MatchDirection.Vertical_3 || _matchedResults.direction == MatchDirection.Vertical_4 || _matchedResults.direction == MatchDirection.LongVertical)
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();

                CheckSuperDirection(pot, new Vector2Int(1, 0), extraConnectedPotions);
                CheckSuperDirection(pot, new Vector2Int(-1, 0), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    isCheckedSuper = true;
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
    //public MatchResult IsConnected(Potion potion)
    //{
    //    List<Potion> connectedPotions = new()
    //    {
    //        potion
    //    };

    //    // 가로 체크
    //    CheckHorizontalMatch(potion, connectedPotions);

    //    // 우선 순위 적용
    //    // 1. 5 이상 가로 매치
    //    if (connectedPotions.Count >= 5)
    //    {
    //        //Debug.Log("I have a Long horizontal match, the color of my match is : " + connectedPotions[0].potionType);
    //        isCheckedMatched_5 = true;

    //        return new MatchResult
    //        {
    //            connectedPotions = connectedPotions,
    //            direction = MatchDirection.LongHorizontal
    //        };
    //    }

    //    // 2. 4 가로 매치

    //    else if (connectedPotions.Count == 4)
    //    {
    //        // 4 이상 가로 매치

    //        isCheckedHorizontal_4 = true;

    //        return new MatchResult
    //        {
    //            connectedPotions = connectedPotions,
    //            direction = MatchDirection.Horizontal_4
    //        };
    //    }

    //    // 3. 3 가로 매치
    //    else if (connectedPotions.Count == 3)
    //    {
    //        if (CheckHorizontalSquareMatch(potion, connectedPotions))
    //        {
    //            return new MatchResult
    //            {
    //                connectedPotions = connectedPotions,
    //                direction = MatchDirection.Square
    //            };
    //        }
    //        else
    //        {
    //            return new MatchResult
    //            {
    //                connectedPotions = connectedPotions,
    //                direction = MatchDirection.Horizontal_3
    //            };
    //        }
    //    }

    //    // 4. 2 가로 매치
    //    else if (connectedPotions.Count == 2)
    //    {
    //        if (CheckHorizontalSquareMatch(potion, connectedPotions))
    //        {
    //            return new MatchResult
    //            {
    //                connectedPotions = connectedPotions,
    //                direction = MatchDirection.Square
    //            };
    //        }
    //    }

    //    // clear out the connectedpotions
    //    connectedPotions.Clear();

    //    // read our initial potion
    //    connectedPotions.Add(potion);

    //    // 세로 체크
    //    CheckVerticalMatch(potion, connectedPotions);

    //    // check up
    //    //CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);

    //    // check down -- 없어도 될듯? 체크 필요
    //    //CheckDirection(potion, new Vector2Int(0, -1), connectedPotions);

    //    // 우선 순위 적용
    //    // 1. 5 이상 세로 매치
    //    if (connectedPotions.Count >= 5)
    //    {
    //        //Debug.Log("I have a Long Vertical match, the color of my match is : " + connectedPotions[0].potionType);
    //        isCheckedMatched_5 = true;

    //        return new MatchResult
    //        {
    //            connectedPotions = connectedPotions,
    //            direction = MatchDirection.LongVertical
    //        };
    //    }
    //    // 2. 4 세로 매치
    //    else if (connectedPotions.Count == 4)
    //    {
    //        //Debug.Log("I have a Vertical_4 match, the color of my match is : " + connectedPotions[0].potionType);
    //        isCheckedVertical_4 = true;

    //        return new MatchResult
    //        {
    //            connectedPotions = connectedPotions,
    //            direction = MatchDirection.Vertical_4
    //        };
    //    }
    //    // 3. 3 세로 매치
    //    // TODO : 1. xOO
    //    //           OOO 해당 배치를 세로로
    //    else if (connectedPotions.Count == 3)
    //    {
    //        //return new MatchResult
    //        //{
    //        //    connectedPotions = connectedPotions,
    //        //    direction = MatchDirection.Vertical_3
    //        //};

    //        if (CheckVerticalSquareMatch(potion, connectedPotions))
    //        {
    //            return new MatchResult
    //            {
    //                connectedPotions = connectedPotions,
    //                direction = MatchDirection.Square
    //            };
    //        }
    //        else
    //        {
    //            return new MatchResult
    //            {
    //                connectedPotions = connectedPotions,
    //                direction = MatchDirection.Vertical_3
    //            };
    //        }
    //    }

    //    // 가로 체크에서 네모 항상 발견해서 여기서는 네모 체크 제외
    //    else
    //    {
    //        return new MatchResult
    //        {
    //            connectedPotions = connectedPotions,
    //            direction = MatchDirection.None
    //        };
    //    }
    //}

    // 블럭 타입이 일치하는지 확인 후 Match 결과 반환
    public MatchResult TestIsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new()
        {
            potion
        };

        // 우선 순위 적용


        // 가로 체크
        CheckHorizontalMatch(potion, connectedPotions);

        // 1. 5 이상 가로 매치
        if (connectedPotions.Count >= 5)
        {
            //Debug.Log("I have a Long horizontal match, the color of my match is : " + connectedPotions[0].potionType);
            isCheckedMatched_5 = true;

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };
        }

        // 2. 4 가로 매치

        else if (connectedPotions.Count == 4)
        {
            // 4 이상 가로 매치

            isCheckedHorizontal_4 = true;

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal_4
            };
        }

        // 3. 3 가로 매치
        else if (connectedPotions.Count == 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal_3
            };
        }

        // 4. 2 가로 매치
        //else if (connectedPotions.Count == 2)
        //{
        //    if (CheckHorizontalSquareMatch(potion, connectedPotions))
        //    {
        //        return new MatchResult
        //        {
        //            connectedPotions = connectedPotions,
        //            direction = MatchDirection.Square
        //        };
        //    }
        //}

        // clear out the connectedpotions
        connectedPotions.Clear();

        // read our initial potion
        connectedPotions.Add(potion);

        // 세로 체크
        CheckVerticalMatch(potion, connectedPotions);

        // check up
        //CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);

        // check down -- 없어도 될듯? 체크 필요
        //CheckDirection(potion, new Vector2Int(0, -1), connectedPotions);

        // 우선 순위 적용
        // 1. 5 이상 세로 매치
        if (connectedPotions.Count >= 5)
        {
            //Debug.Log("I have a Long Vertical match, the color of my match is : " + connectedPotions[0].potionType);
            isCheckedMatched_5 = true;

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongVertical
            };
        }
        // 2. 4 세로 매치
        else if (connectedPotions.Count == 4)
        {
            //Debug.Log("I have a Vertical_4 match, the color of my match is : " + connectedPotions[0].potionType);
            isCheckedVertical_4 = true;

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical_4
            };
        }
        // 3. 3 세로 매치
        // TODO : 1. xOO
        //           OOO 해당 배치를 세로로
        else if (connectedPotions.Count == 3)
        {
            //return new MatchResult
            //{
            //    connectedPotions = connectedPotions,
            //    direction = MatchDirection.Vertical_3
            //};

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical_3
            };
        }

        // clear out the connectedpotions
        connectedPotions.Clear();

        // read our initial potion
        connectedPotions.Add(potion);

        // 4 이상 네모
        CheckSquareMatch(potion, connectedPotions);

        if (connectedPotions.Count >= 4)
        {
            isCheckedSquare = true;

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

    void CheckHorizontalMatch(Potion pot, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + 1;
        int y = pot.yIndex;

        // check that we're within the boundaries of the board
        while (x >= 0 && x < board.width)
        {
            if (board.potionBoard[x, y].isUsable)
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
            if (board.potionBoard[x, y].isUsable)
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

        if (x < board.width - 1 && y < board.height - 1 && board.potionBoard[x + 1, y].isUsable)
        {
            Potion rightneighbourPotion = board.potionBoard[x + 1, y].potion.GetComponent<Potion>();

            if (!rightneighbourPotion.isMatched && rightneighbourPotion.potionType == potionType)
            {
                // 위, 오른족 위 블럭 같은지 체크
                if (
                    board.potionBoard[x, y + 1].isUsable &&
                    board.potionBoard[x + 1, y + 1].isUsable)
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
                        if (y > 0 && board.potionBoard[x, y - 1].isUsable && board.potionBoard[x, y - 1].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion leftdownOtherPotion = board.potionBoard[x, y - 1].potion.GetComponent<Potion>();

                            connectedPotions.Add(leftdownOtherPotion);
                            //leftdownOtherPotion.isMatched = true;
                        }

                        // 아래 오른쪽
                        if (y > 0 && board.potionBoard[x + 1, y - 1].isUsable && board.potionBoard[x + 1, y - 1].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion rightdownOtherPotion = board.potionBoard[x + 1, y - 1].potion.GetComponent<Potion>();

                            connectedPotions.Add(rightdownOtherPotion);
                            //rightdownOtherPotion.isMatched = true;
                        }

                        // 왼쪽
                        if (x > 0 && board.potionBoard[x - 1, y].isUsable && board.potionBoard[x - 1, y].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion leftOtherPotion = board.potionBoard[x - 1, y].potion.GetComponent<Potion>();

                            connectedPotions.Add(leftOtherPotion);
                            //leftOtherPotion.isMatched = true;
                        }

                        // 왼쪽 위
                        if (x > 0 && board.potionBoard[x - 1, y + 1].isUsable && board.potionBoard[x - 1, y + 1].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion leftupOtherPotion = board.potionBoard[x - 1, y + 1].potion.GetComponent<Potion>();

                            connectedPotions.Add(leftupOtherPotion);
                            //leftupOtherPotion.isMatched = true;
                        }

                        // 먼 위 왼쪽
                        if (y < board.height - 2 && board.potionBoard[x, y + 2].isUsable && board.potionBoard[x, y + 2].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion farupleftOtherPotion = board.potionBoard[x, y + 2].potion.GetComponent<Potion>();

                            connectedPotions.Add(farupleftOtherPotion);
                            //farupleftOtherPotion.isMatched = true;
                        }

                        // 먼 위 오른쪽
                        if (y < board.height - 2 && board.potionBoard[x + 1, y + 2].isUsable && board.potionBoard[x + 1, y + 2].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion faruprightOtherPotion = board.potionBoard[x + 1, y + 2].potion.GetComponent<Potion>();

                            connectedPotions.Add(faruprightOtherPotion);
                            //faruprightOtherPotion.isMatched = true;
                        }

                        // 먼 오른쪽
                        if (x < board.width - 2 && board.potionBoard[x + 2, y].isUsable && board.potionBoard[x + 2, y].potion.GetComponent<Potion>().potionType == potionType)
                        {
                            Potion farrightOtherPotion = board.potionBoard[x + 2, y].potion.GetComponent<Potion>();

                            connectedPotions.Add(farrightOtherPotion);
                            //farrightOtherPotion.isMatched = true;
                        }

                        // 먼 오른쪽 위
                        if (x < board.width - 2 && board.potionBoard[x + 2, y + 1].isUsable && board.potionBoard[x + 2, y + 1].potion.GetComponent<Potion>().potionType == potionType)
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


    // 네모 추가 체크
    // 가로 체크의 경우에만 해당 메서드 실행중
    // TODO : 1. 가로의 경우에만 네모 체크중
    //        2. 세로 3줄과 함께 네모되는 경우 네모만 사라지는중 -> 세로 3줄에서도 네모 체크해야함
    //        3. 가로 위쪽에 3줄, 세로 오른쪽 3줄에서도 네모만 사라짐 -> neighbourPotion 기준으로도 반대쪽 네모 체크해야함
    //bool CheckHorizontalSquareMatch(Potion pot, List<Potion> connectedPotions)
    //{
    //    PotionType potionType = pot.potionType;
    //    int x = pot.xIndex;
    //    int y = pot.yIndex;

    //    Potion neighbourPotion = board.potionBoard[x + 1, y].potion.GetComponent<Potion>();

    //    // 오른쪽 블럭 같은지 체크
    //    if (!neighbourPotion.isMatched && neighbourPotion.potionType == potionType)
    //    {
    //        // 위, 오른족 위 블럭 같은지 체크
    //        if (!isCheckedSquare && y < board.height - 1 && board.potionBoard[x + 1, y].isUsable && board.potionBoard[x, y + 1].isUsable && board.potionBoard[x + 1, y + 1].isUsable)
    //        {
    //            Potion upNeighbourPotion = board.potionBoard[x, y + 1].potion.GetComponent<Potion>();
    //            Potion rightupNeighbourPotion = board.potionBoard[x + 1, y + 1].potion.GetComponent<Potion>();

    //            if (neighbourPotion.potionType == upNeighbourPotion.potionType && neighbourPotion.potionType == rightupNeighbourPotion.potionType)
    //            {
    //                isCheckedSquare = true;
    //                connectedPotions.Add(upNeighbourPotion);
    //                connectedPotions.Add(rightupNeighbourPotion);

    //                return true;
    //            }
    //        }

    //        //// 아래, 오른쪽 아래 블럭 같은지 체크
    //        //if (!isCheckedSquare && y > 0 && board.potionBoard[x + 1, y].isUsable && board.potionBoard[x, y - 1].isUsable && board.potionBoard[x + 1, y - 1].isUsable)
    //        //{
    //        //    Potion downNeighbourPotion = board.potionBoard[x, y - 1].potion.GetComponent<Potion>();
    //        //    Potion rightdownNeighbourPotion = board.potionBoard[x + 1, y - 1].potion.GetComponent<Potion>();

    //        //    if (neighbourPotion.potionType == downNeighbourPotion.potionType && neighbourPotion.potionType == rightdownNeighbourPotion.potionType)
    //        //    {
    //        //        isCheckedSquare = true;
    //        //        connectedPotions.Add(downNeighbourPotion);
    //        //        connectedPotions.Add(rightdownNeighbourPotion);

    //        //        return true;
    //        //    }
    //        //}
    //    }

    //    return false;
    //}

    //// 세로 체크용
    /// 가로 2 체크때 스퀘어 체크됨
    //bool CheckVerticalSquareMatch(Potion pot, List<Potion> connectedPotions)
    //{
    //    PotionType potionType = pot.potionType;
    //    int x = pot.xIndex;
    //    int y = pot.yIndex;

    //    Potion neighbourPotion = board.potionBoard[x, y + 1].potion.GetComponent<Potion>();

    //    // 위 블럭 같은지 체크
    //    if (!neighbourPotion.isMatched && neighbourPotion.potionType == potionType)
    //    {
    //        // 오른쪽, 오른쪽위 블럭 같은지 체크
    //        if (!isCheckedSquare && x < board.width - 1 && board.potionBoard[x + 1, y].isUsable && board.potionBoard[x, y + 1].isUsable && board.potionBoard[x + 1, y + 1].isUsable)
    //        {
    //            Potion rightNeighbourPotion = board.potionBoard[x + 1, y].potion.GetComponent<Potion>();
    //            Potion rightupNeighbourPotion = board.potionBoard[x + 1, y + 1].potion.GetComponent<Potion>();

    //            // 오른쪽, 오른쪽위 블럭 같은지 체크
    //            if (neighbourPotion.potionType == rightNeighbourPotion.potionType && neighbourPotion.potionType == rightupNeighbourPotion.potionType)
    //            {
    //                isCheckedSquare = true;
    //                connectedPotions.Add(rightNeighbourPotion);
    //                connectedPotions.Add(rightupNeighbourPotion);

    //                return true;
    //            }
    //        }

    //        //// 왼쪽, 왼쪽위 블럭 같은지 체크
    //        //if (!isCheckedSquare && x > 0 && board.potionBoard[x - 1, y].isUsable && board.potionBoard[x, y + 1].isUsable && board.potionBoard[x - 1, y + 1].isUsable)
    //        //{
    //        //    Potion leftNeighbourPotion = board.potionBoard[x - 1, y].potion.GetComponent<Potion>();
    //        //    Potion leftupNeighbourPotion = board.potionBoard[x - 1, y + 1].potion.GetComponent<Potion>();

    //        //    // 왼쪽, 왼쪽위 블럭 같은지 체크
    //        //    if (neighbourPotion.potionType == leftNeighbourPotion.potionType && neighbourPotion.potionType == leftupNeighbourPotion.potionType)
    //        //    {
    //        //        isCheckedSquare = true;
    //        //        connectedPotions.Add(leftNeighbourPotion);
    //        //        connectedPotions.Add(leftupNeighbourPotion);

    //        //        return true;
    //        //    }
    //        //}
    //    }

    //    return false;
    //}

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
            if (board.potionBoard[x, y].isUsable)
            {
                Potion neighbourPotion = board.potionBoard[x, y].potion.GetComponent<Potion>();

                // does our potionType Match? it must also not be matched
                if (!neighbourPotion.isMatched && neighbourPotion.potionType == potionType)
                {
                    connectedPotions.Add(neighbourPotion);

                    // 네모 추가 체크
                    // 슈퍼매칭때는 없어도 됨 이미 가로 체크에서 체크한거라
                    //if (!isCheckedSquare && y < board.height - 1 && board.potionBoard[x - 1, y].isUsable && board.potionBoard[x, y + 1].isUsable && board.potionBoard[x - 1, y + 1].isUsable)
                    //{
                    //    PotionType leftNeighbourPotionType = board.potionBoard[x - 1, y].potion.GetComponent<Potion>().potionType;
                    //    PotionType upNeighbourPotionType = board.potionBoard[x, y + 1].potion.GetComponent<Potion>().potionType;
                    //    PotionType leftupNeighbourPotionType = board.potionBoard[x - 1, y + 1].potion.GetComponent<Potion>().potionType;

                    //    if (neighbourPotion.potionType == leftNeighbourPotionType && neighbourPotion.potionType == upNeighbourPotionType && neighbourPotion.potionType == leftupNeighbourPotionType)
                    //    {
                    //        isCheckedSquare = true;
                    //    }
                    //}

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
        Debug.Log("둘다 특수");
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
            Potion potion = board.potionBoard[_xIndex, i].potion.GetComponent<Potion>();
            if (board.potionBoard[_xIndex, i].isUsable && !potion.isMatched)
            {
                blocks.Add(potion);
                potion.isMatched = true;

                if (IsSpecialBlock(potion.potionType))
                {
                    Debug.Log("potionType" + potion.potionType);
                    blocks.AddRange(RunSpecialBlock(potion));
                }
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
            Potion potion = board.potionBoard[i, _yIndex].potion.GetComponent<Potion>();
            if (board.potionBoard[i, _yIndex].isUsable && !potion.isMatched)
            {
                blocks.Add(potion);
                potion.isMatched = true;

                if (IsSpecialBlock(potion.potionType))
                {
                    Debug.Log("potionType" + potion.potionType);
                    blocks.AddRange(RunSpecialBlock(potion));
                }

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
                    if (i >= 0 && i < board.width && j >= 0 && j < board.height && board.potionBoard[i, j].isUsable && !board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched)
                    {
                        Potion potion = board.potionBoard[i, j].potion.GetComponent<Potion>();

                        blocks.Add(potion);
                        potion.isMatched = true;

                        if (IsSpecialBlock(potion.potionType))
                        {
                            Debug.Log("potionType" + potion.potionType);
                            blocks.AddRange(RunSpecialBlock(potion));
                        }
                    }
                
            }
        }

        if (_xIndex >= 2 && board.potionBoard[_xIndex - 2, _yIndex] != null && board.potionBoard[_xIndex - 2, _yIndex].isUsable && !board.potionBoard[_xIndex - 2, _yIndex].potion.GetComponent<Potion>().isMatched)
        {
            Potion potion = board.potionBoard[_xIndex - 2, _yIndex].potion.GetComponent<Potion>();
            blocks.Add(potion);
            potion.isMatched = true;

            if (IsSpecialBlock(potion.potionType))
            {
                blocks.AddRange(RunSpecialBlock(potion));
            }
        }
        if (_xIndex < board.width - 2 && board.potionBoard[_xIndex + 2, _yIndex] != null && board.potionBoard[_xIndex + 2, _yIndex].isUsable && !board.potionBoard[_xIndex + 2, _yIndex].potion.GetComponent<Potion>().isMatched)
        {
            Potion potion = board.potionBoard[_xIndex + 2, _yIndex].potion.GetComponent<Potion>();
            blocks.Add(potion);
            potion.isMatched = true;

            if (IsSpecialBlock(potion.potionType))
            {
                blocks.AddRange(RunSpecialBlock(potion));
            }

        }
        if (_yIndex >= 2 && board.potionBoard[_xIndex, _yIndex - 2] != null && board.potionBoard[_xIndex, _yIndex - 2].isUsable && !board.potionBoard[_xIndex, _yIndex - 2].potion.GetComponent<Potion>().isMatched)
        {
            Potion potion = board.potionBoard[_xIndex, _yIndex - 2].potion.GetComponent<Potion>();
            blocks.Add(potion);
            potion.isMatched = true;

            if (IsSpecialBlock(potion.potionType))
            {
                blocks.AddRange(RunSpecialBlock(potion));
            }
        }
        if (_yIndex < board.height - 2 && board.potionBoard[_xIndex, _yIndex + 2] != null && board.potionBoard[_xIndex, _yIndex + 2].isUsable && !board.potionBoard[_xIndex, _yIndex + 2].potion.GetComponent<Potion>().isMatched)
        {
            Potion potion = board.potionBoard[_xIndex, _yIndex + 2].potion.GetComponent<Potion>();
            blocks.Add(potion);
            potion.isMatched = true;

            if (IsSpecialBlock(potion.potionType))
            {
                blocks.AddRange(RunSpecialBlock(potion));
            }
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
            if (board.potionBoard[_xIndex - index, _yIndex - index] != null && board.potionBoard[_xIndex - index, _yIndex - index].isUsable && !board.potionBoard[_xIndex - index, _yIndex - index].potion.GetComponent<Potion>().isMatched)
            {
                blocks.Add(board.potionBoard[_xIndex - index, _yIndex - index].potion.GetComponent<Potion>());
                //board.potionBoard[_xIndex - index, _yIndex - index].potion.GetComponent<Potion>().isMatched = true;
            }
            index++;
        }

        index = 0;

        while (_xIndex + index < board.width && _yIndex + index < board.height)
        {
            if (board.potionBoard[_xIndex + index, _yIndex + index] != null && board.potionBoard[_xIndex + index, _yIndex + index].isUsable && !board.potionBoard[_xIndex + index, _yIndex + index].potion.GetComponent<Potion>().isMatched)
            {
                blocks.Add(board.potionBoard[_xIndex + index, _yIndex + index].potion.GetComponent<Potion>());
                //board.potionBoard[_xIndex + index, _yIndex + index].potion.GetComponent<Potion>().isMatched = true;
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
            if (board.potionBoard[_xIndex - index, _yIndex + index] != null && board.potionBoard[_xIndex - index, _yIndex + index].isUsable && !board.potionBoard[_xIndex - index, _yIndex + index].potion.GetComponent<Potion>().isMatched)
            {
                blocks.Add(board.potionBoard[_xIndex - index, _yIndex + index].potion.GetComponent<Potion>());
                //board.potionBoard[_xIndex - index, _yIndex + index].potion.GetComponent<Potion>().isMatched = true;
            }
            index++;
        }

        index = 0;

        while (_xIndex + index < board.width && _yIndex - index >= 0)
        {
            if (board.potionBoard[_xIndex + index, _yIndex - index] != null && board.potionBoard[_xIndex + index, _yIndex - index].isUsable && !board.potionBoard[_xIndex + index, _yIndex - index].potion.GetComponent<Potion>().isMatched)
            {
                blocks.Add(board.potionBoard[_xIndex + index, _yIndex - index].potion.GetComponent<Potion>());
                //board.potionBoard[_xIndex + index, _yIndex - index].potion.GetComponent<Potion>().isMatched = true;
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
        int[] blockCounts = Enumerable.Repeat<int>(0, board.normalBlockLength).ToArray<int>();

        // 가장 많은 색상 찾아주는 
        for (int i=0; i<board.width; i++)
        {
            for (int j=0; j<board.height; j++)
            {
                if (board.potionBoard[i, j] != null && board.potionBoard[i, j].isUsable && !board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched)
                {
                    if (!IsSpecialBlock(board.potionBoard[i, j].potion.GetComponent<Potion>().potionType))
                    blockCounts[(int)board.potionBoard[i, j].potion.GetComponent<Potion>().potionType]++;
                }

            }
        }

        // 가장 많은 색상 potionType 찾음
        // TODO : 1. 가장 많은 색상이 동일한 경우 처리
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
        if (board.potionBoard[_xIndex, _yIndex] != null && board.potionBoard[_xIndex, _xIndex].isUsable && !board.potionBoard[_xIndex, _xIndex].potion.GetComponent<Potion>().isMatched)
        {
            blocks.Add(board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>());
            //board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>().isMatched = true;
        }

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                // Check if that piece exists
                if (board.potionBoard[i, j] != null && board.potionBoard[i, j].isUsable && !board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched)
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

    // 프리즘 기능
    List<Potion> MatchPiecesOfColor(int _xIndex, int _yIndex, PotionType _targetPotionType)
    {
        List<Potion> blocks = new List<Potion>();

        // Prism 블럭 추가
        if (board.potionBoard[_xIndex, _yIndex] != null && !board.potionBoard[_xIndex, _xIndex].potion.GetComponent<Potion>().isMatched)
        {
            blocks.Add(board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>());
        }

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                // Check if that piece exists
                if (board.potionBoard[i, j] != null && !board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched)
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
                if (i >= 0 && i < board.width && j >= 0 && j < board.height && board.potionBoard[i, j] != null && !board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched)
                {
                    blocks.Add(board.potionBoard[i, j].potion.GetComponent<Potion>());
                    //board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched = true;
                }
            }
        }

        if (_xIndex >= 3 && board.potionBoard[_xIndex - 3, _yIndex] != null && !board.potionBoard[_xIndex - 3, _yIndex].potion.GetComponent<Potion>().isMatched)
        {
            blocks.Add(board.potionBoard[_xIndex - 3, _yIndex].potion.GetComponent<Potion>());
            //board.potionBoard[_xIndex - 3, _yIndex].potion.GetComponent<Potion>().isMatched = true;
        }
        if (_xIndex < board.width - 3 && board.potionBoard[_xIndex + 3, _yIndex] != null && !board.potionBoard[_xIndex + 3, _yIndex].potion.GetComponent<Potion>().isMatched)
        {
            blocks.Add(board.potionBoard[_xIndex + 3, _yIndex].potion.GetComponent<Potion>());
            //board.potionBoard[_xIndex + 3, _yIndex].potion.GetComponent<Potion>().isMatched = true;
        }
        if (_yIndex >= 3 && board.potionBoard[_xIndex, _yIndex - 3] != null && !board.potionBoard[_xIndex, _yIndex - 3].potion.GetComponent<Potion>().isMatched)
        {
            blocks.Add(board.potionBoard[_xIndex, _yIndex - 3].potion.GetComponent<Potion>());
            //board.potionBoard[_xIndex, _yIndex - 3].potion.GetComponent<Potion>().isMatched = true;
        }
        if (_yIndex < board.height - 3 && board.potionBoard[_xIndex, _yIndex + 3] != null && !board.potionBoard[_xIndex, _yIndex + 3].potion.GetComponent<Potion>().isMatched)
        {
            blocks.Add(board.potionBoard[_xIndex, _yIndex + 3].potion.GetComponent<Potion>());
            //board.potionBoard[_xIndex, _yIndex + 3].potion.GetComponent<Potion>().isMatched = true;
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
                if (i >= 0 && i < board.width && board.potionBoard[i, j].isUsable && !board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched)
                {
                    blocks.Add(board.potionBoard[i, j].potion.GetComponent<Potion>());
                    //board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched = true;
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
                if (i >= 0 && i < board.height && board.potionBoard[j, i].isUsable && !board.potionBoard[j, i].potion.GetComponent<Potion>().isMatched)
                {
                    blocks.Add(board.potionBoard[j, i].potion.GetComponent<Potion>());
                    //board.potionBoard[j, i].potion.GetComponent<Potion>().isMatched = true;
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
            int index = 0;

            while (_xIndex - index + i >= 0 && _yIndex - index >= 0)
            {
                if (_xIndex - index + i < board.width && board.potionBoard[_xIndex - index + i, _yIndex - index] != null && !board.potionBoard[_xIndex - index + i, _yIndex - index].potion.GetComponent<Potion>().isMatched)
                {
                    blocks.Add(board.potionBoard[_xIndex - index + i, _yIndex - index].potion.GetComponent<Potion>());
                    //board.potionBoard[_xIndex - index + i, _yIndex - index].potion.GetComponent<Potion>().isMatched = true;
                }
 
                index++;
            }   

            index = 0;

            while (_xIndex + index + i < board.width && _yIndex + index < board.height)
            {
                if (_xIndex + index + i >= 0 && board.potionBoard[_xIndex + index + i, _yIndex + index] != null && !board.potionBoard[_xIndex + index + i, _yIndex + index].potion.GetComponent<Potion>().isMatched)
                {
                    blocks.Add(board.potionBoard[_xIndex + index + i, _yIndex + index].potion.GetComponent<Potion>());
                    //board.potionBoard[_xIndex + index + i, _yIndex + index].potion.GetComponent<Potion>().isMatched = true;

                }

                index++;
            }
        }

        return blocks;
    }

    // 테스트 여부 : O
    public List<Potion> Get3ReverseDiagonalPieces(int _xIndex, int _yIndex)
    {
        List<Potion> blocks = new List<Potion>();

        for (int i = -1; i<=1; i++)
        {
            int index = 0;
 
            while (_xIndex - index + i >= 0 && _yIndex + index < board.height)
            {
                if (_xIndex - index + i < board.width && board.potionBoard[_xIndex - index + i, _yIndex + index] != null && !board.potionBoard[_xIndex - index + i, _yIndex + index].potion.GetComponent<Potion>().isMatched)
                {
                    blocks.Add(board.potionBoard[_xIndex - index + i, _yIndex + index].potion.GetComponent<Potion>());
                    //board.potionBoard[_xIndex - index + i, _yIndex + index].potion.GetComponent<Potion>().isMatched = true;

                }
                index++;
            }

            index = 0;

            while (_xIndex + index + i < board.width && _yIndex - index >= 0)
            {
                if (_xIndex + index + i >= 0 && board.potionBoard[_xIndex + index + i, _yIndex - index] != null && !board.potionBoard[_xIndex + index + i, _yIndex - index].potion.GetComponent<Potion>().isMatched)
                {
                    blocks.Add(board.potionBoard[_xIndex + index + i, _yIndex - index].potion.GetComponent<Potion>());
                    //board.potionBoard[_xIndex + index + i, _yIndex - index].potion.GetComponent<Potion>().isMatched = true;

                }
                index++;
            }
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
                if (board.potionBoard[i, j].isUsable && !board.potionBoard[i, j].potion.GetComponent<Potion>().isMatched)
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

        // Prism 블럭 추가
        if (board.potionBoard[_xIndex, _yIndex] != null && !board.potionBoard[_xIndex, _xIndex].potion.GetComponent<Potion>().isMatched)
        {
            blocks.Add(board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>());
            //board.potionBoard[_xIndex, _yIndex].potion.GetComponent<Potion>().isMatched = true;
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
}