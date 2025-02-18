using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Multiplayer;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyPanel : MonoBehaviour
{
   [SerializeField] private TMP_InputField nameInputField;
   [SerializeField] private Button createLobbyButton;

   

   public void OnClickCreateLobby()
   {
      Task createLobbyTask = CreateLobby();
      
   }
   
   
   private async Task CreateLobby()
   {
      string lobbyName = nameInputField.text;

      if (string.IsNullOrWhiteSpace(lobbyName) || string.IsNullOrEmpty(lobbyName))
      {
         Debug.LogError("Cant Have Empty Name");
         return;
      }
      
      try
      {
         int maxPlayer = 4;
         CreateLobbyOptions options = new CreateLobbyOptions();
         options.IsPrivate = false;

         Player player = new Player(AuthenticationService.Instance.PlayerId,joined:DateTime.Now)
         {
            Data = new Dictionary<string, PlayerDataObject>
            {
               { ConstKeys.PlayerName, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,AuthenticationService.Instance.PlayerName) },
               { ConstKeys.ReadyState, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,ConstKeys.NotReadyValue) }
            }
         };

         options.Player = player;
         
         Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, options);
         MultiplayerEvents.OnJoinLobby(lobby);
      }
      catch (Exception e)
      {
         Debug.LogError(e);
         throw;
      }
      
      
   }
   
}
