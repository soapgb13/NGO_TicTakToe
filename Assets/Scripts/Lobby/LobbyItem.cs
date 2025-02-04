using System.Threading.Tasks;
using TMPro;
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
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(_lobby.Id);
            Debug.Log("You Joined Lobby "+joinedLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
