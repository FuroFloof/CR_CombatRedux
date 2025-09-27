using UnityEngine;

public class cr_player_movement_thrusters : cr_NetworkBehavior
{
    public Rigidbody XROrigin;        // dynamic RB on your rig root
    public cr_player_input input;
    public cr_player_grab_motor grabMotor;

    [Header("Tuning")]
    [Tooltip("Top cruise speed in m/s")]
    public float maxSpeed = 4f;
    [Tooltip("How fast velocity changes toward target, m/s^2")]
    public float accel = 8f;
    [Tooltip("Optional passive damping when no input, m/s^2. Set 0 for pure drift")]
    public float passiveDamp = 0f;

    [Tooltip("Sum both hand forwards then normalize. Off means two hands give a stronger push")]
    public bool normalizeHandSum = true;
    
    

    // cached each Update, applied in FixedUpdate
    private Vector3 _wantDir;

    void Awake()
    {
        if (!XROrigin) return;
        XROrigin.useGravity = false;
        XROrigin.isKinematic = false;                       // velocity based movement
        XROrigin.drag = 0f;                                 // keep drifting
        XROrigin.angularDrag = 0f;
        XROrigin.interpolation = RigidbodyInterpolation.Interpolate;
        XROrigin.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        Vector3 dir = Vector3.zero;

        if (grabMotor.headPhys.localPlayer.gameplayData.GetIsDead()) { _wantDir = dir; return; }


        if (input.leftHand.GetSecondaryButton())
            dir += input.leftHand.GetHandTransform().forward;

        if (input.rightHand.GetSecondaryButton())
            dir += input.rightHand.GetHandTransform().forward;

        if (normalizeHandSum && dir.sqrMagnitude > 1e-6f)
            dir.Normalize();

        if (grabMotor.isGrabbing) dir = Vector3.zero; // zero out thrusters if we're grabbing.

        _wantDir = dir;
    }

    void FixedUpdate()
    {
        if (!XROrigin) return;

        Vector3 v = XROrigin.velocity;

        Vector3 targetVel;
        if (_wantDir.sqrMagnitude > 1e-6f)
        {
            targetVel = _wantDir * maxSpeed;
        }
        else
        {
            if (passiveDamp > 0f)
            {
                float dv = passiveDamp * Time.fixedDeltaTime;
                float mag = Mathf.Max(v.magnitude - dv, 0f);
                v = mag > 0f ? v.normalized * mag : Vector3.zero;
                XROrigin.velocity = v;
            }
            return;
        }

        float dvMax = accel * Time.fixedDeltaTime;
        Vector3 newVel = Vector3.MoveTowards(v, targetVel, dvMax);

        if (newVel.magnitude > maxSpeed)
        {
            newVel = newVel.normalized * maxSpeed;            
        }

        XROrigin.velocity = newVel;
    }
}