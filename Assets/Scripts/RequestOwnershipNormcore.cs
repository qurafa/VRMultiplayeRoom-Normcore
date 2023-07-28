using Normal.Realtime;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

[RequireComponent(typeof(XRGrabInteractable), typeof(RealtimeTransform), typeof(RealtimeView))]
public class RequestOwnershipNormcore : MonoBehaviour
{
    private RealtimeView m_RealtimeView;
    private RealtimeTransform m_RealtimeTransform;
    private XRGrabInteractable m_GrabInteractable;

    //add a listener to the selectEntered so it requests ownership when the object is grabbed
    private void OnEnable()
    {
        m_RealtimeView = GetComponent<RealtimeView>();
        m_RealtimeTransform = GetComponent<RealtimeTransform>();
        m_GrabInteractable = GetComponent<XRGrabInteractable>();

        m_GrabInteractable.selectEntered.AddListener(RequestOwnership);
    }

    private void RequestOwnership(SelectEnterEventArgs args)
    {
        m_RealtimeView.RequestOwnership();
        m_RealtimeTransform.RequestOwnership();
    }

    private void OnDisable() => m_GrabInteractable.selectEntered.RemoveListener(RequestOwnership);
}
