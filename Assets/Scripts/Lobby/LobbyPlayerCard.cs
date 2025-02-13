using Common;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyPlayerCard : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    
    Player _player;
    
    public void SetData(Player player,bool isHost)
    {
        _player = player;
        nameText.text = $"{player.Data[ConstKeys.PlayerName].Value}" + (isHost ? " : Host" : "");
    }
}
