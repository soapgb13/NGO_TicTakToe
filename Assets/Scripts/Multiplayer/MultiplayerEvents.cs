using System;
using Unity.Services.Lobbies.Models;

namespace Multiplayer
{
    public static class MultiplayerEvents
    {
        public static event Action<Lobby> JoinLobby;

        public static void OnJoinLobby(Lobby joinedLobby)
        {
            JoinLobby?.Invoke(joinedLobby);
        }
        
        public static event Action LeaveLobby;

        public static void OnLeaveLobby()
        {
            LeaveLobby?.Invoke();
        }
    }
}