using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class cr_ui_keyboard_manager : MonoBehaviour
{
    private static cr_ui_keyboard_manager _Instance;
    public static cr_ui_keyboard_manager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<cr_ui_keyboard_manager>();
            }
            return _Instance;
        }
    }
    
    void Start()
    {
        ToggleCaps();
    }
    
    
    public bool isCaps = true;
    public bool isAlt;
    private TMP_InputField TargetInputField;

    public void ToggleCaps()
    {
        isCaps = !isCaps;
        isAlt = false;

        OnSpecialToggle();
    }
    public void ToggleAlt()
    {
        isAlt = !isAlt;
        isCaps = false;
        
        OnSpecialToggle();
    }
    public void AddWhiteSpace()
    {
        AddCharacterToInputField(" ");
    }
    public void Backspace()
    {
        if (!TargetInputField) return;
        if (TargetInputField.text.Length < 1) return;
        
        TargetInputField.text = TargetInputField.text[..^1]; // remove last char
    }

    public void OnSpecialToggle()
    {
        OnSpecialToggleEvent?.Invoke();
    }
    public event Action OnSpecialToggleEvent; // event that's subscribbled to

    public void OnButtonPressed(cr_ui_keyboard_letter letter)
    {
        AddCharacterToInputField(letter.text.text);
    }
    
    public void AddCharacterToInputField(string character)
    {
        if (!TargetInputField) return;
        TargetInputField.text += character;
    }

    private Sprite defaultSprite;
    public void OnInputFieldSelected(TMP_InputField field)
    {
        if (defaultSprite == null) defaultSprite = field.image.sprite;
        
        if(TargetInputField != field)
        {
            if(TargetInputField)
            {
                field.image.sprite = defaultSprite;
            }
            
            TargetInputField = field;
        }
    }
    
    public void OnInputFieldDeselected(TMP_InputField field)
    {
        if(TargetInputField)
        {
            field.image.sprite = TargetInputField.spriteState.selectedSprite;
        }
    }
    
    // [ContextMenu("DEV/Assign Button Refs")]
    // public void DEVAssignButtons()
    // {
    //     var buttons = FindObjectsOfType<cr_ui_keyboard_letter>();
        
    //     foreach(var b in buttons)
    //     {
    //         var buttonText = b.GetComponentInChildren<TextMeshProUGUI>();
    //         var buttonButton = b.GetComponent<Button>();
            
    //         b.button = buttonButton;
    //         b.text = buttonText;
    //     }
    // }
    // [ContextMenu("DEV/Assign Button On-Click Event")]
    // public void DEVAssignButtonOnClickEvent()
    // {
    //     var buttons = FindObjectsOfType<cr_ui_keyboard_letter>();
        
    //     foreach(var b in buttons)
    //     {
    //         var buttonButton = b.GetComponent<Button>();
            
    //         UnityEventTools.AddPersistentListener(buttonButton.onClick, b.OnButtonPressed);
    //     }
    // }
}
