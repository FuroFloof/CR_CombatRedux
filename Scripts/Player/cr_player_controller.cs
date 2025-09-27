using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class cr_player_controller : cr_MonoBehavior
{
    public cr_player_input playerInput;
    public cr_player_physics_head headPhys;
    public cr_player_movement_rotation headRot;
    
    void Start()
    {
        ApplyConfigFile();
    }
    
    public override void ApplyConfigFile()
    {
        var configFilePath = Path.Combine(cr_game.CustomGameConfigPath, "controls.cfg");
        float timeToResetRotation = cr_game_file_parser.Instance.GetFileVar<float>
        (
            configFilePath,                             // file path
            nameof(cr_game_defaults.c_stick_rot_reset_timer),             // confVar name
            cr_game_defaults.c_stick_rot_reset_timer    // default value
        );

        // APPLY

        headResetTimerMax = timeToResetRotation;
    }

    void Update()
    {
        HandleHeadReset();
    }

    void FixedUpdate()
    {
        HandleBreak();
    }


    public float breakSpeed = 3;
    public void HandleBreak()
    {
        bool wantsToBreak = playerInput.rightHand.GetStickPressed() && !playerInput.leftHand.GetStickPressed();
        
        var vel = headPhys.rb.velocity;
        
        if(wantsToBreak && vel.magnitude > 0.001f)
        {
            float nextMag = vel.magnitude - breakSpeed * Time.deltaTime;

            Vector3 targetVel = Vector3.ClampMagnitude(vel, nextMag);
            headPhys.rb.velocity = targetVel;
            
        }else if(wantsToBreak && vel.magnitude < 0.001f)
        {
            headPhys.rb.velocity = Vector3.zero;
        }
    }
    
    
    public float headResetTimerMax = 1.75f;
    float headResetTimer;
    bool canResetHead = true;
    public void HandleHeadReset()
    {
        bool wantsToReset = playerInput.rightHand.GetStickPressed() && playerInput.leftHand.GetStickPressed();
    
        if(wantsToReset && canResetHead)
        {
            if(headResetTimer >= headResetTimerMax)
            {
                ResetPlayerRotation();
                headResetTimer = 0f;
                canResetHead = false;
                return;
            }else
            {
                headResetTimer += Time.deltaTime;
            }
        }else if(!wantsToReset)
        {
            canResetHead = true;
            headResetTimer = 0f;
        }
    }



    public void TeleportPlayer(Vector3 position, float yawDegrees = 0, bool resetVelocity = true, LayerMask mask = default)
    {
        ResetPlayerRotation();
        
        headPhys.TeleportHeadTo(position, yawDegrees, resetVelocity, mask);
    }
    
    public void ResetPlayerRotation()
    {
        headRot.ResetRotation();
    }
}
