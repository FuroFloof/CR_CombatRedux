using System.IO;
using UnityEngine;

public class cr_player_movement_rotation : cr_MonoBehavior
{
    [Header("References")]
    public Transform cameraOffset;   // parent of camera and controllers
    public Transform xrHead;         // Main Camera transform
    public Transform pitchHelper;
    public cr_player_input input;

    [Header("Speeds (deg/sec)")]
    public float yawSpeed   = 140f;  // right stick X -> Y (yaw)
    public float pitchSpeed = 120f;  // right stick Y -> X (pitch)
    public float rollSpeed  = 120f;  // left  stick X -> Z (roll)

    [Header("Input")]
    public float deadzone = 0.7f;
    public bool invertPitch = true;
    public bool invertRoll = true;

    [Header("Axis space")]
    public bool yawUsesCameraAxis   = true;  // true = camera up, false = world up
    public bool pitchUsesCameraAxis = true;  // true = camera right, false = world right
    public bool rollUsesCameraAxis  = true;  // true = camera forward, false = world forward

    [Header("Smoothing")]
    public float inputSmoothing = 1f;  // 0 disables smoothing

    private Vector2 _rSmooth, _lSmooth;

    void Reset()
    {
        cameraOffset = transform;
    }
    
    public void Start()
    {
        ApplyConfigFile();
    }
    
    public override void ApplyConfigFile()
    {
        var controlsFile = Path.Combine(cr_game.CustomGameConfigPath, "controls.cfg");
        
        bool yawEnabled = cr_game_file_parser.Instance.GetFileVar<bool>(controlsFile, nameof(cr_game_defaults.c_rot_yaw), cr_game_defaults.c_rot_yaw);
        bool pitchEnabled = cr_game_file_parser.Instance.GetFileVar<bool>(controlsFile, nameof(cr_game_defaults.c_rot_pitch), cr_game_defaults.c_rot_pitch);
        bool rollEnabled = cr_game_file_parser.Instance.GetFileVar<bool>(controlsFile, nameof(cr_game_defaults.c_rot_roll), cr_game_defaults.c_rot_roll);
        
        float yawRotSpeed = cr_game_file_parser.Instance.GetFileVar<float>(controlsFile, nameof(cr_game_defaults.c_rot_yaw_speed), cr_game_defaults.c_rot_yaw_speed);
        float pitchRotSpeed = cr_game_file_parser.Instance.GetFileVar<float>(controlsFile, nameof(cr_game_defaults.c_rot_pitch_speed), cr_game_defaults.c_rot_pitch_speed);
        float rollRotSpeed = cr_game_file_parser.Instance.GetFileVar<float>(controlsFile, nameof(cr_game_defaults.c_rot_roll_speed), cr_game_defaults.c_rot_roll_speed);
        
        float rotationSmoothing = cr_game_file_parser.Instance.GetFileVar<float>(controlsFile, nameof(cr_game_defaults.c_rot_smoothing), cr_game_defaults.c_rot_smoothing);
        float c_deadzone = cr_game_file_parser.Instance.GetFileVar<float>(controlsFile, nameof(cr_game_defaults.c_deadzone), cr_game_defaults.c_deadzone);
        
        bool c_InvertPitch = cr_game_file_parser.Instance.GetFileVar<bool>(controlsFile, nameof(cr_game_defaults.c_invert_pitch), cr_game_defaults.c_invert_pitch);
        bool c_InvertRoll = cr_game_file_parser.Instance.GetFileVar<bool>(controlsFile, nameof(cr_game_defaults.c_invert_roll), cr_game_defaults.c_invert_roll);

        //APPLY

        enableYawRot = yawEnabled;
        enablePitchRot = pitchEnabled;
        enableRollRot = rollEnabled;

        yawSpeed = yawRotSpeed;
        pitchSpeed = pitchRotSpeed;
        rollSpeed = rollRotSpeed;

        inputSmoothing = rotationSmoothing;
        deadzone = c_deadzone;

        invertPitch = c_InvertPitch;
        invertRoll = c_InvertRoll;
    }

    public void ResetRotation()
    {
        yawUsesCameraAxis = false;
        //pitchUsesCameraAxis = false;
        
        ResetAllAxesKeepHead();
    }
    public void ResetHorizon()
    {
        yawUsesCameraAxis = false;
        //pitchUsesCameraAxis = false;
    
        ZeroRollKeepHead();
    }   

