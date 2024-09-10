using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveBlockEffectPool : MonoBehaviour
{
    public RemoveBlockEffect m_RemoveBlockEffect = null;
    private ObjectPool<RemoveBlockEffect> _RemoveBlockEffectPool = null;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        _RemoveBlockEffectPool = new ObjectPool<RemoveBlockEffect>();
    }

    // 제거 이펙트 실행
    public void PlayEffect(Vector2 position)
    {
        IEnumerator ProcessPlayEffect()
        {
            RemoveBlockEffect removeBlockEffect = _RemoveBlockEffectPool.GetRecyclableObject() ??
                _RemoveBlockEffectPool.RegisterRecyclableObject(Instantiate(m_RemoveBlockEffect));
            if (removeBlockEffect != null)
            {
                removeBlockEffect.gameObject.SetActive(true);
                removeBlockEffect.transform.position = position;
            }
            yield return new WaitForSeconds(0.6f);
            removeBlockEffect.gameObject.SetActive(false);
        }

        StartCoroutine(ProcessPlayEffect());
    }

}
