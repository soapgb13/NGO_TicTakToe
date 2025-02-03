using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static  Action<TileOwnerType> OnCurrentTurnUpdated;
    public static Action<BoardTile> OnClickTile;
    public static  Action<TileOwnerType, List<Vector2Int>> OnDeclareWinnerTiles;
    public static  Action<TileOwnerType> OnGameOver;
    
    public static Action<ulong> TerminateGame;
}
