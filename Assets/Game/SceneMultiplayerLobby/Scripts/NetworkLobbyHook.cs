using UnityEngine;
using Prototype.NetworkLobby;
using System.Collections;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook 
{
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        PlayerLobbyInfo player = gamePlayer.GetComponent<PlayerLobbyInfo>();

        player.playerName = lobby.playerName;
        player.playerColor = lobby.playerColor;
    }
}
