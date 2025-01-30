using System;
using TMPro;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ServerOptionPopup : MonoBehaviour
{

    [SerializeField] private TMP_InputField newServerHostID;
    
    public void OnClickHostGame()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.OnClientConnectedCallback += ActionOnClientConnected;
    }

    public void OnClickJoinGame()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void OnDestroy()
    {
        if(NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= ActionOnClientConnected;
    }

    private void ActionOnClientConnected(ulong clientId)
    {
        Debug.Log("Client connected "+clientId);
        NetworkManager.Singleton.SceneManager.LoadScene("TikTacToeGameplay",LoadSceneMode.Single);
    }
    
}
