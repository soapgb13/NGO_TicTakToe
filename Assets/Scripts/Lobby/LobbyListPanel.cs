using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyListPanel : MonoBehaviour
{
    [SerializeField] private LobbyItem lobbyItemPrefab;
    [SerializeField] private Transform spawnContent;

    private List<LobbyItem> spawnedLobbies = new List<LobbyItem>();

    // private void OnEnable()
    // {
    //     _ = RefreshLobbies();
    // }


    [ContextMenu("Refresh lobbies")]
    public void RefreshLobbiesListBtnClicked()
    {
        _ = RefreshLobbies();
    }

    private async Task RefreshLobbies()
    {
        try
        {

            foreach (var preLobby in spawnedLobbies)
            {
                Destroy(preLobby.gameObject);
            }

            QueryLobbiesOptions options = new QueryLobbiesOptions();

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            foreach (var lobby in lobbies.Results)
            {
                LobbyItem newLobby = Instantiate(lobbyItemPrefab, spawnContent);
                newLobby.SetLobby(lobby, this);
                spawnedLobbies.Add(newLobby);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void RemoveLobby(LobbyItem lobbyToRemove)
    {
        spawnedLobbies.Remove(lobbyToRemove);
        Destroy(lobbyToRemove.gameObject);
    }
    
}
