using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

// Potion -> Block
// �̸� ���� �� �޸� ���� �߻�

// TODO : 1. Potion -> Block �����丵
public class Potion : MonoBehaviour
{

    public PotionType potionType;

    public int xIndex;
    public int yIndex;

    public bool isMatched;
    private Vector2 currentPos;
    private Vector2 targetPos;

    public bool isMoving;

    public Potion(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    public void SetIndicies(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    public void MoveToTarget(Vector2 _targetPos)
    {
        StartCoroutine(MoveCoroutine(_targetPos));
    }

    // TODO : 1. ���� �ٲ�� �ð� �ڿ������� ����
    private IEnumerator MoveCoroutine(Vector2 _targetPos)
    {
        isMoving = true;
        // ���� �ִϸ��̼� ���� �ð���
        float duration = 0.2f;

        Vector2 startPosition = transform.position;
        float elaspedTime = 0f;

        while (elaspedTime < duration)
        {
            float t = elaspedTime / duration;

            transform.position = Vector2.Lerp(startPosition, _targetPos, t);

            elaspedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _targetPos;
        isMoving = false;
    }

}

// TODO : 1. Ư�� ����
public enum PotionType
{
    BlueBlock,
    PurpleBlock,
    GreenBlock,
    OrangeBlock,
    PinkBlock,
    RedBlock,
    YellowBlock,
}