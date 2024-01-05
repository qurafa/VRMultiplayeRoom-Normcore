using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Hands;

[RequireComponent(typeof(Rigidbody))]
public class CallibrateRoom : MonoBehaviour
{
    /// <summary>
    /// Room we're calibrating
    /// </summary>
    [SerializeField]
    private GameObject _room;
    /// <summary>
    /// What the room shouuld rotate about
    /// </summary>
    [SerializeField]
    private GameObject _rotationReference;
    /// <summary>
    /// Reference to the center of the player in the room
    /// </summary>
    [SerializeField]
    private GameObject _playerCenterReference;

    /// <summary>
    /// Objects to be ignored when callibrating the room
    /// Added so things like object collision does not affect
    /// </summary>
    [SerializeField]
    private List<GameObject> _ignores;

    /*    private List<Transform> _ignores;
        private List<Rigidbody> _ignoresRigidbody;*/
    /*    /// <summary>
        /// The RealtimeView of the Room
        /// </summary>
        [SerializeField]
        private RealtimeView _rtView;
        /// <summary>
        /// The RealtimeTransform of the Room
        /// </summary>
        [SerializeField]
        private RealtimeTransform _rtTransform;*/
    /// <summary>
    /// realtimeHelper to help us join the room
    /// </summary>
    [SerializeField]
    private realtimeHelper _rtHelper;
    /// <summary>
    /// Event to be triggered after we're done calibrating the room
    /// </summary>
    [SerializeField]
    private MyLoadSceneEvent _doneEvent;
    [SerializeField]
    private bool _doneDebug;
    /// <summary>
    /// Passthrough layer to toggle passthrough for the users at runtime
    /// </summary>
    /*[SerializeField]
    private OVRPassthroughLayer _passthroughLayer;*/

    private Rigidbody _roomRB;
    private List<Transform> _listOfChildren = new List<Transform>();
    /// <summary>
    /// We turn this off when we're done calibrating
    /// </summary>
    [SerializeField]
    private bool _canCalibrate = true;//we turn this off after we're done calibrating

    /// <summary>
    /// Texts to provide ui feedback to the user
    /// 0 - Standby, 1 - Position, 2 - Rotation
    /// </summary>
    [Header("Modes")]
    [SerializeField]
    private TMP_Text[] _modes = new TMP_Text[3];
    /// <summary>
    /// Passthrough text to provide visual feedback
    /// </summary>
    [SerializeField]
    private TMP_Text passThrough;
    [SerializeField]
    private Material[] _seeThroughInPassThroughMaterials = new Material[0];
    [SerializeField]
    private Color UI_NOT_SELECTED = Color.white;
    [SerializeField]
    private Color UI_SELECTED = Color.blue;

    private InputDevice leftHandInput;
    private InputDevice rightHandInput;

    /// <summary>
    /// What we send to the SceneLoader as we move to the next scene
    /// </summary>
    private MyTransform _send;
    GameObject _rotRef;
    
    enum Vision
    {
        Normal,
        Passthrough
    }
    private Vision _vision;

    enum Mode
    {
        Standby,
        CalibratingPos,
        CalibratingRot,
        Done
    }
    private Mode _mode;

    [SerializeField]
    private float direction = 0.0f;
    public readonly float rotFactor = 0.05f;
    public readonly float posFactor = 0.0125f;

    [SerializeField]
    Mode mode
    {
        get { return _mode; }
        set
        {
            _mode = value;

            ModeChanged();
            ToggleModeUI();
        }
    }

    Vision vision
    {
        get { return _vision; }
        set
        {
            _vision = value;

            VisionChanged();
        }
    }

