using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour, IManager
{
    public GameManager gameManager
    {
        get
        {
            return GameManager.gameManager;
        }
    }

    //public static SoundManager instance;
    public AudioClip[] audioClips;

    public AudioSource _BGMAudioSource = null;


    // BGM 지속되게 할 경우 해당 BGM 오디오소스 찾아서 DontDestroyOnLoad
    // : 퍼즐에서 다시 빌리징 올 때 다시 제거 후 실행 만들어야 해서 보류함
    //private void Awake()
    //{
    //    if (_BGMAudioSource == null)
    //    {
    //        _BGMAudioSource = GameObject.Find("BGMAudioSource").GetComponent<AudioSource>();
            
    //    }
    //    DontDestroyOnLoad(_BGMAudioSource);
    //}

    public void PlayBackgroundSound(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(audioClips[0], position);
    }

    public void PlayUIClickSound(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(audioClips[1], position);
    }

    public void PlayCharacterWalkSound(Vector3 position)
    {
        if (GameObject.FindObjectsOfType<AudioSource>().Length == 1)
        {
            AudioSource.PlayClipAtPoint(audioClips[2], position, 0.7f);
        }
    }
}
