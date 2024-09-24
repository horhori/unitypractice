using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageUIManager : MonoBehaviour
{
    public GameObject _ControllerUI = null;
    public GameObject _QuestUI = null;

    private void Awake()
    {
        _ControllerUI = GameObject.Find("Controller");
        // TODO : 1. UI 빌리지, 퍼즐 통합 관리 구조 짜기
        // 현재 비활성화 후 유니티에서 직접 넣어서 사용중
        //_QuestUI = GameObject.Find("Quest");
        //_QuestUI.SetActive(false);
    }
}
