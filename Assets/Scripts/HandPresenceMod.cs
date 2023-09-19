using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Samples.Hands;

[RequireComponent(typeof(HandsAndControllersManager))]    
public class HandPresenceMod : MonoBehaviour
{
    [SerializeField]
    private Animator leftHandAnimator;
    [SerializeField]
    private Animator rightHandAnimator;

    // Start is called before the first frame update
    void Start()
    {
        //XROrigin origin = FindObjectOfType<XROrigin>();
        //hCManager = FindObjectOfType<HandsAndControllersManager>();
        //leftHandOrigin = origin.transform.Find("CameraOffset/Left Controller");
        //rightHandOrigin = origin.transform.Find("CameraOffset/Right Controller");
    }

    // Update is called once per frame
    void Update()
    {
        //MapPosition(leftHand, leftHandOrigin);
        //MapPosition(rightHand, rightHandOrigin);

        UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), leftHandAnimator);
        UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), rightHandAnimator);
    }

    /*void MapPosition(Transform target, Transform originTransform)
    {
        //mapping the position and rotation of the controllers to the displayed head and hands
        //InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        //InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

        target.position = originTransform.position;
        target.rotation = originTransform.rotation;
    }*/

    void UpdateHandAnimation(InputDevice targetDevice, Animator handAnimator)
    {
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }
}