    void OnEnable()
    {
        //getting left and hright hand/controller inputbest
        leftHandInput = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightHandInput = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void Start()
    {
        if (!_rotationReference)
            _rotationReference = GameObject.FindWithTag("MainCamera");

        _roomRB = _room.GetComponent<Rigidbody>();
        mode = Mode.Standby;

        //saving the starting transform
        _send = new MyTransform(transform.position, transform.eulerAngles);
        _rotRef = Instantiate(new GameObject("_rotRef"), _rotationReference.transform);
    }

    // Update is called once per frame
    void Update()
    {
        //only if we can can calibrate and are holding the controllers
        if (_canCalibrate)
        {
            rightHandInput.TryGetFeatureValue(CommonUsages.primaryButton, out bool aPressed);
            rightHandInput.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bPressed);
            leftHandInput.TryGetFeatureValue(CommonUsages.primaryButton, out bool xPressed);
            leftHandInput.TryGetFeatureValue(CommonUsages.secondaryButton, out bool yPressed);
            leftHandInput.TryGetFeatureValue(CommonUsages.menuButton, out bool menuPressed);

            //if the A button is pressed
            if (aPressed)
            {
                Debug.Log("A was pressed");

                switch (mode)
                {
                    case Mode.Standby:
                        mode = Mode.CalibratingPos;
                        break;
                    default:
                        Debug.Log($"Switch from {mode.ToString()} to standby");
                        mode = Mode.Standby;
                        return;
                }
            }
            //if the B or Y button is pressed
            if (bPressed || yPressed)
            {
                switch (mode)
                {
                    case Mode.Standby:
                        mode = Mode.CalibratingRot;
                        direction = bPressed ? 1 : -1;
                        break;
                    default:
                        Debug.Log($"Switch from {mode.ToString()} to standby");
                        mode = Mode.Standby;
                        return;
                }
            }
            //if the X button is pressed switch vision
            if (xPressed)
            {
                switch (vision)
                {
                    case Vision.Normal:
                        vision = Vision.Passthrough;
                        break;
                    case Vision.Passthrough:
                        vision = Vision.Normal;
                        break;
                    default:
                        vision = Vision.Normal;
                        return;
                }
            }
            //if the "option" or "|||" or "done" button is pressed
            if (menuPressed || _doneDebug)
            {
                mode = Mode.Done;
            }

            if (mode == Mode.CalibratingPos)
            {
                //get the joystick inputs

                rightHandInput.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rAxes);//OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
                leftHandInput.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 lAxes);
                _rotRef.transform.eulerAngles = new Vector3(0, _rotationReference.transform.eulerAngles.y, 0);
                _room.transform.Translate(new Vector3(rAxes.x, lAxes.y, rAxes.y) * posFactor, _rotRef.transform);
            }

