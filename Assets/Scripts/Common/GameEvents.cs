using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static Action StartGame;
    public static  Action<string> OnCurrentTurnUpdated;
    public static Action<BoardTile> OnClickTile;
    public static  Action<string, List<Vector2Int>> OnDeclareWinnerTiles;
    public static  Action<string> OnGameOver;
    
    public static Action<ulong> TerminateGame;
}
