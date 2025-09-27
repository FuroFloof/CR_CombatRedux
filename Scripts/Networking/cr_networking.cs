using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fusion;
using Fusion.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class cr_networking : cr_NetworkBehavior, INetworkRunnerCallbacks
{

    public static cr_networking Instance { get; private set; }
    
    private static NetworkRunner _fusionRunner;
    public static NetworkRunner fusionRunner
    {
        get
        {
            if (_fusionRunner == null)
            {
                _fusionRunner = FindObjectOfType<NetworkRunner>();
            }
            
            return _fusionRunner;
        }
    }
    
    
    private static cr_player_spawner _playerSpawner;
    public static cr_player_spawner playerSpawner
    {
        get
        {
            if(_playerSpawner == null)
            {
                _playerSpawner = FindObjectOfType<cr_player_spawner>();
            }
            
            return _playerSpawner;
        }
    }

    private static cr_MatchBehavior _matchBehavior;
    public static cr_MatchBehavior matchBehavior
    {
        get
        {
            if(_matchBehavior == null)
            {
                _matchBehavior = FindObjectOfType<cr_MatchBehavior>();
            }

            return _matchBehavior;
        }
    }

    public string NetworkingRegion
    {
        get
        {
            var serverConfigFilePath = Path.Combine(cr_game.CustomGameConfigPath, "server.cfg");
            string region = cr_game_file_parser.Instance.GetFileVar<string>(serverConfigFilePath, "sv_game_region", "eu");
            
            return region;
        }
    }
    
    

    void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        
        Instance = this;
    }

    
    void Start()
    {
        
    }
    
    [ContextMenu("Connect/Join Or Host")]
    // public async void DEV_JoinOrCreateDevRoom()
    // {
    //     // await fusionRunner.StartGame(new StartGameArgs()
    //     // {
    //     //     GameMode = GameMode.AutoHostOrClient,
    //     //     SessionName = "dev",
    //     //     Scene = null,
    //     //     SceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>()

    //     // });
        
    //     //Scene play began? idk...
    // }
    
    public void Bind(NetworkRunner runner)
    {
        runner.AddCallbacks(this);
    }
    
    public static cr_player_api localPlayer
    {
        get
        {
            if (playerSpawner == null) return null;
            if (fusionRunner == null) return null;
        
            var playerObject = playerSpawner.GetPlayerObjectByPlayerRef(fusionRunner.LocalPlayer);
            
            if(playerObject == null) return null;
            
            var playerAPI = playerObject.GetComponent<cr_player_api>();
            
            if (!playerAPI) return null;
            
            return playerAPI;
        }
    }
    
    public cr_player_api GetPlayerByID(int id)
    {
        var playerObject = playerSpawner.GetPlayerObjectByPlayerId(id);
        
        if(!playerObject) return null;
            
        var playerAPI = playerObject.GetComponent<cr_player_api>();
        
        if (!playerAPI) return null;
        
        return playerAPI;
    }
    
    public cr_player_api[] GetAllPlayers()
    {
        var players = playerSpawner.GetAllPlayerObjects();

        cr_player_api[] playerApis = new cr_player_api[players.Length];
        //List<cr_player_api> playerApis = new List<cr_player_api>();
        
        for(int i = 0; i < playerApis.Length; i++)
        {
            if (players[i] == null) continue;

            //var playerAPI = players[i].GetComponent<cr_player_api>();
            cr_player_api playerAPI = null;
            
            if(players[i].gameObject.TryGetComponent<cr_player_api>(out var api))
            {
                playerAPI = api;
            }
            
            if(playerAPI == null) continue;
            
            playerApis[i] = playerAPI;
        }
        
        return playerApis;
    }

    new public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    new public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    new public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        playerSpawner.OnPlayerJoined(runner, player);
    }

    new public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        playerSpawner.OnPlayerLeft(runner, player);
    }

    new public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        playerSpawner.OnShutdown(runner, shutdownReason);
    }

    new public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        playerSpawner.OnDisconnectedFromServer(runner, reason);
    }

    new public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    new public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    new public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }

    new public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    new public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    new public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
    }

    new public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    new public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    new public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    new public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    new public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    new public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    new public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }
}