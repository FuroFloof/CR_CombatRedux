using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_player_audio : cr_MonoBehavior
{
    public AudioSource AS;

    public float spacialAudioMaxDist = 10f;

    public float SFXVolume;
    public float VoicesVolume;
    public float MusicVolume;
    
    public void PlaySFX(AudioClip clip, int overrideSFXVolume = -1, float pitch = 1)
    {
        AS.volume = SFXVolume / 100;
        AS.pitch = pitch;
        
        if(overrideSFXVolume != -1)
        {
            AS.volume = overrideSFXVolume / 100;
        }

        AS.PlayOneShot(clip);
    }
    
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, int overrideSFXVolume = -1, float pitch = 1)
    {
        var newASOBJ = new GameObject("TempLocalAudio");
        newASOBJ.transform.position = position;
        var newAS = newASOBJ.AddComponent<AudioSource>();
        
        newAS.maxDistance = spacialAudioMaxDist;
        newAS.rolloffMode = AudioRolloffMode.Linear;
        newAS.spatialBlend = 1f;
        
        newAS.volume = SFXVolume / 100;
        newAS.pitch = pitch;
        
        if(overrideSFXVolume != -1)
        {
            newAS.volume = overrideSFXVolume / 100;
        }

        newAS.PlayOneShot(clip);

        Destroy(newASOBJ, clip.length + 1);
    }
}
