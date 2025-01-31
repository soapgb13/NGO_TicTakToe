using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public enum TileOwnerType
{
    Empty = 0,
    Host = 1, // currently host
    Client = 2, // currently client
}

public class BoardManager : NetworkBehaviour
{
    private const int RequiredSequenceForWin = 3;
    
    [Header("Board Settings")]
    [SerializeField] private List<BoardTile> boardTiles = new List<BoardTile>();
    private Dictionary<Vector2Int, TileOwnerType> _tilesDict = new Dictionary<Vector2Int, TileOwnerType>();
    
    
    [Header("Player Settings")] 
    [SerializeField] private PlayersIcons hostIconPrefab;
    [SerializeField] private PlayersIcons clientIconPrefab;
    
    public NetworkVariable<TileOwnerType> currentTurn = new NetworkVariable<TileOwnerType>(TileOwnerType.Host,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    
    public static Action<TileOwnerType> OnCurrentTurnUpdated;
    public static Action<BoardTile> OnClickTile;
    public static Action<TileOwnerType,List<Vector2Int>> OnDeclareWinnerTiles;
    public static Action<TileOwnerType> OnGameOver;
    
    private bool isGameOver = false;

    private void Start()
    {
        foreach (var boardTile in boardTiles)
        {

            if (!_tilesDict.ContainsKey(boardTile.GetPosition()))
            {
                _tilesDict.Add(boardTile.GetPosition(),TileOwnerType.Empty);
            }
            else
            {
                Debug.LogError($"Board tile : { boardTile.gameObject.name } already occupied "+boardTile.GetPosition());
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn for board manager!");
        
        OnClickTile += OnCurrentTurnPlayerAction;
        
        currentTurn.OnValueChanged += OnTurnEndValueUpdated;

        if (IsHost || IsServer)
        {
            currentTurn.Value = TileOwnerType.Host;
            Debug.Log("OnNetworkSpawn set current turn!");
        }
        else
        {
            OnCurrentTurnUpdated?.Invoke(currentTurn.Value);
        }
        
        base.OnNetworkSpawn();
    }

    private void OnTurnEndValueUpdated(TileOwnerType oldValue, TileOwnerType newValue)
    {
        Debug.Log($"OnTurnEndValueUpdated value changed {oldValue} -> {newValue}!");
        OnCurrentTurnUpdated?.Invoke(newValue);
    }


    public override void OnDestroy()
    {
        OnClickTile -= OnCurrentTurnPlayerAction;
        base.OnDestroy();
    }


    private void OnCurrentTurnPlayerAction(BoardTile clickedTile)
    {
        if(isGameOver) return;
        
        if (clickedTile.GetCurrentOwner() != TileOwnerType.Empty)
        {
            //Debug.Log("Clicked On Already Occupied Tile");
            return;
        }

        if (currentTurn.Value == TileOwnerType.Host && !IsHost)
        {
            //Debug.Log("Not a Host wait for Host Turn");
            return;
        }

        if (currentTurn.Value == TileOwnerType.Client && IsHost)
        {
            //Debug.Log("Not a Client wait for Client Turn");
            return;
        }

        UpdateTileAndTurnDataRpc(clickedTile.GetPosition());
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateTileAndTurnDataRpc(Vector2Int clickedTile)
    {
        BoardTile boardTile = boardTiles.Find(k => k.GetPosition() == clickedTile);

        SpawnIconForPlayer(boardTile);

        CheckForCombination(clickedTile, currentTurn.Value);

        CheckForDraw();
        
        ChangeTurn();
    }

    private void SpawnIconForPlayer(BoardTile boardTile)
    {
        Debug.Log($"clickedTile {boardTile.GetPosition()} , value set {currentTurn.Value}");
        
        boardTile.SetCurrentOwner(currentTurn.Value);
        
        _tilesDict[boardTile.GetPosition()] = currentTurn.Value;
        
        if (currentTurn.Value == TileOwnerType.Host)
        {
            Instantiate(hostIconPrefab, boardTile.transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(clientIconPrefab, boardTile.transform.position, Quaternion.identity);
        }
    }
    
    private void ChangeTurn()
    {
        if(!IsHost) return;
        if(isGameOver) return;
        
        if (currentTurn.Value == TileOwnerType.Host)
        {
            currentTurn.Value = TileOwnerType.Client;
        }
        else
        {
            currentTurn.Value = TileOwnerType.Host;
        }
    }

    private void DeclareWinner(List<Vector2Int> winningTiles,TileOwnerType winnerName)
    {
        isGameOver = true;
        
        Debug.Log($"<color=red> Game Over! Winner {winnerName} , Found {winningTiles.Count} tiles! </color>");
        
        OnDeclareWinnerTiles?.Invoke(winnerName,winningTiles);

        StartCoroutine(_WaitAndShowGoToMainMenuPopup());
    }

    private IEnumerator _WaitAndShowGoToMainMenuPopup()
    {
        yield return new WaitForSeconds(3);
        OnGameOver?.Invoke(currentTurn.Value);
    }

    #region Code To Check board for win condition

    private void CheckForCombination(Vector2Int boardTile,TileOwnerType forPlayerToCheck)
    {
        List<Vector2Int> foundCombination = new List<Vector2Int>();
        
        //for horizontal check
        foundCombination = GetSequenceInDirection(boardTile, Vector2Int.right,forPlayerToCheck);
        if (foundCombination.Count >= RequiredSequenceForWin)
        {
            DeclareWinner(foundCombination,forPlayerToCheck);
            return;
        }
        
        //for vertical check
        foundCombination = GetSequenceInDirection(boardTile, Vector2Int.up,forPlayerToCheck);
        if (foundCombination.Count >= RequiredSequenceForWin)
        {
            DeclareWinner(foundCombination,forPlayerToCheck);
            return;
        }
        
        //for left to right diagonal check
        foundCombination = GetSequenceInDirection(boardTile, new Vector2Int(-1,1),forPlayerToCheck);
        if (foundCombination.Count >= RequiredSequenceForWin)
        {
            DeclareWinner(foundCombination,forPlayerToCheck);
            return;
        }
        
        //for right to left diagonal check
        foundCombination = GetSequenceInDirection(boardTile, new Vector2Int(1,1),forPlayerToCheck);
        if (foundCombination.Count >= RequiredSequenceForWin)
        {
            DeclareWinner(foundCombination,forPlayerToCheck);
            return;
        }
    }

    //only good for 3x3 board
    //this works but we can improve performance and ability to get all matched line
    private List<Vector2Int> GetSequenceInDirection(Vector2Int origin,Vector2Int direction ,TileOwnerType forPlayerToCheck )
    {
        var foundSequences = new List<Vector2Int> { origin };

        for (var i = 1; i <= RequiredSequenceForWin; i++)
        {
            var forwardTileToLookAt = origin + (direction * i);
            if (_tilesDict.ContainsKey(forwardTileToLookAt) && _tilesDict[forwardTileToLookAt] == forPlayerToCheck)
            {
                foundSequences.Add(forwardTileToLookAt);
            }
            
            var backWardTileToLookAt =  origin + (-direction * i);
            if (_tilesDict.ContainsKey(backWardTileToLookAt) && _tilesDict[backWardTileToLookAt] == forPlayerToCheck)
            {
                foundSequences.Add(backWardTileToLookAt);
            }

        }

        return foundSequences;
    }
    
    #endregion

    private void CheckForDraw()
    {
        if(isGameOver) return;

        bool isAnyEmpty = false;
        
        foreach (var tile in _tilesDict)
        {
            if (tile.Value == TileOwnerType.Empty)
            {
                isAnyEmpty = true;
                break;
            }
        }

        if (!isAnyEmpty)
        {
            isGameOver = true;
            OnGameOver?.Invoke(TileOwnerType.Empty);
        }
    }
}