    void ResetAllAxesKeepHead()
    {
        if (!cameraOffset || !xrHead) return;
        Vector3 pivot = xrHead.position;
        cameraOffset.localRotation = Quaternion.identity;
        cameraOffset.position += pivot - xrHead.position;
        _rSmooth = _lSmooth = Vector2.zero;
    }

    void ZeroRollKeepHead()
    {
        if (!cameraOffset || !xrHead) return;
        Vector3 pivot = xrHead.position;
        Vector3 e = cameraOffset.localRotation.eulerAngles;
        cameraOffset.localRotation = Quaternion.Euler(e.x, e.y, 0f);
        cameraOffset.position += pivot - xrHead.position;
        _rSmooth = _lSmooth = Vector2.zero;
    }

    public bool enableYawRot;
    public bool enablePitchRot;
    public bool enableRollRot;
    
    void Update()
    {
        if (!cameraOffset || !xrHead || !pitchHelper) return;

        pitchHelper.transform.position = xrHead.position;
        pitchHelper.transform.eulerAngles = new Vector3(0, xrHead.eulerAngles.y, 0);

        // read sticks
        Vector2 r = input ? input.rightHand.GetStickAxis() : Vector2.zero;
        Vector2 l = input ? input.leftHand.GetStickAxis()  : Vector2.zero;

        if (!enableYawRot) r.x = 0;
        if (!enablePitchRot) r.y = 0;
        if (!enableRollRot) l.x = 0;

        // deadzone (square)
        r = ApplyDeadzone(r, deadzone);
        l = ApplyDeadzone(l, deadzone);

        // exponential smoothing
        float k = 1f - Mathf.Exp(-inputSmoothing * Time.deltaTime);
        // _rSmooth = Vector2.Lerp(_rSmooth, r, k);
        // _lSmooth = Vector2.Lerp(_lSmooth, l, k);


        _rSmooth = r;
        _lSmooth = l;

        // angles
        float yaw   = _rSmooth.x * yawSpeed   * Time.deltaTime;
        float pitch = (invertPitch ? -_rSmooth.y : _rSmooth.y) * pitchSpeed * Time.deltaTime;
        float roll  = (invertRoll  ? -_lSmooth.x :  _lSmooth.x) * rollSpeed  * Time.deltaTime;

        // pivot to keep head position fixed
        Vector3 pivot = xrHead.position;

        // choose axes per toggle
        Vector3 yawAxis   = yawUsesCameraAxis   ? xrHead.up      : Vector3.up;
        Vector3 pitchAxis = pitchUsesCameraAxis ? xrHead.right   : pitchHelper.right;//Vector3.right;
        Vector3 rollAxis  = rollUsesCameraAxis  ? xrHead.forward : Vector3.forward;

        // apply world-space incremental rotation about chosen axes
        Quaternion worldQ = cameraOffset.rotation;
        worldQ = Quaternion.AngleAxis(yaw,   yawAxis)   * worldQ;  // yaw first
        worldQ = Quaternion.AngleAxis(pitch, pitchAxis) * worldQ;  // then pitch
        worldQ = Quaternion.AngleAxis(roll,  rollAxis)  * worldQ;  // then roll
        cameraOffset.rotation = Normalize(worldQ);

        // re-pin head world position
        Vector3 after = xrHead.position;
        Vector3 correction = pivot - after;
        if (correction.sqrMagnitude > 0f)
        {
            cameraOffset.position += correction;
        }
        
        if(roll > 0 || pitch > 0)
        {
            yawUsesCameraAxis = true;
            //pitchUsesCameraAxis = true;
        }
    }

    static Vector2 ApplyDeadzone(Vector2 v, float dz)
    {
        float ax = Mathf.Abs(v.x), ay = Mathf.Abs(v.y);
        float x = ax > dz ? v.x : 0f;
        float y = ay > dz ? v.y : 0f;
        return new Vector2(x, y);
    }

    static Quaternion Normalize(Quaternion q)
    {
        float s = Mathf.Sqrt(q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w);
        if (s > 1e-6f) { float inv = 1f / s; q.x*=inv; q.y*=inv; q.z*=inv; q.w*=inv; }
        return q;
    }
}
