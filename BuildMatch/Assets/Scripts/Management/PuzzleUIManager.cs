using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO : 1. PuzzleManager에서 UI 기능 분리
public class PuzzleUIManager : MonoBehaviour
{
    private PotionBoard board;

    public GameObject warningUI; // 10초 남았을 때 경고 UI
    private Image warningImage;

    public GameObject backgroundPanel; // grey background 승리/패배 화면 클릭할 때 포션 동작 안되게 
    public GameObject clearPanel;
    public GameObject failedPanel;

    public TMP_Text stageText;
    public TMP_Text pointsText;
    public TMP_Text timeText;

    // 경고 UI 설정 컬러값
    public Color originWarningColor;
    public Color fullWarningColor;

    // 결과창 Scale 설정값
    public Vector3 firstResultScale;
    public Vector3 middleResultScale;
    public Vector3 lastResultScale;

    private void Awake()
    {
        board = FindObjectOfType<PotionBoard>();
        warningImage = warningUI.GetComponent<Image>();
        originWarningColor = warningImage.GetComponent<Image>().color;
        fullWarningColor = new Color(1, 1, 1, 1);

        firstResultScale = Vector3.zero;
        middleResultScale = new Vector3(1.2f, 1.2f, 1);
        lastResultScale = new Vector3(1, 1, 1);
    }

    public void WarningLeftTime()
    {
        timeText.color = Color.red;
        warningUI.SetActive(true);
        StartCoroutine(LerpWarningColor());
    }

    // 10초 되면 경고 UI 깜빡깜빡하도록(컬러의 a값(불투명도) 조절)
    public IEnumerator LerpWarningColor()
    {
        while (warningUI.GetComponent<Image>().color != fullWarningColor)
        {
            warningUI.GetComponent<Image>().color = Color.Lerp(originWarningColor, fullWarningColor, Mathf.PingPong(Time.time, 1));
            yield return null;
        }
    }

    // 결과창 크기 조절
    public IEnumerator LerpClearPanelScale()
    {
        bool firstCheck = false;
        bool lastCheck = false;

        float elaspedTime = 0f;

        float duration = 0.5f;

        // 0에서 1.2까지 커짐
        while (!firstCheck)
        {
            float t = elaspedTime / duration;
            clearPanel.transform.localScale = Vector3.Lerp(firstResultScale, middleResultScale, t);

            elaspedTime += Time.deltaTime;

            if (clearPanel.transform.localScale.x >= middleResultScale.x - 0.1f)
            {
                firstCheck = true;
            }

            yield return null;
        }

        elaspedTime = 0f;

        // 1.2에서 1까지 줄어듬
        while (!lastCheck)
        {
            float t = elaspedTime / duration;
            clearPanel.transform.localScale = Vector3.Lerp(middleResultScale, lastResultScale, t);

            elaspedTime += Time.deltaTime;

            if (clearPanel.transform.localScale.x <= lastResultScale.x && firstCheck)
            {
                lastCheck = true;
            }

            yield return null;
        }

    }

    public IEnumerator LerpFailedPanelScale()
    {
        yield return new WaitWhile(() => board.isProcessMoving);

        // 기존 StageFailed 내용, 보드판 이동이 끝나면 active 상태 false로 만듬 
        PuzzleManager.Instance.isStageEnded = true;
        warningUI.SetActive(false);
        backgroundPanel.SetActive(true);
        failedPanel.SetActive(true);
        PotionBoard.Instance.potionParent.SetActive(false);
        PuzzleManager.Instance.isGameRunning = false;

        bool firstCheck = false;
        bool lastCheck = false;

        float elaspedTime = 0f;

        float duration = 0.5f;

        // 0에서 1.2까지 커짐
        while (!firstCheck)
        {
            float t = elaspedTime / duration;
            failedPanel.transform.localScale = Vector3.Lerp(firstResultScale, middleResultScale, t);

            elaspedTime += Time.deltaTime;

            if (failedPanel.transform.localScale.x >= middleResultScale.x - 0.1f)
            {
                firstCheck = true;
            }

            yield return null;
        }

        elaspedTime = 0f;

        // 1.2에서 1까지 줄어듬
        while (!lastCheck)
        {
            float t = elaspedTime / duration;
            failedPanel.transform.localScale = Vector3.Lerp(middleResultScale, lastResultScale, t);

            elaspedTime += Time.deltaTime;

            if (failedPanel.transform.localScale.x <= lastResultScale.x && firstCheck)
            {
                lastCheck = true;
            }

            yield return null;
        }
    }
}
