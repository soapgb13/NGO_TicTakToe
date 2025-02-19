using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Multiplayer;
using TMPro;
using Unity.Netcode;
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
    [SerializeField] private Button startGameButton;

    [SerializeField] private Transform lobbyListContent;
    [Header("Prefabs")]
    [SerializeField] private LobbyPlayerCard lobbyPlayerCardPrefab;
    
    private Dictionary<string,LobbyPlayerCard> lobbyPlayerCards = new Dictionary<string, LobbyPlayerCard>();
    private List<string> lobbyPlayerIndex =  new List<string> ();
    
    private Lobby currentLobby;
    private LobbyEventCallbacks lobbyEventCallbacks;
    
    private bool isHostOfLobby = false;
    private Coroutine heartbeatCoroutine;
    private const float HeartbeatIntervalInSecond = 15f;
    private bool isReady = false;
    private ILobbyEvents subscriberHolder;


    public void AssignLobbyToPanel(Lobby lobby)
    {
        isReady = false;
        SetReadyStatusButtonView(isReady);
        
        currentLobby = lobby;

        isHostOfLobby = lobby.HostId == AuthenticationService.Instance.PlayerId;

        UpdateStartGameButton();
        
        UpdateLobbyInfo();

        CreateAlreadyJoinedPlayerCard();
        
        _ = SubscribeToLobbyEvents(lobby);
    }

    private void UpdateStartGameButton()
    {
        if (isHostOfLobby)
        {
            startGameButton.gameObject.SetActive(true);

            bool isOnlyOnePlayer = lobbyPlayerIndex.Count == 1;

            bool isAllPlayersReady = true;

            foreach (var player in currentLobby.Players)
            {
                if(player.Data.TryGetValue(ConstKeys.ReadyState, out var readyValue))
                {
                    if (readyValue.Value != ConstKeys.ReadyValue)
                    {
                        isAllPlayersReady = false;
                        break;
                    }
                }
            }
            
            startGameButton.interactable = !isOnlyOnePlayer && isAllPlayersReady;
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
        }
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
        lobbyPlayerIndex.Clear();

        for (int i = 0; i < currentLobby.Players.Count; i++)
        {
            Player player = currentLobby.Players[i];

            // Debug.Log($"Player Data Reading : {player.Id}");
            
            // foreach (var playerData in player.Data)
            // {
            //     Debug.Log($"player Data : {playerData.Key} : {playerData.Value.Value}");
            // }
            //
            // if (player.Id == AuthenticationService.Instance.PlayerId)
            // {
            //     Debug.Log($"Player index {i}.");
            // }
            
            LobbyPlayerCard newPlayerCard = Instantiate(lobbyPlayerCardPrefab, lobbyListContent);
            newPlayerCard.SetData(player, currentLobby.HostId == player.Id); 
            
            lobbyPlayerCards.Add(player.Id, newPlayerCard); 
            lobbyPlayerIndex.Add(player.Id);
        }
        
        UpdatePlayerCount();
    }

    private async Task SubscribeToLobbyEvents(Lobby lobby)
    {
        // Debug.Log("Subscribe lobby events.");
        try
        {
            lobbyEventCallbacks = new LobbyEventCallbacks(); 
            lobbyEventCallbacks.PlayerJoined += OnLobbyPlayerAdded;
            lobbyEventCallbacks.PlayerLeft += OnLobbyPlayerLeft;
            lobbyEventCallbacks.PlayerDataChanged += OnLobbyPlayerDataChanged;
            lobbyEventCallbacks.LobbyChanged += OnLobbyDataChanged;
            
            subscriberHolder = await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id,lobbyEventCallbacks);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to subscribe to lobby events: {e.Message}");
        }
    }

  

    public async void UnsubscribeFromLobbyEvents(Action onUnsubscribeSuccessAction)
    {
        // Debug.Log("Unsubscribing from lobby events.");
        
        try
        {
            await subscriberHolder.UnsubscribeAsync();
            
            lobbyEventCallbacks.PlayerJoined -= OnLobbyPlayerAdded;
            lobbyEventCallbacks.PlayerLeft -= OnLobbyPlayerLeft;
            lobbyEventCallbacks.PlayerDataChanged -= OnLobbyPlayerDataChanged;
            lobbyEventCallbacks.LobbyChanged -= OnLobbyDataChanged;

            lobbyEventCallbacks = null;
            
            onUnsubscribeSuccessAction?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
       
    }
    
    private void OnLobbyDataChanged(ILobbyChanges lobbyChanges)
    {
        try
        {
            lobbyChanges.ApplyToLobby(currentLobby);

            if (lobbyChanges.PlayerData.Changed || lobbyChanges.PlayerJoined.Changed || lobbyChanges.PlayerLeft.Changed)
            {
                UpdateStartGameButton();
            }

            if (lobbyChanges.Data.Changed)
            {
                JoinRelayFromLobbyCode();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }
    
    private void OnLobbyPlayerLeft(List<int> playerRemovedIndex)
    {
        for (int i = playerRemovedIndex.Count - 1; i >= 0 ; i--)
        {
            string id = lobbyPlayerIndex[playerRemovedIndex[i]];
            //Debug.Log("OnLobbyPlayerLeft called for "+id);

            if (lobbyPlayerCards.ContainsKey(id))
            {
                Destroy(lobbyPlayerCards[id].gameObject);
                lobbyPlayerCards.Remove(id);
                lobbyPlayerIndex.RemoveAt(playerRemovedIndex[i]);
                
                //Debug.Log($"OnLobbyPlayerLeft lobbyPlayerCards removed id {id} , lobbyPlayerIndex index {playerRemovedIndex[i]}");
            }
            else
            {
                Debug.LogError("Player index doesn't exist But you tried remove it from lobby , Index "+id);
            }
        }
        
        UpdatePlayerCount();
        CheckIfNeedToUpdateHostCardView();
    }
    
    private void OnLobbyPlayerAdded(List<LobbyPlayerJoined> newPlayers)
    {
        foreach (var newAddedPlayer in newPlayers)
        {
            //Debug.Log("OnLobbyPlayerAdded called for "+newAddedPlayer.Player.Id);
            
            LobbyPlayerCard newPlayerCard = Instantiate(lobbyPlayerCardPrefab, lobbyListContent);
            newPlayerCard.SetData(newAddedPlayer.Player,currentLobby.HostId == newAddedPlayer.Player.Id);
    
            if (!lobbyPlayerCards.ContainsKey(newAddedPlayer.Player.Id))
            {
                lobbyPlayerCards.Add(newAddedPlayer.Player.Id,newPlayerCard);
            }
            
            lobbyPlayerIndex.Add(newAddedPlayer.Player.Id);
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
            UpdateStartGameButton();
        }
    }

    private void UpdatePlayerCount()
    {
        joinedPlayerCountText.text = $"Player Count : {currentLobby.Players.Count}/{currentLobby.MaxPlayers}";
    }

    public void OnClickLeaveLobbyBtn()
    {
        LeaveLobbyAsync();
    }

    private async void LeaveLobbyAsync()
    {
        
        try
        {
            UpdatePlayerOptions updatedValue = new UpdatePlayerOptions();
            updatedValue.Data = new Dictionary<string, PlayerDataObject>();

            await LobbyService.Instance.UpdatePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId, updatedValue);
        }
        catch (Exception e)
        {
            Debug.LogError("Error Update On PLayer Leave Async"+e);
        }
        
        
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id,AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError("Error Leaving lobby :"+e);
            throw;
        }
        
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
        
        isReady = status;
        SetReadyStatusButtonView(isReady);
        
        // foreach (var player in currentLobby.Players)
        // {
        //     Debug.Log($"Player {player.Id}");
        //
        //     foreach (var lPlayerDataObject in player.Data)
        //     {
        //         Debug.Log($"Data {lPlayerDataObject.Key} : {lPlayerDataObject.Value.Value}");
        //     }
        // }
        
        try
        {
            UpdatePlayerOptions updatedValue = new UpdatePlayerOptions();
            updatedValue.Data = new Dictionary<string, PlayerDataObject>()
            {
                { ConstKeys.ReadyState, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, isReady ? ConstKeys.ReadyValue : ConstKeys.NotReadyValue) }
            };

            await LobbyService.Instance.UpdatePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId, updatedValue);
        }
        catch (Exception e)
        {
            Debug.LogError("Error Update PLayer Async"+e);
        }

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
                //Debug.Log($"OnLobbyPlayerDataChanged key {updatedValue.Key}");
                // foreach (var changes in updatedValue.Value)
                // {
                //     Debug.Log($"OnLobbyPlayerDataChanged data : key {changes.Key} : value {changes.Value.Value.Value}");
                // }
                
                if (updatedValue.Value.TryGetValue(ConstKeys.ReadyState, out var readyValue))
                {
                    bool isReady = readyValue.Value.Value == ConstKeys.ReadyValue;
                    lobbyPlayerCards[ lobbyPlayerIndex[updatedValue.Key]].ReadyStatus(isReady);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("OnLobbyPlayerDataChanged Error :"+e);
        }
    }

    public void OnClickStartGame()
    {
        startGameButton.interactable = false;
        CreateRelayCodeAndStartGame();
    }

    private async void CreateRelayCodeAndStartGame()
    {

        string newLobbyCode = await RelayManager.instance.CreateRelay(currentLobby.Players.Count);

        if (newLobbyCode == "0")
        {
            Debug.LogError("Cant Create Relay Code");
            startGameButton.interactable = true;
            return;
        }
        
        UpdateLobbyOptions newLobbyOptions = new UpdateLobbyOptions();
        newLobbyOptions.Data = new Dictionary<string, DataObject>()
        {
            { ConstKeys.LobbyRelayCodeKey, new DataObject(DataObject.VisibilityOptions.Member, newLobbyCode) }
        };

        try
        {
            GameManager.instance.WaitForOtherClients();
            GameManager.instance.SetPlayerData(currentLobby);

            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id,newLobbyOptions);
        
            startGameButton.interactable = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    private async void JoinRelayFromLobbyCode()
    {
        if(isHostOfLobby) return;
        
        if (!currentLobby.Data.TryGetValue(ConstKeys.LobbyRelayCodeKey, out DataObject dataObject))
        {
            Debug.LogError("Cant Parse Lobby Data");
            return;
        }

        string newLobbyCode = dataObject.Value;
        
        if (newLobbyCode == "0")
        {
            Debug.LogError("Cant Create Relay Code");
            startGameButton.interactable = true;
            return;
        }

        bool isInRelay = await RelayManager.instance.JoinRelay(newLobbyCode);

        Debug.Log("Is in Relay "+isInRelay);
        
        if (isInRelay)
        {
            try
            {
               NetworkManager.Singleton.StartClient();
               GameManager.instance.SetPlayerData(currentLobby: currentLobby);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
    }

}
