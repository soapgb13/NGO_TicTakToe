using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    private Dictionary<string,string> playersIDToNameDict = new Dictionary<string, string>();
    
    private int totalPlayers , connectedClients;
    private string _playerName;
    public string PlayerName { get => _playerName; }
    
    private string _playerID;
    public string PlayerID { get => _playerID; }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerData(Lobby currentLobby)
    {
        _playerID = AuthenticationService.Instance.PlayerId;
        _playerName = AuthenticationService.Instance.PlayerName;
        totalPlayers = currentLobby.Players.Count;
    }

    public void AddPlayerDataToHost()
    {
        try
        {
            // Debug.Log("PlayerID: " + _playerID+" Name: " + _playerName+" SetPlayerIdAndNameInHostDictRPC calling");
            SetPlayerIdAndNameInHostDictRPC(_playerID, _playerName,_playerName);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    public void WaitForOtherClients()
    {
        try
        {
            connectedClients = 1;
            NetworkManager.Singleton.StartHost();
            playersIDToNameDict.Clear();
            NetworkManager.Singleton.OnClientConnectedCallback += ActionOnClientConnected;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
        
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= ActionOnClientConnected;
        }
    }


    [Rpc(SendTo.Server)]
    private void SetPlayerIdAndNameInHostDictRPC(string playerID, string playerName,string senderID)
    {
        if (!playersIDToNameDict.ContainsKey(playerID))
        {
            playersIDToNameDict.Add(playerID, playerName);
            // Debug.Log($"Added Player {playerID} Name {playerName}, Dict size {playersIDToNameDict.Count} , senderID : {senderID}");
            SendDictionary(playersIDToNameDict,senderID);
            if (playersIDToNameDict.Count == totalPlayers)
            {
                BoardManager.Instance.ActionOnAllPlayerRegisteredRpc();
            }
        }
        else
        {
            Debug.LogError($"Player ID {playerID} already present in dictionary , me : {_playerName}");
        }
    }
    
    [Rpc(SendTo.NotServer)]
    public void SendDictionaryServerRpc(NetworkSerializableDictionary dictWrapper,string senderID)
    {
        // Debug.Log($"server sent updated Dict count {dictWrapper.Dictionary.Count}, local dict count {playersIDToNameDict.Count}");
        playersIDToNameDict = dictWrapper.Dictionary;
    }

    public void SendDictionary(Dictionary<string, string> dict,string senderID)
    {
        try
        {
            var wrapper = new NetworkSerializableDictionary { Dictionary = dict };
            SendDictionaryServerRpc(wrapper,senderID);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }
    
    private void ActionOnClientConnected(ulong clientId)
    {
        // Debug.Log("ActionOnClientConnected called client id : "+clientId);

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Connected Client is Host");
            return;
        }
        
        connectedClients++;

        if (connectedClients == totalPlayers)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("TikTacToeGameplay",LoadSceneMode.Single);
        }
    }
    
    public List<string> GetJoinedPlayersList()
    {
        return playersIDToNameDict.Keys.ToList();
    }

    public string GetNameFromID(string playerID)
    {
      return  playersIDToNameDict[playerID];
    }
    
}
