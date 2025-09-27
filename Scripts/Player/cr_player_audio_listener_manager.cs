using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_player_audio_listener_manager : MonoBehaviour
{
    private static cr_player_audio_listener_manager _instance;
    public static cr_player_audio_listener_manager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<cr_player_audio_listener_manager>();
            }
            return _instance;
        }
    }

    public Transform AL;

    public Transform MainCamera;
    public Transform LoadingCamera;
    public Transform CurrentTarget;

    void Start() => CurrentTarget = LoadingCamera; // on load, set the fucking target uwu

    public void SwapListenerTarget(float delay, float duration)
    {
        Transform newTarget = null;
        if (CurrentTarget == MainCamera)    newTarget = LoadingCamera;
        if (CurrentTarget == LoadingCamera) newTarget = MainCamera;

        if (newTarget == null) newTarget = MainCamera;
        
        StartCoroutine(CSwapListenerTarget(newTarget, duration, delay));
    }
    
    IEnumerator CSwapListenerTarget(Transform newTarget, float duration, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
    
        float startVol = AudioListener.volume;
        float t = 0f;

        // Fade out
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, duration);
            AudioListener.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }
        AudioListener.volume = 0f;

        // Hand off
        CurrentTarget = newTarget;
        SnapListenerToTarget();

        // Fade in
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, duration);
            AudioListener.volume = Mathf.Lerp(0f, startVol, t);
            yield return null;
        }
        AudioListener.volume = startVol;
    }


    // Update is called once per frame
    void Update()
    {
        AL.transform.position = CurrentTarget.position;
        AL.transform.rotation = CurrentTarget.rotation;
    }
    
    void SnapListenerToTarget()
    {
        if (!AL || !CurrentTarget) return;

        AL.position = CurrentTarget.position;
        AL.rotation = CurrentTarget.rotation;
    }
}
