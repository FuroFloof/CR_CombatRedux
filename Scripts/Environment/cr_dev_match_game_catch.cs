using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;

public class cr_dev_match_game_catch : cr_NetworkBehavior
{
    private static cr_dev_match_game_catch _instance;
    public static cr_dev_match_game_catch Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<cr_dev_match_game_catch>();
            }
            
            return _instance;
        }
    }
    
    public int maxPointsTillWin = 50;
    public TextMeshProUGUI[] leaderboard;
    public cr_dev_match_game_catch_zone_fx[] captureZones;
    public Transform captureZoneRoot;

    [Networked, Capacity(cr_game.SV_MAX_PLAYERS), OnChangedRender(nameof(OnPointsChange))]
    public NetworkDictionary<int, int> Points { get; }
    
    [Networked, Capacity(cr_game.SV_MAX_PLAYERS), OnChangedRender(nameof(OnWinsChange))]
    public NetworkDictionary<int, int> PlayerWins { get; }
    
    [Networked, OnChangedRender(nameof(OnItChanged))]
    public int CurrentIt { get; set; }
    
    [Networked]
    public bool GameStalled { get; set; }

    [Networked]
    public float DefaultThrusterSpeed { get; set; }
    
    [Networked]
    public float ItThrusterSpeed { get; set; }
    
    public List<int> playersInCaptureZones = new List<int>();
    
    
    
    void Update()
    {
        if (!Object) return;
        if (!Object.IsValid) return;
        if (Object.HasStateAuthority) StateAuthorityTick();

        CommonPlayerTick();
    }

    void Start()
    {   
        
    }
    
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            ResetPoints();
            ResetWins();
        }

        RefreshPlayerList();
    
        ApplyFromConfigFile();
    }
    
    public void ApplyFromConfigFile()
    {
        var serverConfigPath = Path.Combine(cr_game.CustomGameConfigPath, "server.cfg");

        float v_matchTimeout = cr_game_file_parser.Instance.GetFileVar<float>(
            serverConfigPath,
            nameof(cr_game_defaults_dev_catch.m_match_timeout),
            cr_game_defaults_dev_catch.m_match_timeout
        );
        int v_pointsPerCycle = cr_game_file_parser.Instance.GetFileVar<int>(
            serverConfigPath, 
            nameof(cr_game_defaults_dev_catch.m_points_per_cycle),
            cr_game_defaults_dev_catch.m_points_per_cycle
        );
        float v_cycleTimer = cr_game_file_parser.Instance.GetFileVar<float>(
            serverConfigPath,
            nameof(cr_game_defaults_dev_catch.m_cycle_timer),
            cr_game_defaults_dev_catch.m_cycle_timer
        );
        float v_zoneCycleTimer = cr_game_file_parser.Instance.GetFileVar<float>(
            serverConfigPath, 
            nameof(cr_game_defaults_dev_catch.m_zone_cycle_timer),
            cr_game_defaults_dev_catch.m_zone_cycle_timer
        );
        int v_pointsTillWin = cr_game_file_parser.Instance.GetFileVar<int>(
            serverConfigPath, 
            nameof(cr_game_defaults_dev_catch.m_points_till_win),
            cr_game_defaults_dev_catch.m_points_till_win
        );

        float v_defaultThrusterSpeed = cr_game_file_parser.Instance.GetFileVar<float>(
            serverConfigPath,
            nameof(cr_game_defaults.p_thrusters_max_speed),
            cr_game_defaults.p_thrusters_max_speed
        );
        
        float v_itThrusterSpeed = cr_game_file_parser.Instance.GetFileVar<float>(
            serverConfigPath,
            nameof(cr_game_defaults_dev_catch.m_it_thrusters_speed),
            cr_game_defaults_dev_catch.m_it_thrusters_speed
        );

        //APPLY

        matchTimeout = v_matchTimeout;
        PointsPerCycle = v_pointsPerCycle;
        CycleTimer = v_cycleTimer;
        ZoneCycleTimer = v_zoneCycleTimer;
        maxPointsTillWin = v_pointsTillWin;
        
        if(Object.HasStateAuthority)
        {
            DefaultThrusterSpeed = v_defaultThrusterSpeed;
            ItThrusterSpeed = v_itThrusterSpeed;   
        }
    }
    
    

    public float matchTimeout = cr_game_defaults_dev_catch.m_match_timeout;

    public int PointsPerCycle = cr_game_defaults_dev_catch.m_points_per_cycle;
    public float CycleTimer = cr_game_defaults_dev_catch.m_cycle_timer;
    public float CurrentCycleTimer = 0;

    public float ZoneCycleTimer = cr_game_defaults_dev_catch.m_zone_cycle_timer;
    public float CurrentZoneCycleTimer = 0;
    public void StateAuthorityTick()
    {//runs on server host, put game logic in here

        float dt = Time.deltaTime;
        
        if(CurrentCycleTimer >= CycleTimer)
        {
            CurrentCycleTimer = 0;
            RewardAllPlayersInZones();
        }else
        {
            CurrentCycleTimer += dt;
        }
        
        if(CurrentZoneCycleTimer >= ZoneCycleTimer)
        {
            CurrentZoneCycleTimer = 0;
            SpreadZones();
        }else
        {
            CurrentZoneCycleTimer += dt;
        }
        
        if(CurrentIt == 0)
        {
            PickRandomNewIT();
        }
    }

    
    public void PickRandomNewIT()
    {
        int randomPlayerID = Random.Range(0, playersInMatch.Length);
        
        int newIT = playersInMatch[randomPlayerID].playerId;
        
        CurrentIt = newIT;
    }
    
    public void UnStallGame()
    {
        GameStalled = false;
    }
    
    public void PlayerReachedMaxPoints(int playerID)
    {
        if (!HasStateAuthority) return;
        
        CurrentCycleTimer = -matchTimeout;
        CurrentZoneCycleTimer = ZoneCycleTimer - (matchTimeout - 1);

        PlayerWins.Set(playerID, PlayerWins[playerID] + 1);
        
        ResetPoints();

        GameStalled = true;
        Invoke(nameof(UnStallGame), matchTimeout);
        
        RPC_PlayerReachedMaxPoints(playerID);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayerReachedMaxPoints(int playerID)
    {//runs on all players
        var winner = cr_networking.Instance.GetPlayerByID(playerID);

        if (!winner) return;
    
        Debug.Log($"{winner.accountInstance.displayName} Has Won This Match!");
        
        PlayWinParticles();
        UpdateLeaderboardWinner(winner);
    }
    public ParticleSystem WinPS;
    public void PlayWinParticles()
    {
        WinPS.Stop();
        WinPS.Play();
    }
    
    public void SpreadZones()
    {
        Vector3[] newPositions = new Vector3[captureZones.Length];

        float maxX = 12f;
        float maxY = 25f;
        float maxZ = 12f;
        
        for(int i = 0; i < captureZones.Length; i++)
        {
            float randX = Mathf.Floor(Random.Range(-maxX, maxX));
            float randY = Mathf.Floor(Random.Range(0, maxY));
            float randZ = Mathf.Floor(Random.Range(-maxZ, maxZ));

            newPositions[i] = captureZoneRoot.TransformPoint(new Vector3(randX, randY, randZ));
        }

        RPC_SpreadZones(newPositions);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SpreadZones(Vector3[] zonePositions)
    {
        for(int i = 0; i < captureZones.Length; i++)
        {
            var zone = captureZones[i];
            Vector3 oldPos = zone.transform.position;
            Vector3 newPos = zonePositions[i];

            zone.TransitionToNewPos(newPos, oldPos);
            //zone.transform.position = newPos;
        }
    }
    
    
    public void RewardAllPlayersInZones()
    {
        if (GameStalled) return;
        
        foreach(int playerID in playersInCaptureZones)
        {
            if (playerID == CurrentIt) continue; // current IT can not obtain points!
            if (playerID == -1) continue;
            
            int newPointsForPlayer = Points[playerID] + PointsPerCycle;

            Points.Set(playerID, newPointsForPlayer);
            
            if(Points[playerID] >= maxPointsTillWin)
            {
                PlayerReachedMaxPoints(playerID);
            }
        }
    }
    public void ResetPoints()
    {
        for (int i = 0; i < Points.Count; i++)
        {
            Points.Set(i, 0);
        } 
    }
    
    public void ResetWins()
    {
        for (int i = 0; i < PlayerWins.Count; i++)
        {
            PlayerWins.Set(i, 0);
        } 
    }
    
    public void CommonPlayerTick()
    {//runs on all clients (incl. server host), put visual logic here
    
    }
    
    public void UpdateLeaderboardWinner(cr_player_api winner)
    {
        string lbt = $"<color=#e6bf00>{winner.accountInstance.displayName}</color> wins!\n\n<u>Current Wins:</u>\n";

        var allPlayers = playersInMatch;
        
        var playerWinsDict = new Dictionary<int, int>();
        
        for (int i = 0; i < allPlayers.Length; i++)
        {
            var playerAPI = allPlayers[i];
            if (playerAPI == null) continue;
            
            int playerID = playerAPI.playerId;
            if (PlayerWins[playerID] == 0) continue;
            
            string playerName = playerAPI.accountInstance.displayName;
            int playerWins = PlayerWins[playerID];

            playerWinsDict[playerID] = playerWins;
        }

        // Sort dictionary by score descending
        var sorted = playerWinsDict.OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key); // optional: tie-break alphabetically
    
        // Build leaderboard text
        string leaderboardText = "";
        foreach (var kvp in sorted)
        {
            string playerName = cr_networking.Instance.GetPlayerByID(kvp.Key).accountInstance.displayName;
            leaderboardText += $"\n{kvp.Value} : {playerName}";
        }
        
        lbt += leaderboardText;
    
        foreach(var lb in leaderboard)
        {
            lb.text = lbt;
        }

        ApplyFromConfigFile();
        
        Invoke(nameof(UpdateLeaderboard), matchTimeout);
    }
    
    public void UpdateLeaderboard()
    {
        // Collect player data

        var playerPointsDict = new Dictionary<int, int>();

        var allPlayers = playersInMatch;
        
        for(int i = 0; i < allPlayers.Length; i++)
        {
            var playerAPI = allPlayers[i];
        
            if (playerAPI == null) continue;
            
            int c_playerID = playerAPI.playerId;
            
            if (Points[c_playerID] == 0) continue; // skip players without points
            if (c_playerID == 0) continue; // player indexes start at 1
            
            string playerName = playerAPI.accountInstance.displayName;
            int playerPoints = Points[c_playerID];

            playerPointsDict[c_playerID] = playerPoints;
        }

        // Sort dictionary by score descending
        var sorted = playerPointsDict.OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key); // optional: tie-break alphabetically

        // Build leaderboard text
        string leaderboardText = "";
        foreach (var kvp in sorted)
        {
            string playerName = cr_networking.Instance.GetPlayerByID(kvp.Key).accountInstance.displayName;
        
            leaderboardText += $"{kvp.Value} : {playerName}\n";
        }

        // Update all leaderboard UI elements
        foreach (var lb in leaderboard)
        {
            lb.text = leaderboardText;
        }
    }
    
    
    
    public void OnGrabEvent(int playerThatGrabbed, int playerThatHasBeenGrabbed)
    {// only runs on state authority
        if (!HasStateAuthority) return;
        if (playerThatHasBeenGrabbed == -1) return;
        if (GameStalled) return;
        
        if(CurrentIt == playerThatGrabbed)
        {
            CurrentIt = playerThatHasBeenGrabbed;
        }
    }

    
    public void OnPointsChange()
    {//points array has updated!
        if (GameStalled) return;
    
        if (playersInMatch.Length == 0) RefreshPlayerList(); //hack, fix pls
    
        UpdateLeaderboard();
    }
    
    
    public void OnWinsChange()
    {//player wins array has updated!
        
    }

    public Material c_DefaultHandMat;
    public Material cItHandMat;
    public AudioClip ItChangedSFX;
    
    public void OnItChanged()
    {//IT has changed

        foreach(var player in cr_networking.Instance.GetAllPlayers()) // reset all IT indicators
        {
            if(!player) continue;

            Color C = Color.white;
            
            if(ColorUtility.TryParseHtmlString(player.accountInstance.clientColor, out var col)) C = col;
            
            player.playerUI.NameplateText.color = C;
            
            foreach(var mr in player.playerUI.hands)
            {
                mr.material = c_DefaultHandMat;
            }

            player.thrusters.maxSpeed = DefaultThrusterSpeed;
        }

        var currentItPlayer = cr_networking.Instance.GetPlayerByID(CurrentIt);
        
        if(cr_networking.localPlayer.playerId == CurrentIt)
        {//we are IT!

            currentItPlayer.localAudio.PlaySFX(ItChangedSFX);
            
            currentItPlayer.thrusters.maxSpeed = ItThrusterSpeed;
            
        }else
        {//we are not IT!
            if (currentItPlayer)
            {
                currentItPlayer.playerUI.NameplateText.color = Color.red;
                var pos = currentItPlayer.playerInput.GetRemoteHeadPosition();

                cr_networking.localPlayer.localAudio.PlaySFXAtPosition(ItChangedSFX, pos);
            }
        }

        if (currentItPlayer) // update IT's hands to be it. Both local and remote
        {
            foreach(var mr in currentItPlayer.playerUI.hands)
            {
                mr.material = cItHandMat;
            }
        }
        
    }
    
    public void OnPlayerEnterCaptureZone(int playerID)
    {
        if (!Object.HasStateAuthority) return;
        
        Debug.Log($"Player with id {playerID} entered a capture zone");

        if (playersInCaptureZones.Contains(playerID)) return;
        
        playersInCaptureZones.Add(playerID);
    }
    
    public void OnPlayerExitCaptureZone(int playerID)
    {
        if (!Object.HasStateAuthority) return;
        
        Debug.Log($"Player with id {playerID} exited a capture zone");

        if (!playersInCaptureZones.Contains(playerID)) return;
        
        playersInCaptureZones.Remove(playerID);
    }

    
    
    public cr_player_api[] playersInMatch;
    public void RefreshPlayerList()
    {
        Debug.Log("Updating Player List");
    
        playersInMatch = cr_networking.Instance.GetAllPlayers();
    }

    public override void _OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        RefreshPlayerList();
        
        if(Object.HasStateAuthority)
        {
            if (!Points.ContainsKey(player.PlayerId)) Points.Add(player.PlayerId, 0);
            if (!PlayerWins.ContainsKey(player.PlayerId)) PlayerWins.Add(player.PlayerId, 0);
        }
    }
    
    public override void _OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        RefreshPlayerList();

        if (!HasStateAuthority) return;

        Points.Remove(player.PlayerId);
        PlayerWins.Remove(player.PlayerId);
        
        playersInCaptureZones.RemoveAll(id => id == player.PlayerId);
        
        if (player.PlayerId == CurrentIt) 
        {//current IT left. Pick random player and turn them into IT
            PickRandomNewIT();
        }
    }
}
