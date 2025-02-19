using System;
using UnityEngine;

public class GlobalUI : MonoBehaviour
{
    [SerializeField] private LoadingScreenUI loadingScreen;
    
    public static GlobalUI Singleton;

    private void Awake()
    {
        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowLoadingScreen()
    {
        loadingScreen.Show();
    }

    public void HideLoadingScreen()
    {
        loadingScreen.Hide();
    }
    
}
