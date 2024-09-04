using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private CharacterManager _CharacterManager = null;

    public Image m_JoystickImage;
    public Image m_JoystickBackground;

    public RectTransform rectTransform {  get; private set; }

    // 입력 값
    private Vector2 _InputVector = Vector2.zero;

    // 임시 조이스틱 화면 종횡비 추가 값
    // 원래 화면에 따라서 가로>세로 인 경우 터치 값을 제대로 못읽는중
    private Vector2 _TempVector = new Vector2(0, 177.8f);

    private void Awake()
    {
        _CharacterManager = VillageGameManager.GetManagerClass<CharacterManager>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {

        _CharacterManager.inputVector = new Vector3(_InputVector.x, 0.0f, _InputVector.y);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {

        float ratio = UnityEngine.Screen.width / 1080.0f;
        _InputVector = eventData.position / ratio;
        //_InputVector -= rectTransform.offsetMin;
        
        // 임시
        if (Screen.width > Screen.height)
        {
            _InputVector += _TempVector;

        }

        float backHalfXSize = m_JoystickBackground.rectTransform.sizeDelta.x * 0.5f;

        _InputVector = (m_JoystickBackground.rectTransform.anchoredPosition - _InputVector) *
            -1.0f / backHalfXSize;

        _InputVector = (_InputVector.magnitude > 1.0f) ?
            _InputVector.normalized : _InputVector;

        m_JoystickImage.rectTransform.anchoredPosition = _InputVector * backHalfXSize;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {

    }

    // 마우스 뗐을 때 제자리로 돌아가도록
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        m_JoystickImage.rectTransform.anchoredPosition = _InputVector = Vector2.zero;
    }
}
