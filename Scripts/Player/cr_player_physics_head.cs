using Unity.XR.CoreUtils;
using UnityEngine;

public class cr_player_physics_head : cr_MonoBehavior
{
    [Header("Refs")]
    public Rigidbody rb;                 // XR Origin rigidbody (dynamic)
    public Transform XRHead;             // HMD transform
    public Transform cameraOffset;       // parent of camera/controllers
    public SphereCollider headSphere;    // on XR Origin
    public cr_player_grab_motor grabMotor;
    public cr_player_api localPlayer;

    [Header("Head shape")]
    public float radius = 0.125f;
    public Vector3 headOffset = new Vector3(0f, -0.08f, 0.08f);

    [Header("Collision")]
    public LayerMask collisionMask = ~0;
    public float skin = 0.01f;           // small gap to avoid sticking
    public float sweepBias = 0.0005f;    // tiny push off the surface to reduce re-contact

    [Header("Comfort")]
    public float corrSmoothTime = 0.06f; // seconds to ease Camera Offset correction
    public float corrMaxSpeed = 6f;      // max correction speed in m/s

    // state from FixedUpdate -> LateUpdate
    private Vector3 _allowedWorld;
    private Vector3 _hitNormal;
    private bool _blocked;

    // smoothing for cameraOffset
    private Vector3 _camVel;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!headSphere) headSphere = GetComponent<SphereCollider>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.drag = 0f;
        rb.angularDrag = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        headSphere.radius = radius;
        headSphere.isTrigger = false; // give it a frictionless PhysicMaterial
        
        localPlayer.gameplayData.OnPlayerDeathEvent += OnDeath;
    }
    
    void OnDeath(int playerID)
    {
        rb.velocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (!XRHead || !headSphere) return;

        // current sphere center in world
        Vector3 center = transform.TransformPoint(headSphere.center);

        // target head center from tracking
        Vector3 desired = XRHead.position + XRHead.rotation * headOffset;
        
        // if(grabMotor.isGrabbing && grabMotor.currentStaticGrabHand.StaticGrabPoint)
        // {
        //     desired += grabMotor.currentStaticGrabHand.GetHandGrabOffset();
        // }

        Vector3 delta = desired - center;

        if (cr_networking.localPlayer.gameplayData.GetIsDead()) delta = Vector3.zero; // if we're dead, no need to handle grab logic
        
        float dist = delta.magnitude;

        _blocked = false;
        _hitNormal = Vector3.zero;
        _allowedWorld = desired;

        if (dist > 1e-5f)
        {
            Vector3 dir = delta / dist;

            // Sphere sweep to stop before walls - prevents center teleporting through geometry
            if (Physics.SphereCast(center, radius, dir, out RaycastHit hit, dist + skin, collisionMask, QueryTriggerInteraction.Ignore))
            {
                _blocked = true;
                _hitNormal = hit.normal;
                float moveDist = Mathf.Max(hit.distance - skin, 0f);
                _allowedWorld = center + dir * moveDist + hit.normal * sweepBias;
            }
        }

        // sanity overlap push-out if somehow inside
        if (!_blocked && Physics.CheckSphere(_allowedWorld, radius, collisionMask, QueryTriggerInteraction.Ignore))
        {
            Collider[] cols = Physics.OverlapSphere(_allowedWorld, radius + skin, collisionMask, QueryTriggerInteraction.Ignore);
            float best = 0f; Vector3 bestDir = Vector3.zero;
            foreach (var c in cols)
            {
                if (!c) continue;
                Vector3 p = c.ClosestPoint(_allowedWorld);
                Vector3 outDir = _allowedWorld - p;
                float d = outDir.magnitude;
                if (d < 1e-5f) continue;
                float need = radius + skin - d;
                if (need > best) { best = need; bestDir = outDir / d; }
            }
            if (best > 0f)
            {
                _blocked = true;
                _hitNormal = bestDir;
                _allowedWorld += bestDir * best;
            }
        }

        // write the sphere center (local) to the allowed point - preserves your smooth feel
        Vector3 allowedLocal = transform.InverseTransformPoint(_allowedWorld);
        headSphere.center = allowedLocal;
    }

    void LateUpdate()
    {
        if (_skipCorrections > 0){ _skipCorrections--; return; }
        
        if (!XRHead || !cameraOffset) return;

        if (!_blocked)
        {
            // nothing to fix: let playspace pass through, decay smoothing
            _camVel = Vector3.Lerp(_camVel, Vector3.zero, 10f * Time.deltaTime);
            return;
        }

        // where the HMD wants to be this frame
        Vector3 desired = XRHead.position + XRHead.rotation * headOffset;

        // world correction needed to keep the HMD outside geometry
        Vector3 corr = _allowedWorld - desired;

        // only cancel the into-wall component so you can still move tangentially
        Vector3 normalCorr = _hitNormal.sqrMagnitude > 0f ? Vector3.Project(corr, _hitNormal) : corr;

        if (normalCorr.sqrMagnitude < 1e-10f) return;

        Vector3 target = cameraOffset.position + normalCorr;
        cameraOffset.position = Vector3.SmoothDamp(
            cameraOffset.position,
            target,
            ref _camVel,
            corrSmoothTime,
            corrMaxSpeed
        );
    }
    
    int _skipCorrections;
    public void TeleportHead(Vector3 targetHeadWorldPos, Vector3 faceForward, bool zeroVel = false)
    {
        if (!rb || !XRHead) return;

        if (zeroVel) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        // Move origin by the delta needed to put the head at the target
        Vector3 delta = targetHeadWorldPos - XRHead.position;
        rb.position += delta;
        Physics.SyncTransforms();

        // Optional yaw while keeping the head pinned
        if (cameraOffset && faceForward.sqrMagnitude > 1e-6f)
        {
            Vector3 targetF = faceForward; targetF.y = 0f;
            Vector3 headF   = XRHead.forward; headF.y = 0f;
            if (targetF.sqrMagnitude > 1e-6f && headF.sqrMagnitude > 1e-6f)
            {
                targetF.Normalize(); headF.Normalize();
                float yaw = Vector3.SignedAngle(headF, targetF, Vector3.up);

                Vector3 pivot = XRHead.position;                      // pin the head
                Quaternion pre = cameraOffset.localRotation;
                cameraOffset.localRotation = pre * Quaternion.AngleAxis(yaw, Vector3.up);
                cameraOffset.position += (pivot - XRHead.position);   // cancel drift
                Physics.SyncTransforms();
            }
        }

        // Optional: keep your head-sphere aligned for next physics tick
        if (headSphere)
        {
            Vector3 desired = XRHead.position + XRHead.rotation * headOffset;
            headSphere.center = transform.InverseTransformPoint(desired);
        }

        // Debug to verify pinning
        Debug.Log($"Teleport residual: {(XRHead.position - targetHeadWorldPos)}");
    }


    
    
    public void TeleportHeadTo(Vector3 targetHeadCenter, float? yawDegrees = null, bool zeroVelocity = true, LayerMask mask = default)
    {
        if (!rb || !headSphere || !XRHead) return;
        if (mask == default) mask = ~0;

        // Current head-sphere center in world
        Vector3 centerNow = transform.TransformPoint(headSphere.center);
        //Vector3 centerNow = XRHead.position + XRHead.rotation * headOffset;


        // 1) Safe path: spherecast to target to avoid spawning inside walls
        Vector3 toTarget = targetHeadCenter - centerNow;
        Vector3 newRootPos = rb.position;

        if (toTarget.sqrMagnitude > 1e-6f)
        {
            Vector3 dir = toTarget.normalized;
            float dist = toTarget.magnitude;

            if (Physics.SphereCast(centerNow, radius, dir, out RaycastHit hit, dist, mask, QueryTriggerInteraction.Ignore))
            {
                // stop just before the obstacle
                float moveDist = Mathf.Max(hit.distance - skin, 0f);
                newRootPos += dir * moveDist;
            }
            else
            {
                newRootPos += toTarget;
            }
        }

        // 2) Commit the teleport: set position directly, clear velocities if desired
        if (zeroVelocity) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        rb.position = newRootPos;
        Physics.SyncTransforms();
        
        if (XRHead)
        {
            Vector3 localHead = XRHead.localPosition; // playspace offset
            Vector3 worldDelta = cameraOffset ? cameraOffset.TransformVector(localHead) : transform.TransformVector(localHead);
            rb.position += worldDelta;
            Physics.SyncTransforms();
        }

    
        // 3) Optional yaw rotate about the HMD pivot while keeping the HMD world position fixed
        if (yawDegrees.HasValue && cameraOffset)
        {
            Vector3 pivot = XRHead.position; // head stays put in world
            Quaternion preRot = cameraOffset.localRotation;
            cameraOffset.localRotation = preRot * Quaternion.AngleAxis(yawDegrees.Value, Vector3.up);
            Vector3 after = XRHead.position;
            cameraOffset.position += (pivot - after); // keep head pinned
        }

        // 4) Finalize: snap sphere to the HMD head-center locally so future frames are aligned
        Vector3 desiredHeadCenter = XRHead.position + XRHead.rotation * headOffset;
        headSphere.center = transform.InverseTransformPoint(desiredHeadCenter);

        // 5) If the target spot was inside geometry, push out minimally
        if (Physics.CheckSphere(desiredHeadCenter, radius, mask, QueryTriggerInteraction.Ignore))
        {
            Collider[] cols = Physics.OverlapSphere(desiredHeadCenter, radius + skin, mask, QueryTriggerInteraction.Ignore);
            float best = 0f; Vector3 bestDir = Vector3.zero;
            foreach (var c in cols)
            {
                if (!c) continue;
                Vector3 p = c.ClosestPoint(desiredHeadCenter);
                Vector3 outDir = desiredHeadCenter - p;
                float d = outDir.magnitude; if (d < 1e-5f) continue;
                float need = radius + skin - d;
                if (need > best) { best = need; bestDir = outDir / d; }
            }
            if (best > 0f)
            {
                rb.position += bestDir * best;
                Physics.SyncTransforms();
                // resnap center after the tiny push
                desiredHeadCenter = XRHead.position + XRHead.rotation * headOffset;
                headSphere.center = transform.InverseTransformPoint(desiredHeadCenter);
            }
        }
        
        _skipCorrections = 1;
    }
    

    void OnDrawGizmosSelected()
    {
        if (!headSphere) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.TransformPoint(headSphere.center), radius);
    }
}