using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour, IManager
{
    public GameManager gameManager
    {
        get
        {
            return GameManager.gameManager;
        }
    }

    public int availableStage { get; set; } = 1;

    public int meat { get; set; }

    public int gold { get; set; }


}
