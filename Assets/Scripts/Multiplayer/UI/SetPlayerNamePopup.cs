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
    [SerializeField] private TMP_Text playerNameInMenuText;
    [SerializeField] private GameObject playerNamePopup;
    [SerializeField] private TMP_InputField playerNameText;
    [SerializeField] private Button okButton;
    
    string newPlayerName = "";
    private string orignalName = "";
    private string nameWithoutHash = "";
    
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
        else
        {
            playerNameInMenuText.text = AuthenticationService.Instance.PlayerName;
        }
    }

    public void OpenNameChangePopup()
    {
        orignalName = AuthenticationService.Instance.PlayerName;
        nameWithoutHash = orignalName.Split("#")[0];
        playerNameText.text = nameWithoutHash;
        playerNamePopup.SetActive(true);
    }
    
    public void OnClickUpdateName()
    {
        UpdateName();
    }

    private async void UpdateName()
    {
        okButton.interactable = false;
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newPlayerName);
            playerNameInMenuText.text = AuthenticationService.Instance.PlayerName;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
        playerNamePopup.SetActive(false);
    }

    public void OnTextFieldInputUpdated(string value)
    {
        newPlayerName = playerNameText.text;
        
        if (string.IsNullOrEmpty(newPlayerName) || nameWithoutHash == newPlayerName)
        {
            okButton.interactable = false;
            return;
        }
        
        okButton.interactable = true;
    }

    public void OnClickCancelButton()
    {
        playerNamePopup.SetActive(false);
    }
    
}
