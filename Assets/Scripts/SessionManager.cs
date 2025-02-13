using UnityEngine;
using Unity.Services.Core;
using System;
using Unity.Services.Authentication;

public class SessionManager : MonoBehaviour
{
    async void Start()
    {
	
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            //await AuthenticationService.Instance.GetPlayerNameAsync();
            
            
            Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId} , PlayerName: {AuthenticationService.Instance.PlayerName}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    
    [ContextMenu("Delete Account")]
    public void DeleteAccount()
    {
        try
        {
            AuthenticationService.Instance.DeleteAccountAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

}