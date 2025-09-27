using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_player_fx_death : cr_MonoBehavior
{
    public cr_player_api player;

    public Animator localAnim;

    public ParticleSystem part;
    public AudioClip localDeathAudio;
    public AudioClip remoteDeathAudio;


    void Awake()
    {
        player.gameplayData.OnPlayerDeathEvent += OnDeathEvent;
    }
    
    public void OnDeathEvent(int playerID)
    {
        if (player.isLocal) LocalDeath();
        else RemoteDeath();
    }


    public void LocalDeath()
    {
        player.localAudio.PlaySFX(localDeathAudio);
        localAnim.SetBool("drawAnim", true);
    }
    
    public void OnLocalDeathAnimComplete()
    {
        localAnim.SetBool("drawAnim", false);
    }
    
    public void RemoteDeath()
    {
        var headpos = player.playerInput.GetRemoteHeadPosition();
    
        cr_networking.localPlayer.localAudio.PlaySFXAtPosition(remoteDeathAudio, headpos);
        
        part.transform.position = headpos;
        part.Play();
    }
}