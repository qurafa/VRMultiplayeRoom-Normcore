using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class Misc : MonoBehaviour
{
    [SerializeField]
    Transform m_ResetPos;
    [SerializeField]
    InputAction m_ResetAction;

    void OnEnable()
    {
        //m_ResetAction.performed += M_ResetAction_performed;
    }

    private void M_ResetAction_performed(InputAction.CallbackContext obj)
    {
        Debug.Log("Reset Action Perfromed");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
