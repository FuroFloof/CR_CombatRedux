using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_dev_main_menu_manager : cr_MonoBehavior
{
    public cr_ui_main_menu_tab[] Tabs;

    public cr_ui_main_menu_button[] TabButtons;
    public cr_ui_main_menu_button[] VarButtons;

    public Sprite[] tabSprites; // 0 = selected, 1 = deselected
    public Sprite[] varSprites; // 0 = selected, 1 = deselected



    public int currentTab;
    public int currentVar;
    
    [ContextMenu(nameof(DEV_OpenCurrentTab))]
    public void DEV_OpenCurrentTab()
    {
        OpenTab(Tabs[currentTab]);
    }
    
    [ContextMenu(nameof(DEV_OpenCurrentVar))]
    public void DEV_OpenCurrentVar()
    {
        var tab = Tabs[currentTab];
        OpenVar(tab, tab.Vars[currentVar]);
    }




    void Start()
    {
        OpenMenu();
    }
    
    public void OpenMenu()
    {
        OpenTab(Tabs[0]);
    }

    public void OpenTab(cr_ui_main_menu_tab tab)
    {
        foreach(var b in TabButtons)
        {
            b.ApplyState(false);
        }
        foreach(var b in VarButtons)
        {
            b.ApplyState(false);
        }
    
        for(int i = 0; i < Tabs.Length; i++)
        {
            var cTab = Tabs[i];
            var cBtn = TabButtons[i];

            cBtn.ApplyState(true);
        
            if(cTab == tab) // tab we just opened
            {
                cBtn.button.image.sprite = tabSprites[0];
            }else
            {
                cBtn.button.image.sprite = tabSprites[1];
            }
            
            cBtn.tabRef = cTab;
            cBtn.varRef = null;
            
            cBtn.textObject.text = cTab.TabName;
        }

        LoadTabVars(tab);
        OpenVar(tab, tab.Vars[0]);
    }
    
    public void LoadTabVars(cr_ui_main_menu_tab tab)
    {
        for(int i = 0; i < tab.Vars.Length; i++) //for each var in that tab
        {
            var cVar = tab.Vars[i];
            var cBtn = VarButtons[i];

            cBtn.ApplyState(true);
            
            cBtn.tabRef = tab;
            cBtn.varRef = cVar;
            
            cBtn.textObject.text = cVar.VarName;
        }
    }
    
    public void OpenVar(cr_ui_main_menu_tab tab, cr_ui_main_menu_var var)
    {
        CloseAllTabVars();
    
        for (int i = 0; i < tab.Vars.Length; i ++)
        {
            var cVar = tab.Vars[i];
            var cBtn = VarButtons[i];
        
            if(cVar == var)
            {
                cBtn.button.image.sprite = varSprites[0];
            }else
            {
                cBtn.button.image.sprite = varSprites[1];
            }
            
            cVar.OpenContent(false);
        }
        
        var.OpenContent(true);
    }
    
    public void CloseAllTabVars()
    {
        foreach(var t in Tabs)
        {
            foreach(var v in t.Vars)
            {
                v.OpenContent(false);
            }
        }
    }
}