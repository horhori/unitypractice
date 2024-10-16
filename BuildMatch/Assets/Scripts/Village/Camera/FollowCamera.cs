using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    // 따라다닐 타겟 참조
    [SerializeField] private Transform _FollowTargetTransform = null;

    // 추적 속도
    [SerializeField][Range(0.0f, 1000.0f)] private float _FollowSpeed = 5.0f;

    // 카메라
    public Camera camera { get; private set; }

    private void Awake()
    {
        // 카메라 부드럽게 따라가게
        IEnumerator FollowTarget()
        {
            while(true)
            {
                if (_FollowTargetTransform)
                {
                    transform.position = Vector3.Lerp(
                        transform.position, _FollowTargetTransform.position,
                        _FollowSpeed * Time.deltaTime );
                }

                yield return null;
            }
        }

        camera = GetComponentInChildren<Camera>();

        StartCoroutine( FollowTarget() );
    }

    public void SwitchTransformNPCCamera(Transform transform)
    {
        _FollowSpeed = 1000.0f;
        _FollowTargetTransform = transform;
        gameObject.transform.rotation = transform.rotation;
    }

    public void SwitchTransformPlayerCamera(Transform transform)
    {
        _FollowTargetTransform = transform;
        gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        _FollowSpeed = 50.0f;
    }
}
