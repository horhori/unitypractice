using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    private PlayerInstance _PlayerInstance = null;

    // TODO : 1. 사운드 구조 짜서 처리하기
    private SoundManager _SoundManager = null;

    private void Awake()
    {
        _PlayerInstance = VillageGameManager.GetManagerClass<CharacterManager>().player;
        _SoundManager = VillageGameManager.GetManagerClass<SoundManager>();
    }
    public void OnMapButtonClicked()
    {
        _SoundManager.PlayUIClickSound(Vector3.zero);
        SceneManager.LoadScene("PuzzleScene");
    }

    public void ReturnOriginCamera()
    {
        _PlayerInstance.isQuestEnd = true;
    }
}
