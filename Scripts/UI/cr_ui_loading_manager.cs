using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_ui_loading_manager : cr_MonoBehavior
{
    public GameObject LoadingEnv;
    public MeshRenderer fadeMR;



    public void Start()
    {
        LoadingEnv.SetActive(true);

        FadeAndDoAction(4, 2, 1, 1, SwapViews, null);
        cr_player_audio_listener_manager.Instance.SwapListenerTarget(4, 1);
    }

    public void SwapViews()
    {
        LoadingEnv.SetActive(!LoadingEnv.activeInHierarchy);
    }




    public void FadeAndDoAction(float delay, float postBuffer, float fadeInTime, float fadeOutTime, Action climaxMessage, Action postBufferMessage)
    {
        StartCoroutine(SFadeAndDoAction(delay, postBuffer, fadeInTime, fadeOutTime, climaxMessage, postBufferMessage));
    }
    
    public IEnumerator SFadeAndDoAction(float delay, float postBuffer, float fadeInTime, float fadeOutTime, Action climaxMessage, Action postBufferMessage)
    {
        yield return new WaitForSeconds(delay);
    
        MeshRenderer mr = fadeMR;
        Material mat = mr.material;

        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInTime);

            Color c = mat.color;
            c.a = Mathf.Lerp(0f, 1f, t);
            mat.color = c;

            yield return null;
        }
        
        climaxMessage?.Invoke();
        

        elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutTime);

            Color c = mat.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            mat.color = c;

            yield return null;
        }

        yield return new WaitForSeconds(postBuffer);
        postBufferMessage?.Invoke();
    }
}
