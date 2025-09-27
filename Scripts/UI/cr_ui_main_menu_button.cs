using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class cr_ui_main_menu_button : cr_MonoBehavior
{
    public cr_dev_main_menu_manager manager;

    public Button button;
    public TextMeshProUGUI textObject;
    
    [NonSerialized] public cr_ui_main_menu_tab tabRef;
    [NonSerialized] public cr_ui_main_menu_var varRef;
    
    public void OnButtonPressed()
    {
        if(tabRef && !varRef) // we're a tabs
        {
            manager.OpenTab(tabRef);
        }else if(tabRef && varRef) // we're a tab!
        {
            manager.OpenVar(tabRef, varRef);
        }
        
    }
    
    
    public void ApplyState(bool enabled)
    {
        gameObject.SetActive(enabled);
    }
}
