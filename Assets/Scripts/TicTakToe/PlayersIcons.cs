using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayersIcons : MonoBehaviour
{
    
    [SerializeField] private SpriteRenderer emotionRenderer;
    [SerializeField] private Sprite normalSprite , winSprite , loseSprite;
    
    public TileOwnerType owner;
    private Vector2Int _sitPosition;


    public void Setup(Vector2Int sitPosition)
    {
        _sitPosition = sitPosition;
    }
    
    private void OnEnable()
    {
        BoardManager.OnDeclareWinnerTiles += IsInWinLine;
    }

    private void OnDestroy()
    {
        BoardManager.OnDeclareWinnerTiles -= IsInWinLine;
    }

    public void IsInWinLine(TileOwnerType winner, List<Vector2Int> winPositions)
    {
        if (owner != winner)
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
