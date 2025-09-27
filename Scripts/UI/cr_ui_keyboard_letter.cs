using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class cr_ui_keyboard_letter : MonoBehaviour
{
    public string Regular;
    public string Caps;
    public string Alt;

    public Button button;
    public TextMeshProUGUI text;
    public cr_ui_keyboard_manager manager;

    void Awake()
    {
        manager.OnSpecialToggleEvent += OnSpecialToggle;
    }
    
    public void OnSpecialToggle()
    {
        if(manager.isCaps)
        {
            text.text = Caps;
        }else if(manager.isAlt)
        {
            text.text = Alt;
        }else
        {
            text.text = Regular;   
        }
    }

    public void OnButtonPressed()
    {
        manager.OnButtonPressed(this);
    }
}
