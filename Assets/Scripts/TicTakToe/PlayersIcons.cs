using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayersIcons : MonoBehaviour
{
    
    [SerializeField] private SpriteRenderer emotionRenderer;
    [SerializeField] private Sprite normalSprite , winSprite , loseSprite;
    
    private string _owner;
    private Vector2Int _sitPosition;


    public void Setup(Vector2Int sitPosition,string owner)
    {
        _owner = owner;
        _sitPosition = sitPosition;
    }
    
    private void OnEnable()
    {
        GameEvents.OnDeclareWinnerTiles += IsInWinLine;
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.4f).SetEase(Ease.OutBounce);
    }

    private void OnDestroy()
    {
        GameEvents.OnDeclareWinnerTiles -= IsInWinLine;
    }

    public void IsInWinLine(string winner, List<Vector2Int> winPositions)
    {
        if (_owner != winner)
        {
            emotionRenderer.sprite = loseSprite;
        }
        else
        {
            foreach (var oneOfWinPosition in winPositions)
            {
                if (oneOfWinPosition.x == _sitPosition.x && oneOfWinPosition.y == _sitPosition.y)
                {
                    emotionRenderer.sprite = winSprite;
                    break;
                }
            }
        }
    }
}
