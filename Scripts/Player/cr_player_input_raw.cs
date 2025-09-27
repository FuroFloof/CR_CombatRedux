using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class cr_player_input_raw : cr_MonoBehavior
{
    public bool overwritePrimaryButton;
    public bool overwritePrimaryTouched;
    public bool overwriteSecondaryButton;
    public bool overwriteSecondaryTouched;
    public Vector2 overwriteStickAxis;
    public bool overwriteStickPressed;
    public bool overwriteStickTouched;
    public bool overwriteTriggerPressed;
    public bool overwriteTriggerTouched;
    [Range(-1, 1)]
    public float overwriteTriggerValue = -1;
    public bool overwriteGripPressed;
    [Range(-1, 1)]
    public float overwriteGripValue = -1;
    


    public cr_input_hand_type handType;
    public cr_player_hand hand;
    public cr_player_device_offset offset;

    void Awake()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        
    }

    void LateUpdate()
    {
        _primaryButton.wasHeld = GetPrimaryButton();
        _primaryButtonTouched.wasHeld = GetPrimaryTouched();
        _gripPressed.wasHeld = GetGripPressed();
        _secondaryButton.wasHeld = GetSecondaryButton();
        
        lastPosition = transform.position;
    }



    public InputActionReference primaryButton;
    public cr_input_raw _primaryButton;
    public bool GetPrimaryButton()
    {
        bool r = primaryButton.action.ReadValue<float>() > 0;
        bool b = overwritePrimaryButton ? overwritePrimaryButton : r;
        return b;
    }
    public bool GetPrimaryButtonUp()
    {
        return _primaryButton.GetUp(GetPrimaryButton());
    }
    public bool GetPrimaryButtonDown()
    {
        return _primaryButton.GetDown(GetPrimaryButton());
    }
    
    public InputActionReference primaryButtonTouched;
    public cr_input_raw _primaryButtonTouched;
    public bool GetPrimaryTouched()
    {
        bool r = primaryButtonTouched.action.ReadValue<float>() > 0;
        bool b = overwritePrimaryTouched ? overwritePrimaryTouched : r;
        return b;
    }
    public bool GetPrimaryTouchedUp()
    {
        return _primaryButtonTouched.GetUp(GetPrimaryTouched());
    }
    public bool GetPrimaryTouchedDown()
    {
        return _primaryButtonTouched.GetDown(GetPrimaryTouched());
    }
    
    public InputActionReference secondaryButton;
    public cr_input_raw _secondaryButton;
    public bool GetSecondaryButton()
    {
        bool r = secondaryButton.action.ReadValue<float>() > 0;
        bool b = overwriteSecondaryButton ? overwriteSecondaryButton : r;
        return b;
    }
    public bool GetSecondaryButtonUp()
    {
        return _secondaryButton.GetUp(GetSecondaryButton());
    }
    public bool GetSecondaryButtonDown()
    {
        return _secondaryButton.GetDown(GetSecondaryButton());
    }
    
    
    public InputActionReference secondaryButtonTouched;
    public bool GetSecondaryTouched()
    {
        bool r = secondaryButtonTouched.action.ReadValue<float>() > 0;
        bool b = overwriteSecondaryTouched ? overwriteSecondaryTouched : r;
        return b;
    }
    
    public InputActionReference stickAxis;
    public Vector2 GetStickAxis()
    {
        Vector2 r = stickAxis.action.ReadValue<Vector2>();
        Vector2 v = overwriteStickAxis != Vector2.zero ? overwriteStickAxis : r;
        return v;
    }
    
    public InputActionReference stickPressed;
    public bool GetStickPressed()
    {
        bool r = stickPressed.action.ReadValue<float>() > 0;
        bool b = overwriteStickPressed ? overwriteStickPressed : r;
        return b;
    }
    
    public InputActionReference stickTouched;
    public bool GetStickTouched()
    {
        bool r = stickTouched.action.ReadValue<float>() > 0;
        bool b = overwriteStickTouched ? overwriteStickTouched : r;
        return b;
    }
    
    public InputActionReference triggerPressed;
    public bool GetTriggerPressed()
    {
        bool r = triggerPressed.action.ReadValue<float>() > 0;
        bool b = overwriteTriggerPressed ? overwriteTriggerPressed : r;
        return b;
    }
    
    public InputActionReference triggerTouched;
    public bool GetTriggerTouched()
    {
        bool r = triggerTouched.action.ReadValue<float>() > 0;
        bool b = overwriteTriggerTouched ? overwriteTriggerTouched : r;
        return b;
    }
    
    public InputActionReference triggerValue;
    public float GetTriggerValue()
    {
        float r = triggerValue.action.ReadValue<float>();
        float f = overwriteTriggerValue != -1 ? overwriteTriggerValue : r;
        return f;
    }
    
    public InputActionReference gripPressed;
    public cr_input_raw _gripPressed;
    public bool GetGripPressed()
    {
        bool r = gripPressed.action.ReadValue<float>() > 0;
        bool b = overwriteGripPressed ? overwriteGripPressed : r;
        return b;
    }
    public bool GetGripDown()
    {
        return _gripPressed.GetDown(GetGripPressed());
    }
    public bool GetGripUp()
    {
        return _gripPressed.GetUp(GetGripPressed());
    }
    
    public InputActionReference gripValue;
    public float GetGripValue()
    {
        float r = gripValue.action.ReadValue<float>();
        float f = overwriteGripValue != -1 ? overwriteGripValue : r;
        return f;
    }

    public ActionBasedController XRController;
    public bool SendHapticFeedback(float strength, float length)
    {
        return XRController.SendHapticImpulse(strength, length);
    }

    public Transform handObject;
    public Vector3 GetHandPosition()
    {
        return handObject.position;
    }
    public Quaternion GetHandRotation()
    {
        return handObject.rotation;
    }
    public Transform GetHandTransform()
    {
        return handObject;
    }

    public Vector3 lastPosition;
    public Vector3 GetHandVelocity()
    {
        // meters per second
        return (transform.position - lastPosition) / Mathf.Max(Time.deltaTime, 1e-4f);
    }
    
}

public enum cr_input_hand_type
{
    left,
    right
}


public struct cr_input_raw
{
    public bool wasHeld;
    
    public bool Get(bool current)
    {
        return current;
    }

    public bool GetDown(bool current)
    {
        bool down = current && !wasHeld;
        return down;
    }

    public bool GetUp(bool current)
    {
        bool up = !current && wasHeld;
        return up;
    }
}