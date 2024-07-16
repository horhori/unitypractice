using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    public float swipeAngle = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
    }

    void CalculateAngle()
    {
        // arctan(yDiff-xDiff) = x 사용
        // 각도를 구함
        // 45도 ~ 135도 = 위
        // -45도 ~ 45도 = 오른쪽
        // -135도 ~ -45도 = 아래
        // -135도 ~ 135도 = 왼쪽
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        Debug.Log("swipeAngle : " + swipeAngle);
    }
}
