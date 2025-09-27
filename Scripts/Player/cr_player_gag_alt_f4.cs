using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks; // Steamworks.NET


public class cr_player_gag_alt_f4 : MonoBehaviour
{
    static bool s_didGagOnce = false;
    static bool isPressingGagQuitButton;
    public bool IsInGagLoading = false;
    public bool _IsPressingGagQuitButton;

    private static cr_player_gag_alt_f4 _instance;
    public static cr_player_gag_alt_f4 Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<cr_player_gag_alt_f4>();
            }
            return _instance;
        }
    }

    void Update()
    {
        bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        isPressingGagQuitButton = alt || Input.GetKey(KeyCode.F4);

        _IsPressingGagQuitButton = isPressingGagQuitButton;
    }

    void Awake()
    {
        InstallQuitHook();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void InstallQuitHook()
    {
        Application.wantsToQuit += OnWantsToQuit;
    }

    static bool OnWantsToQuit()
    {
        if (!s_didGagOnce && isPressingGagQuitButton)
        {
            if(Instance.IsInGagLoading)
            {
                s_didGagOnce = true;
                
                Debug.Log("Setting ach_alt_f4");
                cr_Achievements.Instance.UnlockClientAchievement("ACH_GAG_ALT_F4");

                // Show a UI message if available
                //if (Instance) Instance.ShowGagUI();
            }
        }

        // Allow normal quit
        return true;
    }
}
