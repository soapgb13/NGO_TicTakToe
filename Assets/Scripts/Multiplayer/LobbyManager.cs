using System;
using System.Collections.Generic;
using Multiplayer;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject lobbyCreateAndFindUI;
    [SerializeField] private GameObject joinedLobbyUI;
    [SerializeField] private LobbyInfoPanel lobbyInfoPanel;

    private void OnEnable()
    {
        MultiplayerEvents.JoinLobby += OnPlayerJoinLobby;
        MultiplayerEvents.LeaveLobby += OnPlayerLeaveLobby;
    }
    
    private void OnDisable()
    {
        MultiplayerEvents.JoinLobby -= OnPlayerJoinLobby;
        MultiplayerEvents.LeaveLobby -= OnPlayerLeaveLobby;
    }


    private void OnPlayerJoinLobby(Lobby lobby)
    {
        Debug.Log("On Player Join Lobby Start");
        lobbyInfoPanel.AssignLobbyToPanel(lobby);
        ShowJoinedLobbyUI();
        lobbyInfoPanel.StartLobbyHeartBeats();
        Debug.Log("On Player Join Lobby End");
    }
    
    private void OnPlayerLeaveLobby()
    {
        lobbyInfoPanel.UnsubscribeFromLobbyEvents(ShowCreateLobbyUI);
    }

    public void ShowCreateLobbyUI()
    {
        Debug.Log("create lobby object on");
        lobbyCreateAndFindUI.SetActive(true);
        joinedLobbyUI.SetActive(false);
    }

    public void ShowJoinedLobbyUI()
    {
        Debug.Log("Joined lobby object on");
        joinedLobbyUI.SetActive(true);
        lobbyCreateAndFindUI.SetActive(false);
    }
}