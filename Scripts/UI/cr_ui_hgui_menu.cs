using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_ui_hgui_menu : cr_MonoBehavior
{
    public Material buttonSelected;
    public Material buttonUnselected;

    public Transform XRHead;
    public cr_player_input_raw hand;

    public bool isPressingMenuButton => hand.GetPrimaryButton();

    public Animator anim;


    public bool menuIsOpen;
    
    void Update()
    {
        if (hand.GetPrimaryButtonDown()) OnMenuOpen();
        if (hand.GetPrimaryButtonUp()) OnMenuClose();
    }

    public float headBackwardsOffset = -10f;
    public void OnMenuOpen()
    {
        transform.position = hand.transform.position;

        Vector3 headPos = XRHead.position;
        headPos -= XRHead.forward * headBackwardsOffset;
        
        transform.LookAt(headPos);
        
        Vector3 cRot = transform.eulerAngles;
        transform.eulerAngles = new Vector3(cRot.x, cRot.y, 0);

        anim.SetBool("isOpen", true);
    }
    
    public void OnMenuClose()
    {
        anim.SetBool("isOpen", false);
        
        FireCurrentSelectedButton();
    }

    public List<cr_ui_hgui_button> currentSelectedButtons;
    public void OnButtonSelected(cr_ui_hgui_button button)
    {
        foreach (var b in currentSelectedButtons)
        {
            b.OnDeselectButton();
        }

        if (currentSelectedButtons.Contains(button)) currentSelectedButtons.Remove(button);
        
        currentSelectedButtons.Add(button);
        button.OnButtonSelected();
    }
    
    
    public void OnButtonDeselected(cr_ui_hgui_button button)
    {
        currentSelectedButtons.Remove(button);

        button.OnDeselectButton();

        if(currentSelectedButtons.Count > 0) OnButtonSelected(currentSelectedButtons[^1]);
    }
    

    public void FireCurrentSelectedButton()
    {
        if (currentSelectedButtons.Count < 1) return; //no buttons selected lollery
        
        var button = currentSelectedButtons[^1]; // get most recent

        //Debug.Log($"Hand GUI Button {button.name} has been pressed!",  button.gameObject);
        
        switch(button.buttonType)
        {
            case cr_ui_hgui_button_type.Empty:

            break;
            
            
            case cr_ui_hgui_button_type.Menu:
                OpenPauseMenu();
            break;   
        }
    }

    public cr_ui_fgui_manager mainMenu;
    public void OpenPauseMenu()
    {
        mainMenu.OpenMenu();
    }
    
    
}