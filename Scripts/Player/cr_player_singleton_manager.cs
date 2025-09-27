using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_player_singleton_manager : cr_MonoBehavior
{
    public Behaviour[] singletonComponents;
    public Behaviour[] oppositeSingletonComponents;

    public GameObject[] singletonObjects;
    public GameObject[] oppositeSingletonObjects;
    public void ToggleComponents(bool isLocal)
    {
        foreach(var b in singletonComponents)
        {
            b.enabled = isLocal;
        }
        
        foreach(var b in oppositeSingletonComponents)
        {
            b.enabled = !isLocal;
        }   
         
    
        foreach(var c in singletonObjects)
        {
            c.SetActive(isLocal);
        }
        
        foreach(var c in oppositeSingletonObjects)
        {
            c.SetActive(!isLocal);
        }
    }
}