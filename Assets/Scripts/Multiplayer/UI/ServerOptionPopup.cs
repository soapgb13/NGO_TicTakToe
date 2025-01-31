using System;
using TMPro;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ServerOptionPopup : MonoBehaviour
{

    [Header("UI Elements")] 
    [SerializeField] private GameObject serverBtnGroup;
    [SerializeField] private GameObject hostWaitingGroup;
    [SerializeField] private GameObject clientWaitingGroup;
    
    [SerializeField] private TMP_InputField newServerHostID;
    
    public void OnClickHostGame()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ActionOnClientConnected;
            
            hostWaitingGroup.gameObject.SetActive(true);
            serverBtnGroup.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Failed to Start Host");
        }
    }

    public void OnClickJoinGame()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            clientWaitingGroup.gameObject.SetActive(true);
            serverBtnGroup.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Failed to Start Client");
        }
    }

    public void OnDestroy()
    {
        if(NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= ActionOnClientConnected;
    }

    private void ActionOnClientConnected(ulong clientId)
    {
        Debug.Log("ActionOnClientConnected called client id : "+clientId);

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Connected Client is Host");
            return;
        }
        
        NetworkManager.Singleton.SceneManager.LoadScene("TikTacToeGameplay",LoadSceneMode.Single);
    }

    public void StopHostBtnClicked()
    {
        NetworkManager.Singleton.Shutdown();
        hostWaitingGroup.gameObject.SetActive(false);
        serverBtnGroup.gameObject.SetActive(true);
    }

    public void StopClientBtnClicked()
    {
        NetworkManager.Singleton.Shutdown();
        clientWaitingGroup.gameObject.SetActive(false);
        serverBtnGroup.gameObject.SetActive(true);
    }
    
}
