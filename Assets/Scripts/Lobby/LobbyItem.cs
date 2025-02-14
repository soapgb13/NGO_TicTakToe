using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Multiplayer;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyNameTxt;

    private Lobby _lobby;

    public void SetLobby(Lobby lobby)
    {
        _lobby = lobby;
        lobbyNameTxt.text = _lobby.Name;
    }
    
    public void OnClickJoinBtn()
    {
        _ = JoinLobby();
    }

    private async Task JoinLobby()
    {
        try
        {
            
            Player player = new Player(AuthenticationService.Instance.PlayerId,joined:DateTime.Now)
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { ConstKeys.PlayerName, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,AuthenticationService.Instance.PlayerName) },
                    { ConstKeys.ReadyState, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,ConstKeys.NotReadyValue) }
                }
            };
            
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();
            options.Player = player;
            
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(_lobby.Id,options);
            MultiplayerEvents.OnJoinLobby(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }
}
