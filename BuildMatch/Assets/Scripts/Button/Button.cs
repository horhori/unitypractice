using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    private PlayerInstance _PlayerInstance = null;

    // TODO : 1. 사운드 구조 짜서 처리하기
    private SoundManager _SoundManager = null;

    private StageManager _StageManager = null;

    private void Awake()
    {
        _PlayerInstance = GameManager.GetManagerClass<CharacterManager>().player;
        _SoundManager = GameManager.GetManagerClass<SoundManager>();
        _StageManager = GameManager.GetManagerClass<StageManager>();
    }

    #region VillageScene
    // 맵버튼 누르면 스테이지 선택 씬으로 이동
    public void OnMapButtonClicked()
    {
        _SoundManager.PlayUIClickSound(Vector3.zero);
        SceneManager.LoadScene("StageScene");
    }

    // 홈버튼 누르면 빌리지 씬으로 이동
    public void OnHomeButtonClicked()
    {
        _SoundManager.PlayUIClickSound(Vector3.zero);
        SceneManager.LoadScene("VillageScene");
    }

    // 임시로 NPC 클릭 후 다시 원위치로 되돌아가기
    public void ReturnOriginCamera()
    {
        _PlayerInstance.isQuestEnd = true;
    }
    #endregion

    #region StageScene
    public void OnStage1ButtonClicked()
    {
        _SoundManager.PlayBackgroundSound(Vector3.zero);
        _StageManager.stageNumber = 1;
        SceneManager.LoadScene("PuzzleScene");
    }

    public void OnStage2ButtonClicked()
    {
        // TODO : 1. 스테이지 매니저에서 세팅
        _SoundManager.PlayBackgroundSound(Vector3.zero);

    }
    #endregion
}
