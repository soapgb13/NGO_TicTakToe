using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyPanel : MonoBehaviour
{
   [SerializeField] private TMP_InputField nameInputField;
   [SerializeField] private Button createLobbyButton;


   private void Start()
   {
      
   }

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

      int maxPlayer = 4;
      CreateLobbyOptions options = new CreateLobbyOptions();
      options.IsPrivate = false;
      try
      {
         Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, options);
         Debug.Log($"Created lobby {lobby.Name} ! Lobby code {lobby.LobbyCode}");

      }
      catch (Exception e)
      {
         Debug.LogError(e);
         throw;
      }
      
      
   }
   
}
