using Normal.Realtime;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using static UnityEngine.UI.Image;

public class HandSyncImpl : MonoBehaviour
{
    [SerializeField]
    private Handedness _handedness;
    [SerializeField]
    private SkinnedMeshRenderer _mySkinMeshRendererHT;
    [SerializeField]
    private SkinnedMeshRenderer _mySkinMeshRendererC;
    [SerializeField]
    private RealtimeView _rtView;
    [SerializeField]
    private Transform controller;
    [SerializeField]
    private Animator hCAnimator;

    private HandSync _handSync;

    private Transform[] _joints = new Transform[26];
    private bool _jointsAssigned = false;
    private Transform handOrigin;

    private XRHandSubsystem _mSubSystem;
    static readonly List<XRHandSubsystem> s_SubsystemsReuse = new List<XRHandSubsystem>();

    private bool _handTrackingAcquired = false;
    private bool _controllerTrackingAcquired = false;

    private XROrigin origin;
    private Transform cameraOffset;
    private XRHand hand;
    private UnityEngine.XR.InputDevice controllerDevice;
    private bool cDAssigned = false;

    // Start is called before the first frame update
    void Start()
    {
        origin = FindObjectOfType<XROrigin>();
        cameraOffset = origin.gameObject.transform.GetChild(0);
        _rtView = GetComponent<RealtimeView>();
        _handSync = GetComponent<HandSync>();

        _mySkinMeshRendererHT.enabled= false;
        _mySkinMeshRendererC.enabled= false;

        AssignJoints(transform.GetChild(0));//assigning the joints by giving the root as the wrist
        _jointsAssigned = true;

        AssignHandOrigin();
    }

    // Update is called once per frame
    void Update()
    {
        if (!cDAssigned)
            AssignControllerDevice();

        UpdateToNormcore();

        GetSubsystem();

        HandTrackingAcquired();
        ControllerTrackingAcquired();
    }

    private void OnDisable()
    {
        if (_mSubSystem != null)
        {
            _mSubSystem.trackingAcquired -= OnTrackingAcquired;
            _mSubSystem.trackingLost -= OnTrackingLost;
            _mSubSystem.updatedHands -= UpdateHands;
            _mSubSystem = null;
        }
            
    }
    
    private bool HandTrackingAcquired()
    {
        if (_mSubSystem != null)
        {
            switch (_handedness)
            {
                case Handedness.Left:
                    if (_mSubSystem.leftHand.isTracked) _handTrackingAcquired = true;
                    else _handTrackingAcquired = false;
                    break;
                case Handedness.Right:
                    if (_mSubSystem.rightHand.isTracked) _handTrackingAcquired = true;
                    else _handTrackingAcquired = false;
                    break;
                default:
                    Debug.Log("handedness not set");
                    break;
            }
            return _handTrackingAcquired;
        }

        return _handTrackingAcquired;
    }

    private bool ControllerTrackingAcquired()
    {
        UnityEngine.InputSystem.XR.XRController controllerDevice = null;

        switch (_handedness)
        {
            case Handedness.Left:
                controllerDevice = UnityEngine.InputSystem.InputSystem.GetDevice<UnityEngine.InputSystem.XR.XRController>(UnityEngine.InputSystem.CommonUsages.LeftHand);
                break;
            case Handedness.Right:
                controllerDevice = UnityEngine.InputSystem.InputSystem.GetDevice<UnityEngine.InputSystem.XR.XRController>(UnityEngine.InputSystem.CommonUsages.RightHand);
                break;
            default:
                Debug.Log("handedness not set");
                break;
        }

        _controllerTrackingAcquired = (controllerDevice != null);

        return _controllerTrackingAcquired;
    }

