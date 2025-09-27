using System.Collections;
using System.Collections.Generic;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.XR;

public class cr_player_device_offset : cr_MonoBehavior
{
    public cr_player_hand hand;
    public GameObject c_Mdl;

    public cr_player_device_offset_data[] offsets;
    public cr_player_device_offset_data currentOffset;

    void Awake()
    {
        InitControllers();
    }

    public void InitControllers()
    {
        switch(hand.handInput.handType)
        {
            case cr_input_hand_type.right:
                PrintData(cr_player_hardware.Instance.RightController, cr_input_hand_type.right);
                break;
                
            case cr_input_hand_type.left:
                PrintData(cr_player_hardware.Instance.LeftController, cr_input_hand_type.left);
                break;
        }
    }
    
    public void PrintData(cr_player_xr_controller_info? handData, cr_input_hand_type type)
    {
        if (handData == null) return;
        if (!handData.HasValue) return;
    
        string _name = handData.Value.Name;
        string _manufacturer = handData.Value.Manufacturer;

        Debug.Log($"Hand {type} Data:");
        Debug.Log($"Name: {_name}");
        Debug.Log($"Manufacturer: {_manufacturer}");

        ApplyOffsetAndDrawModel(type, handData.Value.Brand);
    }
    
    public void ApplyOffsetAndDrawModel(cr_input_hand_type side, cr_player_hardware_brand brand)
    {
        if (c_Mdl) Destroy(c_Mdl);
        
        var dat = GetOffsetData(side, brand);
        
        currentOffset = dat;

        c_Mdl = Instantiate(dat.ModelPrefab, transform);

        c_Mdl.transform.localPosition = dat.positionalOffset;
        c_Mdl.transform.localEulerAngles = dat.rotationalOffset;
    }
    
    public cr_player_device_offset_data GetOffsetData(cr_input_hand_type side, cr_player_hardware_brand brand)
    {
        foreach(var offset in offsets)
        {
            if (offset.type == side && offset.brand == brand) { return offset; } 
        }
        
        return null;
    }
}

[System.Serializable] 
public class cr_player_device_offset_data
{
    public cr_player_hardware_brand brand;
    public cr_input_hand_type type;
    public GameObject ModelPrefab;

    public Vector3 positionalOffset;
    public Vector3 rotationalOffset;
}
