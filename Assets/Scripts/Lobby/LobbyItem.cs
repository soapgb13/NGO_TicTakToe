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
    private LobbyListPanel _lobbyListPanel;

    public void SetLobby(Lobby lobby,LobbyListPanel lobbyListPanel)
    {
        _lobby = lobby;
        _lobbyListPanel = lobbyListPanel;
        lobbyNameTxt.text = _lobby.Name;
    }
    
    public void OnClickJoinBtn()
    {
        // Debug.Log("OnClickJoinBtn called for lobby "+_lobby.Name);
        _ = JoinLobby();
    }

    private async Task JoinLobby()
    {
        try
        {

            Lobby clickedLobby = await LobbyService.Instance.GetLobbyAsync(_lobby.Id);
            if (clickedLobby == null)
            {
                Debug.LogError("Lobby is null , Removing it");
                _lobbyListPanel.RemoveLobby(this);
                return;
            }
            
            if (clickedLobby.Players.Count >= clickedLobby.MaxPlayers)
            {
                Debug.LogError("Max players Already in Lobby Cant join!");
                return;
            }
            
            _lobby = clickedLobby;
            
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
            
            // Debug.Log("OnClickJoinBtn JoinLobby "+_lobby.Name+" with Player Data");
            
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(_lobby.Id,options);
            MultiplayerEvents.OnJoinLobby(joinedLobby);
            
            // Debug.Log("OnClickJoinBtn JoinLobby Success");
        }
        catch (Exception e)
        {
            Debug.LogError("Error Joining Lobby :"+e);
            LobbyServiceException lobbyServiceException = e as LobbyServiceException;
            Debug.LogError("Lobby Join Fail Reason "+lobbyServiceException.Reason);
        }
    }
    
    
}
