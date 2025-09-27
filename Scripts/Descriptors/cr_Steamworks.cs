using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class cr_Steamworks : cr_MonoBehavior
{
    public bool _IsSteamEnabled;
    public static bool IsSteamEnabled = true;
    

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        SteamAPI.Init();
    }

    void Update()
    {
        _IsSteamEnabled = IsSteamEnabled;
    
        // if (IsSteamEnabled)
        // {
        //     try { SteamAPI.RunCallbacks(); }
        //     catch { IsSteamEnabled = false; }
        // }
    }
    void OnApplicationQuit()
    {
        if (IsSteamEnabled)
        {
            try { SteamAPI.Shutdown(); }
            catch { /* ignore */ }
            IsSteamEnabled = false;
        }
    }

    private static cr_Steamworks _instance;
    public static cr_Steamworks Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<cr_Steamworks>();
            }
            
            return _instance;
        }
    }
    
    
    #region Public APIs
    public T GetIsTF2Installed<T>(T defaultValue)
    {
        if (!IsSteamEnabled) return defaultValue;
        
        return (T)Convert.ChangeType(BIsAppInstalled(440), typeof(T));
    }
    
    public T GetPlayerDisplayname<T>(T defaultValue)
    {
        if (!IsSteamEnabled) return defaultValue;
        
        return (T)Convert.ChangeType(FGetPlayerDisplayname(), typeof(T));
    }
    
    public T UnlockAchievementClient<T>(string achName, T defaultValue)
    {
        if (!IsSteamEnabled) return defaultValue;
        
        return (T)Convert.ChangeType(AUnlockAchievementClient(achName), typeof(T));
    }

    
    
    
    
    
    #endregion
    
    
    
    #region SteamWorks Calls
    public bool BIsAppInstalled(uint appID)
    {
        bool b;
        AppId_t bAppID = new AppId_t(appID);

        b = SteamApps.BIsAppInstalled(bAppID);
        
        return b;
    }
    
    public string FGetPlayerDisplayname()
    {
        return SteamFriends.GetPersonaName();
    }
    
    public bool AUnlockAchievementClient(string achName)
    {
        bool b = SteamUserStats.SetAchievement(achName);
        SteamUserStats.StoreStats();

        return b;
    }
    
    
    
    
    
    #endregion
}


public class cr_external_data
{
    private object _value;
    
    public cr_external_data() {} // empty

    public cr_external_data(object value) // allow inline default construction
    {
        _value = value;
    }

    public void SetData<T>(T value)
    {
        _value = value;
    }

    public T GetData<T>()
    {
        if (_value is T t)
        {
            return t;   
        }
        
        throw new InvalidCastException($"Stored value is {_value?.GetType().Name ?? "null"}, not {typeof(T).Name}");
    }
}