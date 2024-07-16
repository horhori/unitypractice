using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    //to determine whether the space can be filled with potions or not.
    public bool isUsable;

    // TODO : 1. 해당 노드가 스왑되면 매칭이 되는지 여부 (true면 발광, 위아래로 살짝 움직임 적용)
    //public bool isMatchable;

    public GameObject potion;

    public Node(bool _isUsable, GameObject _potion)
    {
        isUsable = _isUsable;
        potion = _potion;
    }
}