    /// <summary>
    /// Get the XRHandSubsystem
    /// </summary>
    /// <returns>The XRHandSubsystem or null if it cant be found</returns>
    private void GetSubsystem()
    {
        if (_mSubSystem == null)
        {
            SubsystemManager.GetSubsystems(s_SubsystemsReuse);
            if (s_SubsystemsReuse.Count == 0)
                return;

            _mSubSystem = s_SubsystemsReuse[0];
            _mSubSystem.trackingAcquired += OnTrackingAcquired;
            _mSubSystem.trackingLost += OnTrackingLost;
            _mSubSystem.updatedHands += UpdateHands;

            //set the hand info after setting the subsystem
            AssignHand();

            return;
        }
        return;
    }

    private void AssignHand()
    {
        if (_mSubSystem != null)
        {
            switch (_handedness)
            {
                case Handedness.Left:
                    hand = _mSubSystem.leftHand;
                    break;
                case Handedness.Right:
                    hand = _mSubSystem.rightHand;
                    break;
                default:
                    Debug.Log("handedness not set");
                    break;
            }
            return;
        }

        return;
    }

    private void AssignHandOrigin()
    {
        switch (_handedness)
        {
            case Handedness.Left:
                handOrigin = origin.transform.Find("CameraOffset/Left Controller");
                break;
            case Handedness.Right:
                handOrigin = origin.transform.Find("CameraOffset/Right Controller");
                break;
            default:
                Debug.Log("XROrigin not present in scene or handedness not set");
                break;
        }
        return;
    }

    private void AssignControllerDevice()
    {
        switch (_handedness)
        {
            case Handedness.Left:
                controllerDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                cDAssigned = true;
                break;
            case Handedness.Right:
                controllerDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                cDAssigned = true;
                break;
            default:
                Debug.Log("XROrigin not present in scene or handedness not set");
                break;
        }
        return;
    }

    /// <summary>
    /// Adds the joints to the _joints array at the right index, ideally start by providing the wrist game object
    /// </summary>
    /// <param name="root"></param>
    private void AssignJoints(Transform root)
    {
        for(int j = 0; j < _joints.Length; j++)
        {
            if (root.name.ToLower().Contains(XRHandJointIDUtility.FromIndex(j).ToString().ToLower()))
            {
                _joints[j] = root;
                //Debug.Log("Assigned " + root.name);
                break;
            }
            //Debug.Log("Joints index " + j + " is null " + _handedness.ToString());
        }
        for(int c = 0; c < root.childCount; c++)
            AssignJoints(root.GetChild(c));
        return;
    }

    void OnTrackingAcquired(XRHand hand)
    {
        if (hand.handedness.Equals(_handedness))
            _handTrackingAcquired = true;
    }

    void OnTrackingLost(XRHand hand)
    {
        if (hand.handedness.Equals(_handedness))
            _handTrackingAcquired = false;
    }

    private void UpdateHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        if (updateType == XRHandSubsystem.UpdateType.Dynamic)
            return;

