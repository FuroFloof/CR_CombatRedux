using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_Achievements : cr_MonoBehavior
{
    private static cr_Achievements _instance;
    public static cr_Achievements Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<cr_Achievements>();
            }
            return _instance;
        }
    }


    public void UnlockClientAchievement(string achName)
    {
        cr_Steamworks.Instance.UnlockAchievementClient(achName, false);
    }
}
