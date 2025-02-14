using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Multiplayer;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInfoPanel : MonoBehaviour
{

    [Header("UI Elements")]
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text joinedPlayerCountText;
    [SerializeField] private TMP_Text lobbyCodeText;
    [SerializeField] private Button readyStatusButton;
    [SerializeField] private Sprite readySprite , notReadySprite;
    [SerializeField] private TMP_Text readyStatusText;
    
    [SerializeField] private Transform lobbyListContent;
    [Header("Prefabs")]
    [SerializeField] private LobbyPlayerCard lobbyPlayerCardPrefab;
    
    private Dictionary<int,LobbyPlayerCard> lobbyPlayerCards = new Dictionary<int, LobbyPlayerCard>();
    
    private Lobby currentLobby;
    private LobbyEventCallbacks lobbyEventCallbacks;
    
    private bool isHostOfLobby = false;
    private Coroutine heartbeatCoroutine;
    private const float HeartbeatIntervalInSecond = 15f;
    private bool isReady = false;
    
    public void AssignLobbyToPanel(Lobby lobby)
    {
        SetReadyStatusButtonView(false);
        
        currentLobby = lobby;

        isHostOfLobby = lobby.HostId == AuthenticationService.Instance.PlayerId;
        
        UpdateLobbyInfo();

        CreateAlreadyJoinedPlayerCard();
        
        _ = SubscribeToLobbyEvents(lobby);
    }

    public void RemoveLobbyFromPanel()
    {
        UnsubscribeFromLobbyEvents();
    }
    

    public void StartLobbyHeartBeats()
    {
        if (isHostOfLobby)
        {
            if (heartbeatCoroutine != null)
            {
                StopCoroutine(heartbeatCoroutine);
            }
            
            heartbeatCoroutine = StartCoroutine(LobbyHeartBeat());
        }
    }

    private IEnumerator LobbyHeartBeat()
    {
        YieldInstruction waitAfterSendBeat = new WaitForSeconds(HeartbeatIntervalInSecond);
        while (isHostOfLobby)
        {
            yield return waitAfterSendBeat;
            yield return LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
        }
    }

    private void CreateAlreadyJoinedPlayerCard()
    {
        if (currentLobby == null)
        {
            Debug.LogError("No lobby assigned to panel.");
            return;
        }

        // Clear previous player cards to avoid duplication
        foreach (var card in lobbyPlayerCards.Values)
        {
            Destroy(card.gameObject);
        }
        lobbyPlayerCards.Clear();

        for (int i = 0; i < currentLobby.Players.Count; i++)
        {
            Player player = currentLobby.Players[i];

            if (player.Id == AuthenticationService.Instance.PlayerId)
            {
                Debug.Log($"Player index {i}.");
            }
            
            LobbyPlayerCard newPlayerCard = Instantiate(lobbyPlayerCardPrefab, lobbyListContent);
            newPlayerCard.SetData(player, currentLobby.HostId == player.Id); 

            lobbyPlayerCards.Add(i, newPlayerCard); 
        }
        
        UpdatePlayerCount();
    }

    private async Task SubscribeToLobbyEvents(Lobby lobby)
    {
        try
        {
            lobbyEventCallbacks = new LobbyEventCallbacks(); 
            lobbyEventCallbacks.PlayerJoined += OnLobbyPlayerAdded;
            lobbyEventCallbacks.PlayerLeft += OnLobbyPlayerLeft;
            lobbyEventCallbacks.PlayerDataChanged += OnLobbyPlayerDataChanged;
            lobbyEventCallbacks.LobbyChanged += OnLobbyDataChanged;
            
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id,lobbyEventCallbacks);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to subscribe to lobby events: {e.Message}");
        }
    }

  

    public void UnsubscribeFromLobbyEvents()
    {
        lobbyEventCallbacks.PlayerJoined -= OnLobbyPlayerAdded;
        lobbyEventCallbacks.PlayerLeft -= OnLobbyPlayerLeft;
        lobbyEventCallbacks.PlayerDataChanged -= OnLobbyPlayerDataChanged;
        lobbyEventCallbacks.LobbyChanged -= OnLobbyDataChanged;

        lobbyEventCallbacks = null;
    }
    
    private void OnLobbyDataChanged(ILobbyChanges lobbyChanges)
    {
        lobbyChanges.ApplyToLobby(currentLobby);
    }
    
    private void OnLobbyPlayerLeft(List<int> playerRemovedIndex)
    {
        foreach (var playerIndex in playerRemovedIndex)
        {
            if (lobbyPlayerCards.ContainsKey(playerIndex))
            {
                Destroy(lobbyPlayerCards[playerIndex].gameObject);
                lobbyPlayerCards.Remove(playerIndex);
            }
            else
            {
                Debug.LogError("Player index doesn't exist But you tried remove it from lobby , Index "+playerIndex);
            }
        }
        
        UpdatePlayerCount();
        CheckIfNeedToUpdateHostCardView();
    }
    
    private void OnLobbyPlayerAdded(List<LobbyPlayerJoined> newPlayers)
    {
        foreach (var newAddedPlayer in newPlayers)
        {
            LobbyPlayerCard newPlayerCard = Instantiate(lobbyPlayerCardPrefab, lobbyListContent);
            newPlayerCard.SetData(newAddedPlayer.Player,currentLobby.HostId == newAddedPlayer.Player.Id);
    
            if (!lobbyPlayerCards.ContainsKey(newAddedPlayer.PlayerIndex))
            {
                lobbyPlayerCards.Add(newAddedPlayer.PlayerIndex,newPlayerCard);
            }
        }
    
        UpdatePlayerCount();
    }

    private void UpdateLobbyInfo()
    {
        lobbyNameText.text = "Lobby Name :"+currentLobby.Name;
        lobbyCodeText.text = "Lobby Code :"+currentLobby.LobbyCode;
        UpdatePlayerCount();
    }

    private void CheckIfNeedToUpdateHostCardView()
    {
        foreach (var card in lobbyPlayerCards)
        {
            if (currentLobby.HostId == card.Value.GetPlayer().Id)
            {
                card.Value.UpdateHostStatus(true);
                break;
            }
        }
        
        if (currentLobby.HostId == AuthenticationService.Instance.PlayerId && !isHostOfLobby)
        {
            isHostOfLobby = true;
            StartLobbyHeartBeats();
        }
    }

    private void UpdatePlayerCount()
    {
        joinedPlayerCountText.text = $"Player Count : {currentLobby.Players.Count}/{currentLobby.MaxPlayers}";
    }

    public void OnClickLeaveLobbyBtn()
    {
        LobbyService.Instance.RemovePlayerAsync(currentLobby.Id,AuthenticationService.Instance.PlayerId);
        
        if (isHostOfLobby)
        {
            StopCoroutine(heartbeatCoroutine);
            isHostOfLobby = false;
        }
        
        MultiplayerEvents.OnLeaveLobby();
    }

    public void OnClickToggleStatusBtn()
    {
        readyStatusButton.interactable = false;
        SetPlayerReadyStatus(!isReady);
    }

    private async void SetPlayerReadyStatus(bool status)
    {
        try
        {
            isReady = status;
            SetReadyStatusButtonView(isReady);

            UpdatePlayerOptions updatedValue = new UpdatePlayerOptions();
            updatedValue.Data = new Dictionary<string, PlayerDataObject>()
            {
                { ConstKeys.ReadyState, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, isReady ? ConstKeys.ReadyValue : ConstKeys.NotReadyValue) }
            };
            
            await LobbyService.Instance.UpdatePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId, updatedValue);

            foreach (var card in lobbyPlayerCards)
            {
                if (card.Value.GetPlayer().Id == AuthenticationService.Instance.PlayerId)
                {
                    card.Value.ReadyStatus(isReady);
                    break;
                }
            }
            
            readyStatusButton.interactable = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void SetReadyStatusButtonView(bool status)
    {

        if (isReady)
        {
            readyStatusButton.image.sprite = readySprite;
            readyStatusText.text = "Ready";
        }
        else
        {
            readyStatusButton.image.sprite = notReadySprite;
            readyStatusText.text = "Not Ready";
        }
    }
    
    //need to implement this
    private void OnLobbyPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> updatedPlayers)
    {
        try
        {
            foreach (var updatedValue in updatedPlayers)
            {
                //for updating ready status
                if (updatedValue.Value.TryGetValue(ConstKeys.ReadyState, out var readyValue))
                {
                    bool isReady = readyValue.Value.Value == ConstKeys.ReadyValue;
                    lobbyPlayerCards[updatedValue.Key].ReadyStatus(isReady);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

}
