using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, IMovement
{
    private CharacterManager _CharacterManager = null;

    private PlayerInstance _PlayerInstance = null;

    public Vector3 direction { get; private set; }
    public Vector3 lookDirection { get; private set; }

    // 플레이어 속도
    [Range(1.0f, 100.0f)] public float m_MoveSpeed = 6.0f;

    private void Awake()
    {
        _CharacterManager = GameManager.GetManagerClass<CharacterManager>();
        _PlayerInstance = GetComponent<PlayerInstance>();
    }

    private void Update()
    {
        (this as IMovement).Movement();
    }

    void IMovement.Movement()
    {
        direction = _CharacterManager.inputVector;

        lookDirection = (direction != Vector3.zero) ? direction.normalized : lookDirection;

        _PlayerInstance.controller.SimpleMove(direction * m_MoveSpeed);

        if (direction != Vector3.zero)
        {
            gameObject.RotateTo(direction);
        }
    }
}
