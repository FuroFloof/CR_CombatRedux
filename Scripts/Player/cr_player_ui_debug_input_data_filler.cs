using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class cr_player_ui_debug_input_data_filler : MonoBehaviour
{
    public cr_player_input input;
    public cr_player_ui_debug_input_data left;
    public cr_player_ui_debug_input_data right;


    void Update()
    {
        left.PrimaryButton.text = $"{input.leftHand.GetPrimaryButton()}";
        left.PrimaryTouched.text = $"{input.leftHand.GetPrimaryTouched()}";
        left.SecondaryButton.text = $"{input.leftHand.GetSecondaryButton()}";
        left.SecondaryTouched.text = $"{input.leftHand.GetSecondaryTouched()}";
        left.StickClicked.text = $"{input.leftHand.GetStickPressed()}";
        left.StickTouched.text = $"{input.leftHand.GetStickTouched()}";
        left.Grip.text = $"{input.leftHand.GetGripValue():F2}";
        left.GripPressed.text = $"{input.leftHand.GetGripPressed()}";
        left.Trigger.text = $"{input.leftHand.GetTriggerValue()}";
        left.TriggerPressed.text = $"{input.leftHand.GetTriggerPressed()}";
        left.StickAxis.text = $"{input.leftHand.GetStickAxis().x:F2} | {input.leftHand.GetStickAxis().y:F2}";
        
        
        
        right.PrimaryButton.text = $"{input.rightHand.GetPrimaryButton()}";
        right.PrimaryTouched.text = $"{input.rightHand.GetPrimaryTouched()}";
        right.SecondaryButton.text = $"{input.rightHand.GetSecondaryButton()}";
        right.SecondaryTouched.text = $"{input.rightHand.GetSecondaryTouched()}";
        right.StickClicked.text = $"{input.rightHand.GetStickPressed()}";
        right.StickTouched.text = $"{input.rightHand.GetStickTouched()}";
        right.Grip.text = $"{input.rightHand.GetGripValue():F2}";
        right.GripPressed.text = $"{input.rightHand.GetGripPressed()}";
        right.Trigger.text = $"{input.rightHand.GetTriggerValue()}";
        right.TriggerPressed.text = $"{input.rightHand.GetTriggerPressed()}";
        right.StickAxis.text = $"{input.rightHand.GetStickAxis().x:F2} | {input.rightHand.GetStickAxis().y:F2}";
    }

}

[System.Serializable]
public class cr_player_ui_debug_input_data
{
    public TextMeshProUGUI PrimaryButton;
    public TextMeshProUGUI PrimaryTouched;
    public TextMeshProUGUI SecondaryButton;
    public TextMeshProUGUI SecondaryTouched;
    public TextMeshProUGUI StickClicked;
    public TextMeshProUGUI StickTouched;
    public TextMeshProUGUI Grip;
    public TextMeshProUGUI GripPressed;
    public TextMeshProUGUI Trigger;
    public TextMeshProUGUI TriggerPressed;
    public TextMeshProUGUI StickAxis;
}