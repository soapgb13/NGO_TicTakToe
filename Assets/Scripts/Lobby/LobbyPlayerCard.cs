using Common;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerCard : MonoBehaviour
{
    [Header("Tab UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image tabImage;
    [SerializeField] private Sprite defaultSprite , hostSprite;
    
    [Header("Ready UI")]
    [SerializeField] private Image readyImage;
    [SerializeField] private Sprite readySprite, notReadySprite;
    
    Player _player;
    bool _isHost;
    bool _isReady;
    
    public void SetData(Player player,bool isHost)
    {
        _player = player;
        nameText.text = $"{player.Data[ConstKeys.PlayerName].Value}";
        _isReady = player.Data[ConstKeys.ReadyState].Value == ConstKeys.ReadyValue; 
        
        _isHost = isHost;
        UpdateView();
        ReadyStatus(_isReady);
    }

    private void UpdateView()
    {
        if (_isHost)
        {
            tabImage.sprite = hostSprite;
        }
        else
        {
            tabImage.sprite = defaultSprite;
        }
    }

    public void ReadyStatus(bool isReady)
    {
        _isReady = isReady;
        readyImage.sprite = isReady ? readySprite : notReadySprite;
    }
    
    public Player GetPlayer() => _player;

    public void UpdateHostStatus(bool isHost)
    {
        _isHost = isHost;
        UpdateView();
    }
}
