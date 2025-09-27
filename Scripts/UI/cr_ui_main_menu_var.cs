using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_ui_main_menu_var : cr_MonoBehavior
{
    public string VarName = "TVar";

    public cr_ui_main_menu_tab tab;
    public GameObject Content;
    
    public void OpenContent(bool b)
    {
        Content.SetActive(b);
    }
}
