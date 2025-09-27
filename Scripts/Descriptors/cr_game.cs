using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class cr_game : cr_MonoBehavior
{
    public string[] overrideParameters = new string[] {"-ioPath", @"G:\Project Files\Unity\Project_CR\Builds\dev\game"};
    
    

    private static cr_game _instance;
    public static cr_game Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<cr_game>();
            }
            
            return _instance;
        }
    }
    public static string GameRootPath;
    //public static string GameConfigPath;
    public static string CustomGameConfigPath;
    public static string TrueGameConfigPath;
    public static string GameSavesPath;
    public static string GameLogsPath;
    public static string GameEnginePath;
    public static string CDNURL;
    public static string APIURL;

    void Awake()
    {
        ApplyGameRootPath();
        ApplyGameMiscPaths();

        PrintTestValues();
        ApplyAPIURLS();

        ApplyMiscParams();
    }
    
    public void ApplyMiscParams()
    {
        string s = GetApplicationParameter("nosteam");

        if (s == "1")
        {//disable steam
            cr_Steamworks.IsSteamEnabled = false;
        }
        else if (s == null) cr_Steamworks.IsSteamEnabled = true;
    }

    public void PrintTestValues()
    {
        var parser = cr_game_file_parser.Instance;

        string s = parser.GetFileVar<string>(Path.Combine(CustomGameConfigPath, "template", "syntax.cfg"), "aCustomString", "Value not found");
        Debug.LogError(s);
    }
    
    public void ApplyAPIURLS()
    {
        var parser = cr_game_file_parser.Instance;

        APIURL = parser.GetFileVarOrAdd<string>(Path.Combine(CustomGameConfigPath, "networking.cfg"), "API_URL", cr_game_defaults.net_api_url);
        CDNURL = parser.GetFileVarOrAdd<string>(Path.Combine(CustomGameConfigPath, "networking.cfg"), "CDN_URL", cr_game_defaults.net_cdn_url);
    }
    
    public void ApplyGameMiscPaths()
    {
        //GameConfigPath = Path.Combine(GameRootPath, "config", "custom");
        TrueGameConfigPath = Path.Combine(GameRootPath, "config");
        CustomGameConfigPath = Path.Combine(GameRootPath, "config", "custom");
        
        #if UNITY_EDITOR
            //GameConfigPath = Path.Combine(GameRootPath, "config");
            CustomGameConfigPath = Path.Combine(GameRootPath, "config", "_custom");
        #endif
        
        GameSavesPath = Path.Combine(GameRootPath, "saves");
        GameLogsPath = Path.Combine(GameRootPath, "logs");
        GameEnginePath = Path.Combine(GameRootPath, "engine");
    }
    
    public void ApplyGameRootPath()
    {
        string s = null;
        
        s = GetApplicationParameter("ioPath");

        #if UNITY_EDITOR // if we're in the unity editor, use override params, duh
            s = GetApplicationParameter("ioPath", overrideParameters);
        #endif
        
        GameRootPath = s;
    }



    public string GetApplicationParameter(string parameterName, string[] overrideParams = null)
    {
        if (parameterName.StartsWith('-'))
        {
            parameterName = parameterName.Split('-')[1]; // if accidental -Parameter we cut the '-'
        } 
    
        string[] args = overrideParams == null ? Environment.GetCommandLineArgs() : overrideParams;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("-" + parameterName + "=", StringComparison.OrdinalIgnoreCase))
            {
                return args[i].Substring(parameterName.Length + 2);
            }
            
            if (args[i].Equals("-" + parameterName, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        
        return null;
    }
    
    public static string GetPCUsername()
    {
        return System.Environment.UserName;
    }

    public const int SV_MAX_PLAYERS = 15;
    public static string CR_MAIN_MENU_SCENE = "00_main";
}

public static class cr_game_defaults
{
    //player movement
    public const float p_max_pushoff_speed = 4f;
    public const float p_thrusters_max_speed = 4f;
    public const float p_thrusters_acceleration = 8f;
    public const bool p_thrusters_normalize = true;

    //player control defaults
    public const bool c_rot_yaw = true;
    public const bool c_rot_pitch = false;
    public const bool c_rot_roll = false;
    public const int c_rot_speed_step = 24;
    public const float c_rot_yaw_speed = 140f;
    public const float c_rot_yaw_speed_min = c_rot_yaw_speed - (c_rot_speed_step * 5);
    public const float c_rot_yaw_speed_max = c_rot_yaw_speed + (c_rot_speed_step * 9);
    public const float c_rot_pitch_speed = 120f;
    public const float c_rot_pitch_speed_min = c_rot_pitch_speed - (c_rot_speed_step * 5);
    public const float c_rot_pitch_speed_max = c_rot_pitch_speed + (c_rot_speed_step * 9);
    public const float c_rot_roll_speed = 120f;
    public const float c_rot_roll_speed_min = c_rot_roll_speed - (c_rot_speed_step * 5);
    public const float c_rot_roll_speed_max = c_rot_roll_speed + (c_rot_speed_step * 9);
    public const float c_rot_smoothing = 100f;
    public const float c_deadzone = 0.7f;
    public const bool c_invert_pitch = true;
    public const bool c_invert_roll = true;
    
    public const float c_stick_rot_reset_timer = 1f;

    //player game event defaults
    public const float p_default_health = 100f;
    public const float p_max_health = 100f;
    public const float p_default_shield = 20f;
    public const float p_max_shield = 20f;

    public const float p_default_respawn_time = 5f;

    //game default API URLS
    public const string net_api_url = "https://api-cr.monosodium.net";
    public const string net_cdn_url = "https://cdn-cr.monosodium.net";
}

public static class cr_game_defaults_tips
{
    public static string[] cr_loading_tips = { "LOADING STUB #1", "LOADING STUB #2", "LOADING STUB #3" };
}

public static class cr_game_defaults_dev_catch
{
    public const float m_match_timeout = 5f;
    public const int m_points_per_cycle = 1;
    public const float m_cycle_timer = 1f;
    public const float m_zone_cycle_timer = 10f;
    public const int m_points_till_win = 50;
    public const float m_it_thrusters_speed = 5f;
}

public static class cr_game_defaults_dev_racism
{
    public const int m_match_max_laps = 3;
    public const float m_max_thruster_speed = 15f;
}

public static class cr_game_defaults_colors 
{
    public static string GetColorHexFromColorCode(string colorCode)
    {
        switch(colorCode)
        {
            case "a": return colors[10];
            case "b": return colors[11];
            case "c": return colors[12];
            case "d": return colors[13];
            case "e": return colors[14];
            case "f": return colors[15];
        }
        
        return colors[int.Parse(colorCode)];
    }

    public static string[] colors = { 
    "#000000", // 0 Black
    "#0000AA", // 1 Dark Blue
    "#00AA00", // 2 Dark Green
    "#00AAAA", // 3 Dark Aqua
    "#AA0000", // 4 Dark Red
    "#AA00AA", // 5 Dark Purple
    "#FFAA00", // 6 Gold         
    "#AAAAAA", // 7 Gray
    "#555555", // 8 Dark Gray
    "#5555FF", // 9 Blue
    "#55FF55", // a 10 Green
    "#55FFFF", // b 11 Aqua
    "#FF5555", // c 12 Red
    "#FF55FF", // d 13 Light Purple
    "#FFFF55", // e 14 Yellow
    "#FFFFFF", // f 15 White
    };
}