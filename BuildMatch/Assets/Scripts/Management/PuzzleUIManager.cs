using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO : 1. PuzzleManager���� UI ��� �и�
public class PuzzleUIManager : MonoBehaviour
{
    private PotionBoard board;

    private StageManager _StageManager = null;

    public GameObject warningUI; // 10�� ������ �� ��� UI
    private Image warningImage;

    public GameObject backgroundPanel; // grey background �¸�/�й� ȭ�� Ŭ���� �� ���� ���� �ȵǰ� 
    public GameObject clearPanel;
    public GameObject failedPanel;

    public Text stageText;
    public Text pointsText;
    public Text timeText;

    // ��� UI ���� �÷���
    public Color originWarningColor;
    public Color fullWarningColor;

    // ���â Scale ������
    public Vector3 firstResultScale;
    public Vector3 middleResultScale;
    public Vector3 lastResultScale;

    private void Awake()
    {
        _StageManager = GameManager.GetManagerClass<StageManager>();

        board = FindObjectOfType<PotionBoard>();
        warningImage = warningUI.GetComponent<Image>();
        originWarningColor = warningImage.GetComponent<Image>().color;
        fullWarningColor = new Color(1, 1, 1, 1);

        firstResultScale = Vector3.zero;
        middleResultScale = new Vector3(1.2f, 1.2f, 1);
        lastResultScale = new Vector3(1, 1, 1);
    }

    private void Start()
    {
        stageText.text = "Stage " + _StageManager.stageNumber;
    }

    public void WarningLeftTime()
    {
        timeText.color = Color.red;
        warningUI.SetActive(true);
        StartCoroutine(LerpWarningColor());
    }

    // 10�� �Ǹ� ��� UI ���������ϵ���(�÷��� a��(������) ����)
    public IEnumerator LerpWarningColor()
    {
        while (warningUI.GetComponent<Image>().color != fullWarningColor)
        {
            warningUI.GetComponent<Image>().color = Color.Lerp(originWarningColor, fullWarningColor, Mathf.PingPong(Time.time, 1));
            yield return null;
        }
    }

    // ���â ũ�� ����
    public IEnumerator LerpClearPanelScale()
    {
        PuzzleManager.Instance.isStageEnded = true;
        warningUI.SetActive(false);
        backgroundPanel.SetActive(true);
        clearPanel.SetActive(true);
        board.potionParent.SetActive(false);
        PuzzleManager.Instance.isGameRunning = false;

        bool firstCheck = false;
        bool lastCheck = false;

        float elaspedTime = 0f;

        float duration = 0.5f;

        // 0���� 1.2���� Ŀ��
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

        // 1.2���� 1���� �پ��
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

        // ���� StageFailed ����, ������ �̵��� ������ active ���� false�� ���� 
        PuzzleManager.Instance.isStageEnded = true;
        warningUI.SetActive(false);
        backgroundPanel.SetActive(true);
        failedPanel.SetActive(true);
        board.potionParent.SetActive(false);
        PuzzleManager.Instance.isGameRunning = false;

        bool firstCheck = false;
        bool lastCheck = false;

        float elaspedTime = 0f;

        float duration = 0.5f;

        // 0���� 1.2���� Ŀ��
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

        // 1.2���� 1���� �پ��
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
