using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class cr_ui_fools_player_turn_around_tip_looker : cr_MonoBehavior
{
    public UnityEvent leEvent;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("LocalPlayer"))
        {
            leEvent?.Invoke();
        }
    }
}
