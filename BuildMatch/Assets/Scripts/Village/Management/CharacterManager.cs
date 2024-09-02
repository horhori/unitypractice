using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour, IManager
{
    public VillageGameManager gameManager
    {
        get
        {
            return VillageGameManager.gameManager;
        }
    }

    // 조이스틱 방향벡터
    public Vector3 inputVector { get; set; }

    // 캐릭터 인스턴스 프로퍼티
    public PlayerInstance player { get; set; }
}
