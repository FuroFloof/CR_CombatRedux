using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_player_static_grab_point : MonoBehaviour
{
    private Vector3 previousPosition = Vector3.zero;

    void Awake()
    {
        previousPosition = transform.position;
    }

    public Vector3 velocity;
    public float speed;

    public void FixedUpdate()
    {
        velocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
        speed = velocity.magnitude;
        previousPosition = transform.position;
    }
}
