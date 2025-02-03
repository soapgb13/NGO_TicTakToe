using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum TileOwnerType
{
    Empty = 0,
    Host = 1,
    Client = 2,
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

    public NetworkVariable<TileOwnerType> currentTurn = new NetworkVariable<TileOwnerType>(
        TileOwnerType.Host, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // public static event Action<TileOwnerType> OnCurrentTurnUpdated;
    // public static Action<BoardTile> OnClickTile;
    // public static event Action<TileOwnerType, List<Vector2Int>> OnDeclareWinnerTiles;
    // public static event Action<TileOwnerType> OnGameOver;

    private bool isGameOver = false;

    private void Start()
    {
        InitializeBoard();
    }

    public override void OnNetworkSpawn()
    {
        GameEvents.OnClickTile += OnCurrentTurnPlayerAction;
        currentTurn.OnValueChanged += OnTurnEndValueUpdated;

        if (IsHost || IsServer)
        {
            SetInitialTurn();
        }
        else
        {
            GameEvents.OnCurrentTurnUpdated?.Invoke(currentTurn.Value);
        }

        base.OnNetworkSpawn();
    }

    private void OnTurnEndValueUpdated(TileOwnerType oldValue, TileOwnerType newValue)
    {
        GameEvents.OnCurrentTurnUpdated?.Invoke(newValue);
    }

    public override void OnDestroy()
    {
        GameEvents.OnClickTile -= OnCurrentTurnPlayerAction;
        currentTurn.OnValueChanged -= OnTurnEndValueUpdated;
        base.OnDestroy();
    }

    private void InitializeBoard()
    {
        foreach (var boardTile in boardTiles)
        {
            var position = boardTile.GetPosition();
            if (!_tilesDict.ContainsKey(position))
            {
                _tilesDict.Add(position, TileOwnerType.Empty);
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
        if (clickedTile.GetCurrentOwner() != TileOwnerType.Empty) return false;
        if (currentTurn.Value == TileOwnerType.Host && !IsHost) return false;
        if (currentTurn.Value == TileOwnerType.Client && IsHost) return false;
        return true;
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateTileAndTurnDataRpc(Vector2Int clickedTile)
    {
        var boardTile = boardTiles.Find(k => k.GetPosition() == clickedTile);
        if (boardTile == null) return;

        SpawnIconForPlayer(boardTile);
        CheckForCombination(clickedTile, currentTurn.Value);
        CheckForDraw();
        ChangeTurn();
    }

    private void SpawnIconForPlayer(BoardTile boardTile)
    {
        boardTile.SetCurrentOwner(currentTurn.Value);
        _tilesDict[boardTile.GetPosition()] = currentTurn.Value;

        var iconPrefab = currentTurn.Value == TileOwnerType.Host ? hostIconPrefab : clientIconPrefab;
        Instantiate(iconPrefab, boardTile.transform.position, Quaternion.identity);
    }

    private void ChangeTurn()
    {
        if (!IsHost || isGameOver) return;

        currentTurn.Value = currentTurn.Value == TileOwnerType.Host ? TileOwnerType.Client : TileOwnerType.Host;
    }

    private void DeclareWinner(List<Vector2Int> winningTiles, TileOwnerType winnerName)
    {
        isGameOver = true;

        Debug.Log($"<color=red> Game Over! Winner {winnerName}, Found {winningTiles.Count} tiles! </color>");
        GameEvents.OnDeclareWinnerTiles?.Invoke(winnerName, winningTiles);

        StartCoroutine(WaitAndShowGoToMainMenuPopup());
    }

    private IEnumerator WaitAndShowGoToMainMenuPopup()
    {
        yield return new WaitForSeconds(3);
        GameEvents.OnGameOver?.Invoke(currentTurn.Value);
    }

    private void CheckForCombination(Vector2Int boardTile, TileOwnerType forPlayerToCheck)
    {
        if (CheckDirection(boardTile, Vector2Int.right, forPlayerToCheck) ||
            CheckDirection(boardTile, Vector2Int.up, forPlayerToCheck) ||
            CheckDirection(boardTile, new Vector2Int(-1, 1), forPlayerToCheck) ||
            CheckDirection(boardTile, new Vector2Int(1, 1), forPlayerToCheck))
        {
            return;
        }
    }

    private bool CheckDirection(Vector2Int origin, Vector2Int direction, TileOwnerType forPlayerToCheck)
    {
        var foundCombination = GetSequenceInDirection(origin, direction, forPlayerToCheck);
        if (foundCombination.Count >= RequiredSequenceForWin)
        {
            DeclareWinner(foundCombination, forPlayerToCheck);
            return true;
        }
        return false;
    }

    private List<Vector2Int> GetSequenceInDirection(Vector2Int origin, Vector2Int direction, TileOwnerType forPlayerToCheck)
    {
        var foundSequences = new List<Vector2Int> { origin };

        for (var i = 1; i <= RequiredSequenceForWin; i++)
        {
            AddTileToSequence(origin + (direction * i), forPlayerToCheck, foundSequences);
            AddTileToSequence(origin + (-direction * i), forPlayerToCheck, foundSequences);
        }

        return foundSequences;
    }

    private void AddTileToSequence(Vector2Int position, TileOwnerType forPlayerToCheck, List<Vector2Int> sequence)
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
            if (tile == TileOwnerType.Empty) return;
        }

        isGameOver = true;
        GameEvents.OnGameOver?.Invoke(TileOwnerType.Empty);
    }

    private void SetInitialTurn()
    {
        currentTurn.Value = TileOwnerType.Host;
    }
}