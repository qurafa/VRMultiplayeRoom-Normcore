/************************************************************************************
Copyright : Copyright 2019 (c) Speak Geek (PTY), LTD and its affiliates. All rights reserved.

Developer : Dylan Holshausen

Script Description : Helper for Normcore Realtime Component

************************************************************************************/

using UnityEngine;
using Normal.Realtime;
using System;
using UnityEngine.UIElements;
using Unity.XR.CoreUtils;

public class realtimeHelper : MonoBehaviour
{
    private Realtime _Realtime;
    [SerializeField]
    private string playerPrefabName;
    [SerializeField]
    private string roomName;
    [SerializeField]
    private GameObject xrOrigin;
    [SerializeField]
    private GameObject corner1;

    private GameObject[] spawns;

    private bool settingScale = false;
    XROrigin origin;
    private void Start()
    {
        origin = FindObjectOfType<XROrigin>();
        _Realtime = GetComponent<Realtime>();

        //xrOrigin.transform.position = new Vector3(-1.959f, 1.191576f, 5.678f);
        //xrOrigin.transform.position = new Vector3(0, 1.191576f, 0);

        //Connect to Random Room Code
        //_Realtime.Connect(randomString(8));

        //Connect to Preset Code
        _Realtime.Connect(roomName);

        /*if (_Realtime.clientID == 0)
            SetScale();*/

        _Realtime.didConnectToRoom += _Realtime_didConnectToRoom;
    }

    void Update()
    {
        /*Debug.Log($"Corner Distance: {Vector3.Distance(corner1.transform.position, corner2.transform.position)}");
        Vector3 left = new Vector3(origin.transform.Find("CameraOffset/Left Controller").position.x, 0, 0);
        Vector3 right = new Vector3(origin.transform.Find("CameraOffset/Right Controller").position.x, 0, 0);
        Debug.Log($"Controller Distance: {Vector3.Distance(left, right)}");*/
    }

    //Realtime Event when Connecting to a Room
    private void _Realtime_didConnectToRoom(Realtime realtime)
    {
        //get the spawn points, if there aren't any, then set it to identity i.e. (0,0,0)
        spawns = GameObject.FindGameObjectsWithTag("Spawn");

        Pose spawnPoint = Pose.identity;

        foreach (GameObject spawner in spawns)
            if (spawner.name.Contains(_Realtime.clientID.ToString()))
                spawnPoint = new Pose(spawner.transform.position, spawner.transform.rotation);  
        
        //Instantiate a New Player
        //GameObject newPlayer = Realtime.Instantiate(playerPrefabName, Realtime.InstantiateOptions.defaults);
        GameObject newPlayer = Realtime.Instantiate(playerPrefabName, spawnPoint.position, spawnPoint.rotation, new Realtime.InstantiateOptions
        {
            ownedByClient = true,
            preventOwnershipTakeover = true,
            destroyWhenOwnerLeaves = true,
            destroyWhenLastClientLeaves = true,
            useInstance = _Realtime,
        });
        RequestOwnerShip(newPlayer);
        spawns = null;
        //AllRequestOwnerShip();
    }

    private void RequestOwnerShip(GameObject o)
    {
        if(o.TryGetComponent<RealtimeView>(out RealtimeView rtView))
            rtView.RequestOwnership();

        if (o.TryGetComponent<RealtimeTransform>(out RealtimeTransform rtTransform))
            rtTransform.RequestOwnership();

        for(int c = 0; c < o.transform.childCount; c++)
            RequestOwnerShip(o.transform.GetChild(c).gameObject);

        return;
    }

    private void AllRequestOwnerShip()
    {
        var rViews = FindObjectsByType<RealtimeView>(FindObjectsSortMode.None);
        var rTransforms = FindObjectsByType<RealtimeTransform>(FindObjectsSortMode.None);

        foreach (RealtimeView v in rViews)
        {
            if(v.isUnownedSelf)
                v.RequestOwnership();
        }
        foreach(RealtimeTransform t in rTransforms)
        {
            if (t.isUnownedSelf)
                t.RequestOwnership();
        }
    }

    //Generate Random String
    private string randomString(int length)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];
        var random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        var finalString = new String(stringChars);

        return finalString;
    }
}
