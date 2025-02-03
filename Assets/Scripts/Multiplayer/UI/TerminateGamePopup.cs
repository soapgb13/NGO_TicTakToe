using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TerminateGamePopup : MonoBehaviour
{
    [SerializeField] private GameObject terminateGamePopup;
    
    private void OnEnable()
    {
        GameEvents.TerminateGame += ShowTerminatePopup;
    }

    private void OnDestroy()
    {
        GameEvents.TerminateGame -= ShowTerminatePopup;
    }

    private void ShowTerminatePopup(ulong clientId)
    {
        if(BoardManager.Instance.isQuitGame) return;
        if(BoardManager.Instance.isGameOver) return;
        
        terminateGamePopup.SetActive(true);
        NetworkManager.Singleton.Shutdown();
    }

    public void OnClickGoBackButton()
    {
        terminateGamePopup.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }
    
}