        UpdateToNormcore();
    }

    private void UpdateToNormcore()
    {
        if (!_jointsAssigned)
            return;

        if (!cDAssigned)
            return;

        if (_mSubSystem == null)
            return;

        //DONT SEND ANYTHING TO NORMCORE IF THE REAALTIME VIEW IS NOT LOCALLY OWNED
        if (_rtView != null)
            if (!_rtView.isOwnedLocallySelf)
                return;

        //initializing data to send
        string dataToSend = "";
        if (_handTrackingAcquired)
        {
            //setting whether to display the hands or not
            dataToSend += "1|";
            for (int j = 0; j < _joints.Length; j++)
            {
                if (!hand.GetJoint((XRHandJointID)(j + 1)).TryGetPose(out Pose jp))
                    return;
                
                //convertting to the global unity space from the local xrorigin space
                //var xrOriginPose = new Pose(origin.Origin.transform.position, origin.Origin.transform.rotation);
                var cameraOffsetPose = new Pose(cameraOffset.position, cameraOffset.rotation);
                Pose jointPose = jp.GetTransformedBy(cameraOffsetPose);
                
                dataToSend += $"{jointPose.position.x}|{jointPose.position.y}|{jointPose.position.z}|{jointPose.rotation.eulerAngles.x}|{jointPose.rotation.eulerAngles.y}|{jointPose.rotation.eulerAngles.z}|";
                /*dataToSend += _joints[j].localPosition.x + "|"
                    + _joints[j].localPosition.y + "|"
                    + _joints[j].localPosition.z + "|"
                    + _joints[j].localEulerAngles.x + "|"
                    + _joints[j].localEulerAngles.y + "|"
                    + _joints[j].localEulerAngles.z + "|";*/
            }
        }
        else if(_controllerTrackingAcquired)
        {
            dataToSend += "2|";
            dataToSend += $"{handOrigin.position.x}|{handOrigin.position.y}|{handOrigin.position.z}|{handOrigin.localEulerAngles.x}|{handOrigin.localEulerAngles.y}|{handOrigin.localEulerAngles.z}|";

            AssignControllerDevice();

            if (controllerDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue))
                dataToSend += $"{triggerValue}|";
            else
                dataToSend += $"{0}|";
            
            if(controllerDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out float gripValue))
                dataToSend += $"{gripValue}|";
            else
                dataToSend += $"{0}|";
        }
        else
        {
            dataToSend += "0|";
        }
        //Debug.Log("Sending " + dataToSend);
        _handSync.SetHandData(dataToSend);
    }

    public void UpdateFromNormcore(string netHandData)
    {
        if (!_jointsAssigned)
            return;
            
        //DONT RECEIVE ANYTHING FROM NORMCORE IF IT IS LOCALLY OWNED
        if (_rtView != null)
            if (_rtView.isOwnedLocallySelf)
                return;
                
                
        if(netHandData == null || netHandData == "")
            return;

        
        string[] netHandDataArr = netHandData.Split('|');
        Debug.Log(netHandDataArr[0]);
        if (netHandDataArr[0] == "0")
        {
            _mySkinMeshRendererHT.enabled = false;
            _mySkinMeshRendererC.enabled = false;
            return;
        }
        else if(netHandDataArr[0] == "1")
        {
            _mySkinMeshRendererHT.enabled = true;
            _mySkinMeshRendererC.enabled = false;

            for (int j = 0; j < _joints.Length; j++)
            {
                Pose xrOriginPose = new Pose(origin.transform.position, origin.transform.rotation);
                //jointPose.GetTransformedBy(xrOriginPose);

                //Debug.Log("Updating Joints " + netHandData + " ownerID " + _rtView.ownerIDSelf);
                int jTmp = j * 6;
                _joints[j].position =
                    new Vector3(
                    float.Parse(netHandDataArr[jTmp + 1]),
                    float.Parse(netHandDataArr[jTmp + 2]),
                    float.Parse(netHandDataArr[jTmp + 3]));
                _joints[j].eulerAngles =
                    new Vector3(
                        float.Parse(netHandDataArr[jTmp + 4]),
                        float.Parse(netHandDataArr[jTmp + 5]),
                        float.Parse(netHandDataArr[jTmp + 6]));
            }
        }
        else if(netHandDataArr[0] == "2")
        {
            //Debug.Log("Updating Controllers " + netHandData + " ownerID " + _rtView.ownerIDSelf);
            _mySkinMeshRendererHT.enabled = false;
            _mySkinMeshRendererC.enabled = true;

            controller.position = new Vector3(float.Parse(netHandDataArr[1]), 
                float.Parse(netHandDataArr[2]), 
                float.Parse(netHandDataArr[3]));
            controller.eulerAngles = new Vector3(float.Parse(netHandDataArr[4]),
                float.Parse(netHandDataArr[5]),
                float.Parse(netHandDataArr[6]));
            hCAnimator.SetFloat("Trigger", float.Parse(netHandDataArr[7]));
            hCAnimator.SetFloat("Grip", float.Parse(netHandDataArr[8]));
        }
        else
        {
            Debug.Log("Error setting mesh renderer");
        } 

        
    }
}
