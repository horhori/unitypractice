using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    //define the size of the board
    public int width = 7;
    public int height = 7;
    //define some spacing for the board
    public float spacingX;
    public float spacingY;
    //get a reference to our potion prefabs
    public GameObject[] potionPrefabs;
    //get a reference to the collection nodes potionBoard + GO
    public Node[,] potionBoard;
    public GameObject potionBoardGO;


    //layoutArray
    public ArrayLayout arrayLayout;
    //public static of potionboard
    public static PotionBoard Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeBoard();
    }

    void InitializeBoard()
    {
        potionBoard = new Node[width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = (float)(height - 1) / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
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
                    potion.GetComponent<Potion>().SetIndicies(x, y);
                    potionBoard[x, y] = new Node(true, potion);
                }
            }
        }

        if (CheckBoard())
        {
            Debug.Log("We have matches let's re-create the board");
            InitializeBoard();
        }
    }

    public bool CheckBoard()
    {
        Debug.Log("Checking Board");
        bool hasMatched = false;

        List<Potion> potionsToRemove = new();

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

                            potionsToRemove.AddRange(matchedPotions.connectedPotions);

                            foreach (Potion pot in matchedPotions.connectedPotions)
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

    // IsConnected
    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new();

        PotionType potionType = potion.potionType;

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

    // 
}



public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}
