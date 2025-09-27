using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class cr_ui_player_velocity_hud : cr_MonoBehavior
{
    public TextMeshProUGUI TXT;

    public cr_player_physics_head head;


    void LateUpdate()
    {
        float vel = Mathf.Clamp(head.rb.velocity.magnitude, 0, 99.99f);

        string c = head.grabMotor.isGrabbing ? $"{vel:F0}" : $"{vel:F1}";
    
        string toDraw = $"{c} M/s";
        
        TXT.text = toDraw;
    }
}
