using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_player_spray_picker : cr_MonoBehavior
{
    public cr_player_api player;
    public cr_player_input_raw XRHand;
    public Transform XRHandRay;
    public GameObject RayOrigin;
    public LineRenderer LR;
    public GameObject RayTarget;

    public cr_player_spray_manager sprayManager;

    public LayerMask Placeable;


    public void WantsToPickSpray()
    {
        if (player.gameplayData.GetIsDead()) return;
        
        isPickingSpray = true;
    }

    public bool isPickingSpray;

    void Update()
    {
        if (player.gameplayData.GetIsDead()) isPickingSpray = false;
        
        RayOrigin.SetActive(isPickingSpray);
        RayTarget.SetActive(isPickingSpray);
        
        if(isPickingSpray)
        {
            RayOrigin.transform.position = XRHandRay.transform.position;
            RayOrigin.transform.eulerAngles = XRHandRay.transform.eulerAngles;
            
            CastTarget();
            
            LR.SetPosition(0, RayOrigin.transform.position);
            LR.SetPosition(1, RayTarget.transform.position);

            if (XRHand.GetTriggerPressed())
            {
                isPickingSpray = false;
                PlaceSprayAtCurrentTarget();
            }
        }
    }
    
    public void CastTarget()
    {
        Vector3 originPos = RayOrigin.transform.position;
        Vector3 originForward = RayOrigin.transform.forward;
        float maxDistance = 10f;
        bool bHit = false;

        if (bHit = Physics.Raycast(originPos, originForward, out RaycastHit hit, maxDistance, Placeable))
        {
            Vector3 point = hit.point;
            Vector3 normal = hit.normal;

            // Position
            RayTarget.transform.position = point;

            Quaternion rot = Quaternion.LookRotation(normal);

            RayTarget.transform.rotation = rot;

            RayTarget.transform.position += RayTarget.transform.forward * 0.01f;
        }

        RayOrigin.SetActive(bHit);
        RayTarget.SetActive(bHit);
    }

    
    
    public void PlaceSprayAtCurrentTarget()
    {
        sprayManager.PlaceDecalAt(RayTarget.transform.position, RayTarget.transform.eulerAngles);
    }
}
