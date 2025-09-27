using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_dev_match_game_catch_zone_fx : MonoBehaviour
{
    public Animator controller;
    public ParticleSystem ZoneUpdatePart;
    public AnimationClip clip;

    bool canProgress = false;
    
    public void OnFadeOutDone()
    {
        canProgress = true;
    }

    public void TransitionToNewPos(Vector3 position, Vector3 oldPos)
    {
        canProgress = false;
        StartCoroutine(TransitionProcess(position, oldPos));
    }

    public IEnumerator TransitionProcess(Vector3 newPos, Vector3 oldPos)
    {
        controller.SetBool("isOpen", false);

        float bailT = 0;
        while(!canProgress)
        {
            if(bailT >= 5) yield break;
            bailT += Time.deltaTime;
            yield return null;
        }
        
        var go = Instantiate(ZoneUpdatePart, oldPos, Quaternion.identity);
        go.Play();
        
        transform.position = newPos;
        controller.SetBool("isOpen", true);
        
        Destroy(go.gameObject, 10f);
    }
}