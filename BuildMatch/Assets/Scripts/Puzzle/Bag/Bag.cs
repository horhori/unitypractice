
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 목표 바구니 오브젝트
// TDOD : 1. 임시로 PuzzleManager에서 Bag 컴포넌트 처리중
//           나중에 개별 Bag에 해당 스크립트 올려놓고 처리하도록 변경
public class Bag : MonoBehaviour
{
    public GameObject _Bag;

    //public SpriteRenderer _SpriteRenderer = null;

    public TMP_Text _Text = null;

    public PotionType _PotionType;

    public int CurrentCount;
    public int GoalCount;

    // 바구니 이미지들
    private Image[] bagImageList;

    // 목표량 채웠을 때 체크되는 이미지 따로 저장
    private Image ClearImage;

    public bool ClearCheck = false; // currentCount == GoalCount 되면 check됨

    // TODO : 1. Instituate 함수로 생성자 실행하기? 좀 더 간단할 수 있을듯
    public Bag(int _GoalCount)
    {
        CurrentCount = 15;
        GoalCount = _GoalCount;
    }

    public void SetGoalCount(int _GoalCount)
    {
        GoalCount = _GoalCount;
        _Text.text = CurrentCount.ToString() + " / " + GoalCount.ToString();
    }

    private void Awake()
    {
        //_SpriteRenderer = _Bag.GetComponentInChildren<SpriteRenderer>();
        _Text = _Bag.GetComponentInChildren<TMP_Text>();;
        _Text.text = CurrentCount.ToString() + " / " + GoalCount.ToString();
        bagImageList = _Bag.GetComponentsInChildren<Image>();
        ClearImage = bagImageList[2];
        ClearImage.gameObject.SetActive(false);
        ClearCheck = false;
    }

    private void Update()
    {
        _Text.text = CurrentCount.ToString() + " / " + GoalCount.ToString();

        // TODO : 1. 완료되면 빨간색 변경 말고 체크 이미지로 변경되게
        if (CurrentCount >= GoalCount)
        {
            CurrentCount = GoalCount;
            // sprite 체크로 변경
            ClearImage.gameObject.SetActive(true);
            _Text.gameObject.SetActive(false);
            ClearCheck = true;
            //_Text.color = Color.red;
        }
    }

    public void UpdateCount()
    {
        if (CurrentCount < GoalCount)
        {
            CurrentCount++;
        }
    }
}