            if (mode == Mode.CalibratingRot)
                _roomRB.transform.RotateAround(_rotationReference.transform.position, Vector3.up, rotFactor * direction);
        }
    }

    private void ModeChanged()
    {
        if (mode == Mode.Standby)
        {
            //freeze everything, inlcuding position and rotation
            //hide the passthrough layer
            //stop all room rotations
            //set the material colors right
            _roomRB.constraints = RigidbodyConstraints.FreezeAll;
            direction = 0;


        }
        else if (mode == Mode.CalibratingPos)
        {
            //only freeze the room rotating
            _roomRB.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else if (mode == Mode.CalibratingRot)
        {
            //only freeze the x, y and z rotations
            _roomRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePosition;
        }
        else if (mode == Mode.Done)
        {
            //done, so send the player's information wrt to the room to the next scene
            _send = new MyTransform(_playerCenterReference.transform.position - _room.transform.position,
            _playerCenterReference.transform.eulerAngles, _room.transform.eulerAngles);

            vision = Vision.Normal;

            if (_doneEvent != null)
                _doneEvent.Invoke(_send, 1);

            //stop ability to calibrate
            _canCalibrate = false;
        }
        else
        {
            Debug.Log("Error with ModeChanged(), mode not set properly");
        }
    }

    private void VisionChanged()
    {
        if (vision == Vision.Normal)
        {
            foreach (Material m in _seeThroughInPassThroughMaterials)
            {
                Color c = m.color;
                c.a = 1;
                m.color = c;
            }
            passThrough.color = UI_NOT_SELECTED;
        }
        else if (vision == Vision.Passthrough)
        {
            //passthrough layer is always active

            foreach (Material m in _seeThroughInPassThroughMaterials)
            {
                Color c = m.color;
                c.a = 0;
                m.color = c;
            }
            passThrough.color = UI_SELECTED;
        }
        else
        {
            Debug.Log("Problem with setting up vision change");
        }
    }

    /*    private void ToggleOwnership(bool val)
        {
            _listOfChildren.Clear();
            GetChildRecursive(_room.transform);
            if (val)
            {
                if (_rtView) _rtView.RequestOwnership();
                if (_rtTransform) _rtTransform.RequestOwnership();
                foreach (Transform t in _listOfChildren)
                {
                    if (t.TryGetComponent<RealtimeView>(out RealtimeView rTV))
                        rTV.RequestOwnership();
                    if (t.TryGetComponent<RealtimeTransform>(out RealtimeTransform rTT))
                        rTT.RequestOwnership();
                }
            }
            else
            {
                _rtView.ClearOwnership();
                _rtTransform.ClearOwnership();
                foreach (Transform t in _listOfChildren)
                {
                    if (t.TryGetComponent<RealtimeView>(out RealtimeView rTV))
                        rTV.ClearOwnership();
                    if (t.TryGetComponent<RealtimeTransform>(out RealtimeTransform rTT))
                        rTT.ClearOwnership();
                }
            }  
        }*/

    private void GetChildRecursive(Transform obj)
    {
        if (null == obj)
            return;

        foreach (Transform child in obj)
        {
            if (null == child)
                continue;

            if (child != obj)
            {
                _listOfChildren.Add(child);
            }
            GetChildRecursive(child);
        }
    }

    private void ToggleModeUI()
    {
        foreach (TMP_Text mode in _modes)
            mode.color = UI_NOT_SELECTED;

        if ((int)mode < _modes.Length)
            _modes[(int)mode].color = UI_SELECTED;
    }

    private void ToggleIgnores(bool val)
    {
        foreach (GameObject g in _ignores)
        {
            g.SetActive(val);
        }
    }

    private void OnHandTrackingAcquired(XRHand hand)
    {

    }

    private void OnHandTrackingLost(XRHand hand)
    {

    }

    /*    private void SaveTransform()
        {
            if (_ignoresTransform == null || _ignoresTransform.Count <= 0 || _ignoresTransform.Count < _ignores.Count)
            {
                _ignoresTransform = new List<Transform>();
                for (int i = 0; i < _ignores.Count; i++)
                    _ignoresTransform.Add(_ignores[i]);
            }
            else
            {
                for (int i = 0; i < _ignores.Count; i++)
                    _ignoresTransform[i] = _ignores[i];
            }
        }

        private void SetTransform()
        {
            if (_ignoresTransform == null || _ignoresTransform.Count < _ignores.Count)
                SaveTransform();

            for (int i = 0; i < _ignores.Count; i++)
                _ignores[i].SetPositionAndRotation(_ignoresTransform[i].position, _ignoresTransform[i].rotation);
        }

        private void SetLocalTransform()
        {
            if (_ignoresTransform == null || _ignoresTransform.Count < _ignores.Count)
                SaveTransform();

            for (int i = 0; i < _ignores.Count; i++)
                _ignores[i].SetLocalPositionAndRotation(_ignoresTransform[i].localPosition, _ignoresTransform[i].localRotation);
        }*/
}

[System.Serializable]
public class MyLoadSceneEvent : UnityEvent<MyTransform, int>
{
}

public struct MyTransform
{
    public MyTransform(Vector3 pos, Vector3 eul)
    {
        position = pos;
        eulerAngles = eul;
        rotAbt = Vector3.zero;
    }

    public MyTransform(Vector3 pos, Vector3 eul, Vector3 rotA)
    {
        position = pos;
        eulerAngles = eul;
        rotAbt = rotA;
    }

    public Vector3 position { get; }
    public Vector3 eulerAngles { get; }
    public Vector3 rotAbt { get; }
}
