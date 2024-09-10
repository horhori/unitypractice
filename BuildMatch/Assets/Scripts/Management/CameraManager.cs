using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour, IManager
{
    public GameManager gameManager
    {
        get
        {
            return GameManager.gameManager;
        }
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
