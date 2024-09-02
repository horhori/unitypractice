using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovement
{
    // 이동 방향 벡터
    Vector3 direction { get; }

    void Movement();
}
