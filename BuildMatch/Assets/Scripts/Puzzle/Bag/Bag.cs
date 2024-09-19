
using TMPro;
using UnityEngine;

// 목표 바구니 오브젝트
// TDOD : 1. 임시로 PuzzleManager에서 Bag 컴포넌트 처리중
//           나중에 개별 Bag에 해당 스크립트 올려놓고 처리하도록 변경
public class Bag : MonoBehaviour
{
    public GameObject _Bag;

    public SpriteRenderer _SpriteRenderer = null;

    public TMP_Text _Text = null;

    public PotionType _PotionType;

    public int CurrentCount;
    public int GoalCount;

    public Bag(int _CurrentCount, int _GoalCount)
    {
        CurrentCount = _CurrentCount;
        GoalCount = _GoalCount;
    }

    private void Awake()
    {
        _SpriteRenderer = _Bag.GetComponentInChildren<SpriteRenderer>();
        _Text = _Bag.GetComponentInChildren<TMP_Text>();;
    }

    private void Update()
    {
        if (CurrentCount >= GoalCount)
        {
            CurrentCount = GoalCount;
            // sprite 체크로 변경
            
            _Text.color = Color.red;
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
