using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrateModels : MonoBehaviour
{
    private Realtime _realTime;

    // Start is called before the first frame update
    void Start()
    {
        _realTime = GetComponent<Realtime>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_realTime.clientID == 0)
        {

        }
    }

    private void SetStartPoint()
    {

    }

    private void SetStopPoint() 
    { 
    
    }
}
