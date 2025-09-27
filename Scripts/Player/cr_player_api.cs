using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using WebSocketSharp;

public class cr_player_api : cr_NetworkBehavior
{
    public cr_account_instance accountInstance; //public player account data.
    
    public int playerId;
    public bool isLocal;
    public bool isMaster => Object.HasStateAuthority;  //is this player the server master/host?
    
    [Networked, OnChangedRender(nameof(OnTokenChanged))] 
    public NetworkString<_64> PublicToken { get; set; } //the player's public identifyer token
    
    [Networked, OnChangedRender(nameof(OnGrabFeud))]
    public int GrabFeudTarget {get; set;}
    
    [Networked, OnChangedRender(nameof(OnLeftGrabbingPlayer))]
    public int LeftCurrentGrabbingPlayer {get; set;}
    [Networked, OnChangedRender(nameof(OnRightGrabbingPlayer))]
    public int RightCurrentGrabbingPlayer {get; set;}

    #region PUBLIC REFERENCES
    public cr_player_movement_thrusters thrusters;
    public cr_player_audio localAudio;
    public cr_player_singleton_manager singletonManager;
    public cr_ui_player_ui_container playerUI;
    public MeshRenderer[] BodyRenderers;
    public cr_player_controller playerController;
    public cr_ui_loading_manager loadingManager;
    public cr_player_gameplay_data gameplayData;
    public cr_player_spray_manager sprayManager;
    
    #endregion
    
    [SerializeField] private cr_player_input _playerInput;
    public cr_player_input playerInput
    {
        get
        {
            //if (isLocal) return _playerInput;
            //return null;
            return _playerInput;
        }
    }

    
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            GrabbingPlayer(-1, (int)cr_input_hand_type.left); //initialize to -1
            GrabbingPlayer(-1, (int)cr_input_hand_type.right); //initialize to -1
        }
    
        playerId = Object.InputAuthority.PlayerId;
        isLocal = Object.HasInputAuthority;
        
        singletonManager.ToggleComponents(isLocal);

        if (isLocal) {
        
            SavePublicToken(cr_account.GetLocalTokenForSelf());
            foreach(var mr in BodyRenderers)
            {
                //mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
            
        } else {
            
            
            
        }

        TryBindAccountFromToken();
        
        GameObject DEV_SPAWN = GameObject.Find("DEV_SPAWN");
        if(DEV_SPAWN != null)
        {
            playerController.headPhys.TeleportHead(DEV_SPAWN.transform.position, DEV_SPAWN.transform.forward, true);
        }
    }
    
    private void ApplyPlayerUIData()
    {
        playerUI.NameplateText.text = accountInstance.displayName;
        
        Color C = Color.white;
        if (ColorUtility.TryParseHtmlString(accountInstance.clientColor, out var col)) C = col;

        C.a = 1;
        
        playerUI.NameplateText.color = C; 
    }
    
    public async void TryBindAccountFromToken()
    {
        Debug.Log($"Trying to bind account to player by token for player {playerId}");
        if (PublicToken.Value.IsNullOrEmpty()) return;
        
        Debug.Log($"Getting Remote Account Instance by Token... T:{PublicToken.Value}");
        
        string token = PublicToken.Value;
        accountInstance = await cr_account.GetPlayerAccountInstanceByPublicToken(token);

        ApplyPlayerUIData();
    }


    public override void FixedUpdateNetwork()
    {
        
    }


    public void SavePublicToken(string token)
    {
        if (!Object.HasInputAuthority) return;

        RPC_NetworkPublicToken(token);
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_NetworkPublicToken(string token)
    {
        PublicToken = token;
    }
    public void OnTokenChanged()
    {
        //called on all clients.
        Debug.Log($"Player {playerId} Public token: {PublicToken}");
        
        TryBindAccountFromToken();
    }
    
    public void PlayerGrabFeud(int _playerID)
    {
        if (!Object.HasInputAuthority) return;

        RPC_PlayerGrabFeud(_playerID);
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_PlayerGrabFeud(int _playerID)
    {
        GrabFeudTarget = _playerID;
    }
    public void OnGrabFeud()
    {
        //called on all clients
        var m_LocalPlayer = cr_networking.localPlayer;
        
        if(m_LocalPlayer.playerId == GrabFeudTarget)
        {// this player has a grab feud with us!!! Stop the grab!! :3
            var grabHand = m_LocalPlayer.playerInput.GetCurrentGrabHandOrNull();
            
            if(grabHand)
            {
                var grab = grabHand.hand;
                
                grab.EndGrab();
            }
        }
    }
    
    public void GrabbingPlayer(int targetPlayer, int handType) 
    {
        if (!Object.HasInputAuthority) return;

        RPC_GrabbingPlayer(targetPlayer, handType);
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_GrabbingPlayer(int targetPlayer, int handType)
    {
        switch((cr_input_hand_type)handType)
        {
            case cr_input_hand_type.left:
                LeftCurrentGrabbingPlayer = targetPlayer;
            break;
                
            case cr_input_hand_type.right:
                RightCurrentGrabbingPlayer = targetPlayer;
            break;      
        }
    }
    public void OnLeftGrabbingPlayer()
    {
        if (LeftCurrentGrabbingPlayer == -1) return;
        OnGrabbingPlayer();
    }
    public void OnRightGrabbingPlayer()
    {
        if (RightCurrentGrabbingPlayer == -1) return;
        OnGrabbingPlayer();
    }
    public void OnGrabbingPlayer()
    {//called on all clients
        //we're just syncing the value, so fuck off.
        if(HasStateAuthority)//for catch test. Remove asap!!!
        {
            cr_dev_match_game_catch.Instance.OnGrabEvent(playerId, GetCurrentGrabbingPlayer());
        }
    }
    public int GetCurrentGrabbingPlayer()
    {
        //method 2
        if (LeftCurrentGrabbingPlayer != -1) return LeftCurrentGrabbingPlayer;
        if (RightCurrentGrabbingPlayer != -1) return RightCurrentGrabbingPlayer;
    
        return -1; //we're not grabbing anyone. return -1 as nobody
    }
    

    
    void Start()
    {
        Debug.Log(playerId);
    }

    void Update()
    {
        
    }
}