using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInstance : MonoBehaviour, INPC
{
    public PlayerInstance player { get; private set; }

    public CharacterManager characterManager { get; private set; }

    public FollowCamera _FollowCamera = null;

    [SerializeField] private GameObject _QuestBalloon { get; set; }

    [SerializeField] private GameObject _TempPlayer = null;

    

    private void Awake()
    {
        _FollowCamera = GameObject.Find("FollowCamera").GetComponent<FollowCamera>();
        _QuestBalloon = GetComponentInChildren<SpriteRenderer>().gameObject;
    }

    private void Update()
    {

    }

    private void OnMouseDown()
    {
        _TempPlayer.SetActive(true);
        _QuestBalloon.SetActive(false);
        // 각도 바꾸고 UI 창 바꾸면 될듯
        //_FollowCamera.SwitchCamera(_TempPlayer.transform);
    }
}
