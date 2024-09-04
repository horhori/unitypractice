using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    // TODO : 1. 사운드 구조 짜서 처리하기
    private SoundManager _SoundManager = null;

    private void Awake()
    {
        _SoundManager = VillageGameManager.GetManagerClass<SoundManager>();

    }
    public void OnMapButtonClicked()
    {
        _SoundManager.PlayUIClickSound(Vector3.zero);
        SceneManager.LoadScene("PuzzleScene");
    } 
}
