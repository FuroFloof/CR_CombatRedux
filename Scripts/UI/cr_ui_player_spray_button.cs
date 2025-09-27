using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cr_ui_player_spray_button : MonoBehaviour
{
    public string sprayFilePath;
    public Button button;
    public cr_ui_player_spray SprayManager;
    public int sprayID;
    
    
    public void OnButtonPressed()
    {
        SprayManager.OnSprayPicked(this);
    }
}
