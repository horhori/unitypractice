using System.Collections;
using System.Collections.Generic;
using UnityChan;
using UnityEngine;

// TODO : 1. 3_Match GameManager와 VillageGameManager 통합 작업
//         -> 현재 빌리징은 VillageGameManager로 별도 관리중
public class GameManager : MonoBehaviour
{
    public static GameManager _GameManagerInstance = null;
    public List<IManager> _ManagerClass = null;

    public static GameManager gameManager
    {
        get
        {
            if (!_GameManagerInstance)
            {
                _GameManagerInstance = GameObject.Find("GameManager").GetComponent<GameManager>();
                _GameManagerInstance.InitializeGameManager();
            }
            return _GameManagerInstance;
        }
    }

    private void InitializeGameManager()
    {
        _ManagerClass = new List<IManager>();

        RegisterManagerClass<CharacterManager>();
        RegisterManagerClass<SoundManager>();
        RegisterManagerClass<CameraManager>();
    }

    private void RegisterManagerClass<T>() where T : IManager
    {
        _ManagerClass.Add(transform.GetComponentInChildren<T>());
    }

    public static T GetManagerClass<T>() where T : class, IManager { 
        // Type 찾아서 같은 타입으로 넘겨줌
        return gameManager._ManagerClass.Find(
            (IManager managerClass) => managerClass.GetType() == typeof(T) ) as T;
    }

    private void Awake()
    {
        if (_GameManagerInstance && _GameManagerInstance != null)
        {
            //Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

    }

    private void Start()
    {
        SetResolution();
    }

    public void SetResolution()
    {
        int setWidth = 1080;
        int setHeight = 1920;

        Screen.SetResolution(setWidth, setHeight, false);
    }
}
