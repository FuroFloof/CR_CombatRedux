using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Events;

public class cr_player_gameplay_data : cr_NetworkBehavior
{   

    public cr_player_api player;
    
    string _pLabel;
    public override void Spawned()
    {
        _pLabel = $"P{cr_networking.Instance.GetPlayerByID(Object.InputAuthority.PlayerId)}";
    }
    
    void Start()
    {
        //subscribe to some events
        OnPlayerDeathEvent += C_OnPlayerDeathEvent;
        OnPlayerRespawnEvent += C_OnPlayerRespawnEvent;
        
    }
    
    void OnDestroy()
    {
        //unsubscribe to some events to avoid mem leaks
        OnPlayerDeathEvent -= C_OnPlayerDeathEvent;
        OnPlayerRespawnEvent -= C_OnPlayerRespawnEvent;
    }

    #region Variables

    [NonSerialized] public float RespawnTime = cr_game_defaults.p_default_respawn_time;
    [NonSerialized] public float MaxHealth = cr_game_defaults.p_max_health;
    [NonSerialized] public float MaxShield = cr_game_defaults.p_max_shield;
    [Networked] public TickTimer RespawnTimer { get; set; } = TickTimer.None;
    
    
    #endregion Variables
    


    public void Update()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (SafeCheckStateAuth()) StateAuthTick();   
    }
    
    public override void Render()
    {
        if (player.isLocal) { LocalPlayerTick(); } else { RemotePlayerTick(); }
    }
    
    
    public void StateAuthTick()
    {//handle shit like, if player health < 0 set health to 0 and fire death event, as well as coroutine for respawn.
        if(IsDead && RespawnTimer.Expired(Runner))
        {
            OnPlayerRespawn();
        }
    }
    
    public void LocalPlayerTick()
    {//handle local stuff like HUD updates, and such.
        
    }
    
    public void RemotePlayerTick()
    {//handle remote things, like sub-nameplate health updates.
        
    }

    


    public bool SafeCheckStateAuth()
    {
        if (!Object) return false;
        
        return Object.HasStateAuthority;
    }
    
    
    
    
    #region Networked Vars

    [Networked, OnChangedRender(nameof(OnTeamChanged))] public cr_player_gameplay_team Team { get; set; } = cr_player_gameplay_team.Unassigned;
    public void OnTeamChanged()
    {
        Debug.Log($"Team for {_pLabel} Changed to {Team}");
    }
    public void ChangeTeam(cr_player_gameplay_team team)
    {//ran by state auth to change value
        if (!SafeCheckStateAuth()) return;

        Team = team;
    }
    public cr_player_gameplay_team GetTeam()
    {
        return Team;
    }

    

    [Networked, OnChangedRender(nameof(OnIsDeadChanged))] public NetworkBool IsDead { get; set; } = false;
    public void OnIsDeadChanged()
    {// render side only. do not mutate networked state here
        Debug.Log($"Player {_pLabel} Updated isDead to {IsDead}");
    }
    public void ChangeIsDead(bool isDead)
    {//ran by state auth to change value
        if (!SafeCheckStateAuth()) return;

        IsDead = isDead;
    }
    public bool GetIsDead()
    {
        return IsDead;
    }



    [Networked, OnChangedRender(nameof(OnHealthChanged))] public float Health { get; set; } = cr_game_defaults.p_default_health;
    public void OnHealthChanged()
    {
        Debug.Log($"Health for {_pLabel} Changed to {Health}");
    }
    public void ChangeHealth(float health)
    {//ran by state auth to change value
        if (!SafeCheckStateAuth()) return;

        Health = health;
    }
    public float GetHealth()
    {
        return Health;
    }
    
    
    [Networked, OnChangedRender(nameof(OnShieldChanged))] public float Shield { get; set; } = cr_game_defaults.p_default_shield;
    public void OnShieldChanged()
    {
        Debug.Log($"Shield for {_pLabel} Changed to {Shield}");
    }
    public void ChangeShield(float shield)
    {//ran by state auth to change value
        if (!SafeCheckStateAuth()) return;

        Shield = shield;
    }
    public float GetShield()
    {
        return Shield;
    }
    
    #endregion Networked Vars
    
    
    
    #region Networked Events
    
    public void OnPlayerDeath()
    {//ran by state auth to start event
        if (!SafeCheckStateAuth() || IsDead) return;
        
        ChangeIsDead(true);
        
        RespawnTimer = TickTimer.CreateFromSeconds(Runner, RespawnTime);
        
        RPC_OnPlayerDeath();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_OnPlayerDeath()
    {
        OnPlayerDeathEvent?.Invoke(player.playerId);
    }
    public event Action<int> OnPlayerDeathEvent;
    public void C_OnPlayerDeathEvent(int playerID)
    {
        Debug.Log($"Player {_pLabel} Canonically Died! :(");
    }
    
    
    
    public void OnPlayerRespawn()
    {//ran by state auth to start event
        if(!SafeCheckStateAuth()) return;

        Health = MaxHealth;
        Shield = MaxShield;
        ChangeIsDead(false);
        RespawnTimer = TickTimer.None;
        
        RPC_OnPlayerRespawn();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_OnPlayerRespawn()
    {
        OnPlayerRespawnEvent?.Invoke(player.playerId);
    }
    public event Action<int> OnPlayerRespawnEvent;
    public void C_OnPlayerRespawnEvent(int playerID)
    {
        Debug.Log($"Player {_pLabel} Canonically Respawned! :)");
    }

    #endregion Networked Events
    
    
    
    #region Common Functions
    
    public void ApplyDamage(float amount)
    {//ran by state auth
        if (!SafeCheckStateAuth()) return;
        if (IsDead) return;
        if (amount <= 1f) return; // if we deal less than or equal to 1 damage, we can just skip that shit bruv

        float leftover = amount;

        if (Shield > 0f)
        {
            float used = Mathf.Min(Shield, leftover);
            Shield -= used;
            leftover -= used;
        }

        if (leftover > 0f) Health -= leftover;

        Health = Mathf.Max(0f, Health);

        if (!IsDead && Health <= 0f)
        {
            OnPlayerDeath(); // RPC fanout
        }
    }
    
    #endregion Common Functions
}

public enum cr_player_gameplay_team
{
    Unassigned,
    A,
    B,
    C,
    D,
    Spectator,
    NPC,
    Boss
}