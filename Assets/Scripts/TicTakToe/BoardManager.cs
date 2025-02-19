using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;



public class BoardManager : NetworkBehaviour
{
    public static BoardManager Instance;
    
    private const int RequiredSequenceForWin = 3;

    [Header("Board Settings")]
    [SerializeField] private List<BoardTile> boardTiles = new List<BoardTile>();
    private Dictionary<Vector2Int, string> _tilesDict = new Dictionary<Vector2Int, string>();

    [Header("Player Settings")]
    [SerializeField] private List<PlayersIcons> playersIcons = new List<PlayersIcons>();
   
    public string currentTurn => joinedPlayers[currentTurnIndex];
    
    private List<string> joinedPlayers = new List<string>();
    private int currentTurnIndex = 0;
    
    public bool isGameOver = false;
    public bool isQuitGame = false;

    
    
    public void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if(NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback += TerminateGameOnClientOrHostDisconnect;
    }
    
    public override void OnDestroy()
    {
        GameEvents.OnClickTile -= OnCurrentTurnPlayerAction;
        
        if(NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= TerminateGameOnClientOrHostDisconnect;
        
        base.OnDestroy();
    }
    
    private void TerminateGameOnClientOrHostDisconnect(ulong clientId)
    {
        GameEvents.TerminateGame?.Invoke(clientId);
    }

    private void Start()
    {
        GameManager.instance.AddPlayerDataToHost();
        InitializeBoard();
    }

    public override void OnNetworkSpawn()
    {
        GameEvents.OnClickTile += OnCurrentTurnPlayerAction;
        base.OnNetworkSpawn();
    }
    
    private void InitializeBoard()
    {
        foreach (var boardTile in boardTiles)
        {
            var position = boardTile.GetPosition();
            if (!_tilesDict.ContainsKey(position))
            {
                _tilesDict.Add(position, "");
            }
            else
            {
                Debug.LogError($"Board tile: {boardTile.gameObject.name} already occupied at {position}");
            }
        }
    }

    private void OnCurrentTurnPlayerAction(BoardTile clickedTile)
    {
        if (isGameOver || !IsValidMove(clickedTile)) return;

        UpdateTileAndTurnDataRpc(clickedTile.GetPosition());
    }

    private bool IsValidMove(BoardTile clickedTile)
    {
        if (!string.IsNullOrEmpty(clickedTile.GetCurrentOwner())) return false;
        
        if (currentTurn != GameManager.instance.PlayerID) return false;
        
        return true;
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateTileAndTurnDataRpc(Vector2Int clickedTile)
    {
        var boardTile = boardTiles.Find(k => k.GetPosition() == clickedTile);
        if (boardTile == null) return;

        SpawnIconForPlayer(boardTile);
        CheckForCombination(clickedTile, currentTurn);
        CheckForDraw();
        ChangeTurn();
    }

    private void SpawnIconForPlayer(BoardTile boardTile)
    {
        boardTile.SetCurrentOwner(currentTurn);
        _tilesDict[boardTile.GetPosition()] = currentTurn;
        
        var iconPrefab = playersIcons[currentTurnIndex] ;
        
        PlayersIcons spawnedIcon = Instantiate(iconPrefab, boardTile.transform.position, Quaternion.identity);
        spawnedIcon.Setup(boardTile.GetPosition(),currentTurn);
    }

    private void ChangeTurn()
    {
        if (!IsHost || isGameOver) return;

        MoveTurnToNextPlayerRpc();
    }

    private void DeclareWinner(List<Vector2Int> winningTiles, string winnerName)
    {
        isGameOver = true;

        Debug.Log($"<color=red> Game Over! Winner {winnerName}, Found {winningTiles.Count} tiles! </color>");
        GameEvents.OnDeclareWinnerTiles?.Invoke(winnerName, winningTiles);

        StartCoroutine(WaitAndShowGoToMainMenuPopup());
    }

    private IEnumerator WaitAndShowGoToMainMenuPopup()
    {
        yield return new WaitForSeconds(3);
        GameEvents.OnGameOver?.Invoke(currentTurn);
    }

    private void CheckForCombination(Vector2Int boardTile, string forPlayerToCheck)
    {
        if (CheckDirection(boardTile, Vector2Int.right, forPlayerToCheck) ||
            CheckDirection(boardTile, Vector2Int.up, forPlayerToCheck) ||
            CheckDirection(boardTile, new Vector2Int(-1, 1), forPlayerToCheck) ||
            CheckDirection(boardTile, new Vector2Int(1, 1), forPlayerToCheck))
        {
            return;
        }
    }

    private bool CheckDirection(Vector2Int origin, Vector2Int direction, string forPlayerToCheck)
    {
        var foundCombination = GetSequenceInDirection(origin, direction, forPlayerToCheck);
        if (foundCombination.Count >= RequiredSequenceForWin)
        {
            DeclareWinner(foundCombination, forPlayerToCheck);
            return true;
        }
        return false;
    }

    private List<Vector2Int> GetSequenceInDirection(Vector2Int origin, Vector2Int direction, string forPlayerToCheck)
    {
        var foundSequences = new List<Vector2Int> { origin };

        for (var i = 1; i <= RequiredSequenceForWin; i++)
        {
            AddTileToSequence(origin + (direction * i), forPlayerToCheck, foundSequences);
            AddTileToSequence(origin + (-direction * i), forPlayerToCheck, foundSequences);
        }

        return foundSequences;
    }

    private void AddTileToSequence(Vector2Int position, string forPlayerToCheck, List<Vector2Int> sequence)
    {
        if (_tilesDict.TryGetValue(position, out var owner) && owner == forPlayerToCheck)
        {
            sequence.Add(position);
        }
    }

    private void CheckForDraw()
    {
        if (isGameOver) return;

        foreach (var tile in _tilesDict.Values)
        {
            if (string.IsNullOrEmpty(tile)) return;
        }

        isGameOver = true;
        GameEvents.OnGameOver?.Invoke("");
    }

    private void SetInitialTurn()
    {
        currentTurnIndex = 0;
    }
    
    
    [Rpc(SendTo.Everyone)]
    public void MoveTurnToNextPlayerRpc()
    {
        if (currentTurnIndex >= joinedPlayers.Count - 1)
        {
            currentTurnIndex = 0;
        }
        else
        {
            currentTurnIndex++;
        }
        
        GameEvents.OnCurrentTurnUpdated?.Invoke(currentTurn);
    }

    [Rpc(SendTo.Everyone)]
    public void ActionOnAllPlayerRegisteredRpc()
    {
        joinedPlayers.Clear();
        joinedPlayers = GameManager.instance.GetJoinedPlayersList();
        GameEvents.OnCurrentTurnUpdated?.Invoke(currentTurn);
        GlobalUI.Singleton.HideLoadingScreen();
    }
}