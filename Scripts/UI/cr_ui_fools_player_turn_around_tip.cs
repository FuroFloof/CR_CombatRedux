using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_ui_fools_player_turn_around_tip : cr_MonoBehavior
{
    public int theBooTipIndex;

    public bool jokeIsEnabled;
    public cr_ui_loading_screen_tips_manager tips;

    public GameObject Boo;

    public void EnableDontTurnAround()
    {
        
    }
    
    public bool CheckIfStillBooTip()
    {
        return tips.currentTipIndex == theBooTipIndex;
    }

    public void PlayerTurnedAround()
    {// check if we have the right tip, then hide
        if (!CheckIfStillBooTip()) return;


        Boo.SetActive(true);
    }

    public void PlayerTurnedBackToScreen()
    {//hide my face uwu
        
        
        Boo.SetActive(false);
    }
}
