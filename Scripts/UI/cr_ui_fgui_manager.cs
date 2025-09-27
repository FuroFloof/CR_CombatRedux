using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class cr_ui_fgui_manager : cr_MonoBehavior
{
    public GameObject Menu;
    public Animator anim;
    public cr_ui_fgui_modals[] Modals;


    void Start()
    {
        
    }


    public void OpenMenu()
    {
        CloseAllModals();

        Debug.Log("Opening Pause Menu");
    
        anim.SetBool("isOpen", true);
    }
    
    public void CloseAllModals()
    {
        foreach(var m in Modals)
        {
            m.modal.SetActive(false);
        }
    }
    
    
    public void CloseMenu()
    {
        anim.SetBool("isOpen", false);
    }


    public void QuitGameConfirm()
    {
        Debug.Log("Quitting Current Game");

        //SceneManager.LoadScene(cr_game_loader.FindBuildIndexByName(cr_game.CR_MAIN_MENU_SCENE));
        cr_game_loader.Instance.StartLocalSingle(cr_game.CR_MAIN_MENU_SCENE);
    }



    #region Controls
    public void OpenModal(string modalName)
    {
        var m = GetModalByName(modalName);
        if (m == null) return;

        m.modal.SetActive(true);
    }
    public void CloseModal(string modalName)
    {
        var m = GetModalByName(modalName);
        if (m == null) return;

        m.modal.SetActive(false);
    }
    public cr_ui_fgui_modals GetModalByName(string modalName)
    {
        for(int i = 0; i < Modals.Length; i++)
        {
            if (Modals[i].modalName == modalName) return Modals[i];
        }

        return null;
    }
    #endregion
}

[System.Serializable]
public class cr_ui_fgui_modals
{
    public string modalName;
    public GameObject modal;
}