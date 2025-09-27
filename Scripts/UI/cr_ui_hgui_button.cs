using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_ui_hgui_button : cr_MonoBehavior
{
    public cr_ui_hgui_button_type buttonType;

    public cr_ui_hgui_menu menu;
    public MeshRenderer[] MRS;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("LocalPlayer")) SelectButton();
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("LocalPlayer")) DeselectButton();
    }
    
    public void SelectButton()
    {
        menu.OnButtonSelected(this);
    }
    
    public void OnButtonSelected()
    {
        foreach(var mr in MRS)
        {
            mr.material = menu.buttonSelected;
        }
    }
    
    
    public void DeselectButton()
    {
        menu.OnButtonDeselected(this);
    }
    
    public void OnDeselectButton()
    {
        foreach(var mr in MRS)
        {
            mr.material = menu.buttonUnselected;
        }
    }
}

public enum cr_ui_hgui_button_type
{
    Empty,
    Menu,
    WeaponSelect
}
