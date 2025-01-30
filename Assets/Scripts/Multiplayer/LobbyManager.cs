using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private Dictionary<string, GameSession> gameSessions = new Dictionary<string, GameSession>();

    public void CreateGameSession(string sessionId)
    {
        if (!gameSessions.ContainsKey(sessionId))
        {
            GameSession newSession = new GameSession(sessionId);
            gameSessions.Add(sessionId, newSession);
            newSession.StartHost();
        }
    }

    public void JoinGameSession(string sessionId)
    {
        if (gameSessions.ContainsKey(sessionId))
        {
            gameSessions[sessionId].StartClient();
        }
    }
}

public class GameSession
{
    private string sessionId;
    private NetworkManager networkManager;

    public GameSession(string id)
    {
        sessionId = id;
        networkManager = new NetworkManager(); // Create a new instance or configure an existing one
    }

    public void StartHost()
    {
        networkManager.StartHost();
    }

    public void StartClient()
    {
        networkManager.StartClient();
    }
}