using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_ui_pos_sync : cr_MonoBehavior
{
    public Transform targetTransform;
    public bool syncPosition = true;
    public bool syncRotation;
    public bool hasRotationLatency;
    public Vector3 rotationLatency = Vector3.one;
    
    public float rotationLerpSpeed = 15f;
    Vector3 _angVel;

    void Awake()
    {
        //Debug.Log("Awake Haed UI", gameObject);
    }

    void LateUpdate()
    {
        if (!targetTransform) return;

        if (syncPosition)
        {
            transform.position = targetTransform.position;
        }

        if (!syncRotation) return;

        if (hasRotationLatency)
        {
            // Per-axis critically damped approach using SmoothDampAngle
            Vector3 cur = transform.rotation.eulerAngles;
            Vector3 tar = targetTransform.rotation.eulerAngles;

            float sx = Mathf.Max(0.0001f, rotationLatency.x);
            float sy = Mathf.Max(0.0001f, rotationLatency.y);
            float sz = Mathf.Max(0.0001f, rotationLatency.z);

            float dt = Time.deltaTime;

            cur.x = Mathf.SmoothDampAngle(cur.x, tar.x, ref _angVel.x, sx, Mathf.Infinity, dt);
            cur.y = Mathf.SmoothDampAngle(cur.y, tar.y, ref _angVel.y, sy, Mathf.Infinity, dt);
            cur.z = Mathf.SmoothDampAngle(cur.z, tar.z, ref _angVel.z, sz, Mathf.Infinity, dt);

            transform.rotation = Quaternion.Euler(cur);
        }
        else
        {
            float tExp = 1f - Mathf.Exp(-rotationLerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetTransform.rotation, tExp);
        }
    }

}
