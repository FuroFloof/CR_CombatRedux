 using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class cr_dev_match_game_race : cr_MatchBehavior
{
    [Header("Game Vars")]

    public int MaxLaps = 3;


    [Header("Game Refs")]
    public GameObject MatchStartBox;
    public TextMeshProUGUI GoalText;
    
    [Networked, Capacity(cr_game.SV_MAX_PLAYERS), OnChangedRender(nameof(OnPassTriggerStatesChanged))]
    public NetworkDictionary<int, int> p_PassTriggerStates{ get; }
    
    [Networked, Capacity(cr_game.SV_MAX_PLAYERS), OnChangedRender(nameof(OnLapStatesChanged))]
    public NetworkDictionary<int, int> p_LapStates{ get; }
    
    [Networked, OnChangedRender(nameof(OnRaceHasStartedChanged))]
    public bool RaceHasStarted { get; set; }

    [Networked, Capacity(cr_game.SV_MAX_PLAYERS), OnChangedRender(nameof(OnSpectatorsChanged))]
    public NetworkDictionary<int, bool> p_Spectators => default;

    public cr_dev_match_game_race_pass_trigger[] passTriggers;

    public override void Spawned()
    {
        base.Spawned();

        if (!Object.HasStateAuthority) return;
        
        // RaceHasStarted = false;
        // p_PassTriggerStates.Clear();
        ClearLapsAndPassStates();
    }
    
    public void ClearLapsAndPassStates()
    {   
        foreach(var k in p_PassTriggerStates) // clear pass states
        {
            int playerID = k.Key;
            int currentPassState = k.Value;

            p_PassTriggerStates.Set(playerID, 0);
        }
        
        foreach(var l in p_LapStates) // clear laps
        {
            int playerID = l.Key;
            int currentPassState = l.Value;

            p_LapStates.Set(playerID, 0);
        }
    }

    public override void StateAuthorityTick()
    {
        
    }

    public override void CommonPlayerTick()
    {
        
    }


    [ContextMenu("RC/Start Race")]
    public void StartRace()
    {
        cr_networking.localPlayer.thrusters.maxSpeed = cr_game_defaults_dev_racism.m_max_thruster_speed;
    
        if (!Object.HasStateAuthority) return;
        if (RaceHasStarted) return;

        UpdatePlayerList();
        
        foreach(var p in playersInMatch)
        {
            DistrobutePlayer(p.playerId);
            
            p.gameplayData.OnPlayerRespawnEvent += OnPlayerRespawned; //subscribe to all players' respawn events!
        }

        RaceHasStarted = true;

        RPC_StartRaceVisuals();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StartRaceVisuals()
    {
        StartCoroutine(CStartRace());
    }
    public IEnumerator CStartRace()
    {
        GoalText.text = "<b>Starting Race...</b>";
        yield return new WaitForSeconds(2);
        
        GoalText.text = "";
        yield return new WaitForSeconds(1);
        
        GoalText.text = "3";
        yield return new WaitForSeconds(1);
        
        GoalText.text = "2";
        yield return new WaitForSeconds(1);
        
        GoalText.text = "1";
        yield return new WaitForSeconds(1);

        GoalText.text = "START!";
        MatchStartBox.SetActive(false);
        
        
        yield return new WaitForSeconds(5);
        GoalText.text = "";
    }
    
    public void PlayerEnteredPassTrigger(int playerID, cr_dev_match_game_race_pass_trigger trigger)
    {
        if(!Object.HasStateAuthority) return;

        int playersCurrentPassID = p_PassTriggerStates[playerID];

        int newTriggerIndex = 0;
        
        for(int i = 0; i < passTriggers.Length; i++)
        {
            if (passTriggers[i] == trigger) newTriggerIndex = i;
        }


        if (playersCurrentPassID + 1 != newTriggerIndex && newTriggerIndex != 0) return;

        p_PassTriggerStates.Set(playerID, newTriggerIndex);
    }
    
    public void PlayerCollidedWithDeath(int playerID)
    {
        if (!Object.HasStateAuthority) return;
        if (!RaceHasStarted) return;
    
        var player = cr_networking.Instance.GetPlayerByID(playerID);

        player.gameplayData.ApplyDamage(999f);
    }

    public void PlayerEnteredGoal(int playerID)
    {
        if (!Object.HasStateAuthority) return;
        if(p_PassTriggerStates[playerID] != passTriggers.Length - 1) return;
        
        if(p_LapStates[playerID] >= MaxLaps)
        {//a player has finished the last lap
        
            foreach (var p in playersInMatch)
            {
                DistrobutePlayer(p.playerId);
            }

            PlayerFinishedRace(playerID);
            
        }else
        {
            p_LapStates.Set(playerID, p_LapStates[playerID] + 1);
            //p_PassTriggerStates.Set(playerID, 0); might give advantage, idk
        }
    }
    
    
    public void PlayerFinishedRace(int playerID)
    {
        ClearLapsAndPassStates();
        
        RaceHasStarted = false;
    
        RPC_PlayerFinishedRace(playerID);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayerFinishedRace(int playerID)
    {
        var player = cr_networking.Instance.GetPlayerByID(playerID);

        GoalText.text = $"<b><color=#dcb962>{player.accountInstance.displayName}</color> Has Won the Race!</b>\n";

        MatchStartBox.SetActive(true);
    }
    
    
    public void OnPlayerRespawned(int playerID)
    {
        if (!Object.HasStateAuthority) return;
        
        var player = cr_networking.Instance.GetPlayerByID(playerID);
        if (!player) return;

        //DistrobutePlayer(player.playerId); // don't tp to race start

        int currentPassTrigger = p_PassTriggerStates[playerID];
        var pass = passTriggers[currentPassTrigger];
        Transform T = pass.spawnPoint;

        RPC_MovePlayerToTarget(T.position, T.forward, playerID);
    }
    
    
    
    public void OnPassTriggerStatesChanged()
    {
        
    }
    
    public void OnLapStatesChanged()
    {
        
    }
    
    public void OnRaceHasStartedChanged()
    {
        
    }
    
    public void OnSpectatorsChanged()
    {// when spectators dictionary changes
        
    }
    
    public void MarkPlayerAsSpectator(int playerID, bool SetAsSpectator)
    {
        p_Spectators.Set(playerID, SetAsSpectator);
    }
    

    public override void _OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        base._OnPlayerJoined(runner, player);
        
        if (!Object.HasStateAuthority) return;

        if (!p_PassTriggerStates.ContainsKey(player.PlayerId)) p_PassTriggerStates.Set(player.PlayerId, 0);
        if (!p_LapStates.ContainsKey(player.PlayerId)) p_LapStates.Set(player.PlayerId, 0);
        if (!p_Spectators.ContainsKey(player.PlayerId)) p_Spectators.Set(player.PlayerId, false); 

        MarkPlayerAsSpectator(player.PlayerId, RaceHasStarted); // mark late joiners as spectators
    }
    
    public override void _OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        base._OnPlayerLeft(runner, player);

        if (!Object.HasStateAuthority) return;
        
        if (p_PassTriggerStates.ContainsKey(player.PlayerId)) p_PassTriggerStates.Remove(player.PlayerId);
        if (p_LapStates.ContainsKey(player.PlayerId)) p_LapStates.Remove(player.PlayerId);
        if (p_Spectators.ContainsKey(player.PlayerId)) p_Spectators.Remove(player.PlayerId);
    }
}