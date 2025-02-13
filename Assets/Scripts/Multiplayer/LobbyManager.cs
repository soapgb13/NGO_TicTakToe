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
        lobbyInfoPanel.AssignLobbyToPanel(lobby);
        ShowJoinedLobbyUI();
        lobbyInfoPanel.StartLobbyHeartBeats();
    }
    
    private void OnPlayerLeaveLobby()
    {
        ShowCreateLobbyUI();
        lobbyInfoPanel.RemoveLobbyFromPanel();
    }

    public void ShowCreateLobbyUI()
    {
        lobbyCreateAndFindUI.SetActive(true);
        joinedLobbyUI.SetActive(false);
    }

    public void ShowJoinedLobbyUI()
    {
        joinedLobbyUI.SetActive(true);
        lobbyCreateAndFindUI.SetActive(false);
    }
}