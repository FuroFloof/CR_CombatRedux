using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class cr_player_ui_settings_controls : cr_MonoBehavior
{
    private int RotationSpeedStep = cr_game_defaults.c_rot_speed_step;
    private string ControlsDotCFGPath => Path.Combine(cr_game.CustomGameConfigPath, "controls.cfg");

    void OnEnable()
    {
        LoadSettingsFromFiles();
    }

    #region Toggles
    public Toggle EnableYawRotToggle;
    public void ToggleEnableYawRot()
    {
        string filePath = ControlsDotCFGPath;
        ToggleConfigFileBool(filePath, nameof(cr_game_defaults.c_rot_yaw), cr_game_defaults.c_rot_yaw);

        cr_networking.localPlayer.playerController.headRot.ApplyConfigFile();
    }
    
    public Toggle EnablePitchRotToggle;
    public void ToggleEnablePitchRot()
    {
        string filePath = ControlsDotCFGPath;
        ToggleConfigFileBool(filePath, nameof(cr_game_defaults.c_rot_pitch), cr_game_defaults.c_rot_pitch);
        
        cr_networking.localPlayer.playerController.headRot.ApplyConfigFile();
    }
    
    public Toggle EnableRollRotToggle;
    public void ToggleEnableRollRot()
    {
        string filePath = ControlsDotCFGPath;
        ToggleConfigFileBool(filePath, nameof(cr_game_defaults.c_rot_roll), cr_game_defaults.c_rot_roll);
        
        cr_networking.localPlayer.playerController.headRot.ApplyConfigFile();
    }
    #endregion


    #region Sliders

    #endregion


    #region Increments
    public TextMeshProUGUI YawRotSpeedLabel;
    public void IncreaseYawRotSpeed()
    {
        string filePath = ControlsDotCFGPath;
        int max = Mathf.FloorToInt(cr_game_defaults.c_rot_yaw_speed_max);
        int min = Mathf.FloorToInt(cr_game_defaults.c_rot_yaw_speed_min);
        AddToConfigFileInt(filePath, nameof(cr_game_defaults.c_rot_yaw_speed), RotationSpeedStep, cr_game_defaults.c_rot_yaw_speed, min, max);
        
        cr_networking.localPlayer.playerController.headRot.ApplyConfigFile();
        DrawYawRotSpeedChange();
    }
    public void DecreaseYawRotSpeed()
    {
        string filePath = ControlsDotCFGPath;
        int max = Mathf.FloorToInt(cr_game_defaults.c_rot_yaw_speed_max);
        int min = Mathf.FloorToInt(cr_game_defaults.c_rot_yaw_speed_min);
        AddToConfigFileInt(filePath, nameof(cr_game_defaults.c_rot_yaw_speed), -RotationSpeedStep, cr_game_defaults.c_rot_yaw_speed, min, max);
        
        cr_networking.localPlayer.playerController.headRot.ApplyConfigFile();
        DrawYawRotSpeedChange();
    }
    public void DrawYawRotSpeedChange()
    {
        int current = Mathf.FloorToInt(cr_networking.localPlayer.playerController.headRot.yawSpeed);
        int steps = GetStepAmount(Mathf.FloorToInt(cr_game_defaults.c_rot_yaw_speed), current, RotationSpeedStep);

        YawRotSpeedLabel.text = steps.ToString();
    }
    
    
    public TextMeshProUGUI PitchRotSpeedLabel;
    public void IncreasePitchRotSpeed()
    {
        string filePath = ControlsDotCFGPath;
        int max = Mathf.FloorToInt(cr_game_defaults.c_rot_pitch_speed_max);
        int min = Mathf.FloorToInt(cr_game_defaults.c_rot_pitch_speed_min);
        AddToConfigFileInt(filePath, nameof(cr_game_defaults.c_rot_pitch_speed), RotationSpeedStep, cr_game_defaults.c_rot_pitch_speed, min, max);
        
        cr_networking.localPlayer.playerController.headRot.ApplyConfigFile();
        DrawPitchRotSpeedChange();
    }
    public void DecreasePitchRotSpeed()
    {
        string filePath = ControlsDotCFGPath;
        int max = Mathf.FloorToInt(cr_game_defaults.c_rot_pitch_speed_max);
        int min = Mathf.FloorToInt(cr_game_defaults.c_rot_pitch_speed_min);
        AddToConfigFileInt(filePath, nameof(cr_game_defaults.c_rot_pitch_speed), -RotationSpeedStep, cr_game_defaults.c_rot_pitch_speed, min, max);
        
        cr_networking.localPlayer.playerController.headRot.ApplyConfigFile();
        DrawPitchRotSpeedChange();
    }
    public void DrawPitchRotSpeedChange()
    {
        int current = Mathf.FloorToInt(cr_networking.localPlayer.playerController.headRot.pitchSpeed);
        int steps = GetStepAmount(Mathf.FloorToInt(cr_game_defaults.c_rot_pitch_speed), current, RotationSpeedStep);

        PitchRotSpeedLabel.text = steps.ToString();
    }
    
    
    public TextMeshProUGUI RollRotSpeedLabel;
    public void IncreaseRollRotSpeed()
    {
        string filePath = ControlsDotCFGPath;
        int max = Mathf.FloorToInt(cr_game_defaults.c_rot_roll_speed_max);
        int min = Mathf.FloorToInt(cr_game_defaults.c_rot_roll_speed_min);
        AddToConfigFileInt(filePath, nameof(cr_game_defaults.c_rot_roll_speed), RotationSpeedStep, cr_game_defaults.c_rot_roll_speed, min, max);
        
        cr_networking.localPlayer.playerController.headRot.ApplyConfigFile();
        DrawRollRotSpeedChange();
    }
    public void DecreaseRollRotSpeed()
    {
        string filePath = ControlsDotCFGPath;
        int max = Mathf.FloorToInt(cr_game_defaults.c_rot_roll_speed_max);
        int min = Mathf.FloorToInt(cr_game_defaults.c_rot_roll_speed_min);
        AddToConfigFileInt(filePath, nameof(cr_game_defaults.c_rot_roll_speed), -RotationSpeedStep, cr_game_defaults.c_rot_roll_speed, min, max);
        
        cr_networking.localPlayer.playerController.headRot.ApplyConfigFile();
        DrawRollRotSpeedChange();
    }
    public void DrawRollRotSpeedChange()
    {
        int current = Mathf.FloorToInt(cr_networking.localPlayer.playerController.headRot.rollSpeed);
        int steps = GetStepAmount(Mathf.FloorToInt(cr_game_defaults.c_rot_roll_speed), current, RotationSpeedStep);

        RollRotSpeedLabel.text = steps.ToString();
    }
    #endregion
    
    
    #region Commons
    public void ToggleConfigFileBool(string filePath, string varName, object defaultVar)
    {
        var parser = cr_game_file_parser.Instance;
        var currentVar = parser.GetFileVar<bool>(filePath, varName, defaultVar);
        var targetVar = !currentVar;
        parser.WriteFileVar(filePath, varName, targetVar);
    }
    
    
    public void AddToConfigFileInt(string filePath, string varName, int value, object defaultVar, int min, int max)
    {
        var parser = cr_game_file_parser.Instance;
        var currentVar = parser.GetFileVar<int>(filePath, varName, defaultVar);
        int targetVar = currentVar + value;

        targetVar = Mathf.Clamp(targetVar, min, max);

        parser.WriteFileVar(filePath, varName, targetVar);
    }
    
    
    public int GetStepAmount(int start, int current, int step)
    {
        int offset = current - start;
        int steps = offset / step;

        return steps + 5;
    }
    #endregion
    
    
    #region Apply On Load
    public void LoadSettingsFromFiles()
    {
        var parser = cr_game_file_parser.Instance;

        //apply Bools
        EnableYawRotToggle.isOn = parser.GetFileVar<bool>(ControlsDotCFGPath, nameof(cr_game_defaults.c_rot_yaw), cr_game_defaults.c_rot_yaw);
        EnablePitchRotToggle.isOn = parser.GetFileVar<bool>(ControlsDotCFGPath, nameof(cr_game_defaults.c_rot_pitch), cr_game_defaults.c_rot_pitch);
        EnableRollRotToggle.isOn = parser.GetFileVar<bool>(ControlsDotCFGPath, nameof(cr_game_defaults.c_rot_roll), cr_game_defaults.c_rot_roll);

        //apply Ints
        YawRotSpeedLabel.text = GetStepAmount(
            Mathf.CeilToInt(cr_game_defaults.c_rot_yaw_speed), 
            parser.GetFileVar<int>(ControlsDotCFGPath, 
            nameof(cr_game_defaults.c_rot_yaw_speed), cr_game_defaults.c_rot_yaw_speed), 
            RotationSpeedStep
        ).ToString();
        
        PitchRotSpeedLabel.text = GetStepAmount(
            Mathf.CeilToInt(cr_game_defaults.c_rot_pitch_speed), 
            parser.GetFileVar<int>(ControlsDotCFGPath, 
            nameof(cr_game_defaults.c_rot_pitch_speed), cr_game_defaults.c_rot_pitch_speed), 
            RotationSpeedStep
        ).ToString();
        
        RollRotSpeedLabel.text = GetStepAmount(
            Mathf.CeilToInt(cr_game_defaults.c_rot_roll_speed), 
            parser.GetFileVar<int>(ControlsDotCFGPath, 
            nameof(cr_game_defaults.c_rot_roll_speed), cr_game_defaults.c_rot_roll_speed), 
            RotationSpeedStep
        ).ToString();
    }
    #endregion
}
