using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class SetPlayerNamePopup : MonoBehaviour
{
    [SerializeField] private GameObject playerNamePopup;
    [SerializeField] private TMP_InputField playerNameText;
    [SerializeField] private Button okButton;
    
    string newPlayerName = "";
    
    private void Start()
    {
        WaitForInitThenCall();
    }

    private async void WaitForInitThenCall()
    {
        while (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
            await Task.Delay(100);
        }
        
        while (AuthenticationService.Instance == null || !AuthenticationService.Instance.IsSignedIn)
        {
            await Task.Delay(100);
        }
        
        if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName))
        {
            playerNameText.text = "";
            okButton.interactable = false;
            playerNamePopup.SetActive(true);
        }
    }

    public void OpenNameChangePopup()
    {
        playerNameText.text = AuthenticationService.Instance.PlayerName;
        playerNamePopup.SetActive(true);
    }
    
    public void OnClickUpdateName()
    {
        UpdateName();
    }

    private async void UpdateName()
    {
        okButton.interactable = false;
        await AuthenticationService.Instance.UpdatePlayerNameAsync(newPlayerName);
        playerNamePopup.SetActive(false);
    }

    public void OnTextFieldInputUpdated(string value)
    {
        newPlayerName = playerNameText.text;
        if (string.IsNullOrEmpty(newPlayerName))
        {
            okButton.interactable = false;
            return;
        }
        
        okButton.interactable = true;
    }

    
    
}
