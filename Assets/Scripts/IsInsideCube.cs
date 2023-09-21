using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsInsideCube : MonoBehaviour
{

    [SerializeField]
    [Tooltip("The Shape Sorting Cube Game Object")]
    GameObject cube;

    [SerializeField]
    [Tooltip("The Shape Sorting Cube Dimentions")]
    Vector3 cubeDimentions = new Vector3(0.10f, 0.10f, 0.10f);

    [SerializeField]
    [Tooltip("The material used when object is not touching targeted objects [:either in shapeNames list or start with \"Collide\"]")]
    Material original;
    [Tooltip("The material used when object is touching with targeted objects [:either in shapeNames list or start with \"Collide\"]")]
    [SerializeField]
    Material inside;
    [SerializeField]
    [Tooltip("Location to put the object once it is successfully put inside the box")]
    Vector3 finalLocation = new Vector3(0, 0, 0);
    [SerializeField]
    [Tooltip("The number of seconds to wait before putting the object in the final location")]
    int waitSeconds = 3;

    float secondsInside = 0;

    bool done = false;

    private void setMaterialRecursive(Material m, GameObject o)
    {
        if (o.GetComponent<Renderer>() != null)
        {
            o.GetComponent<Renderer>().material = m;
        }
        for (int i = 0; i < o.transform.childCount; i++)
        {
            setMaterialRecursive(m, o.transform.GetChild(i).gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        setMaterialRecursive(original, gameObject);
    }

    bool insideSphere()
    {
        return (transform.position-cube.transform.position).magnitude <= cubeDimentions.x/2f;
    }

    bool insideCube()
    {
        Vector3 cubeY = cube.transform.rotation * Vector3.up;
        Vector3 cubeZ = cube.transform.rotation * Vector3.forward;
        Vector3 cubeX = cube.transform.rotation * Vector3.right;

        Plane yPlane = new Plane(cubeY, cube.transform.position);
        Plane zPlane = new Plane(cubeZ, cube.transform.position);
        Plane xPlane = new Plane(cubeX, cube.transform.position);

        return Mathf.Abs(yPlane.GetDistanceToPoint(transform.position)) <= cubeDimentions.y / 2f &&
            Mathf.Abs(zPlane.GetDistanceToPoint(transform.position)) <= cubeDimentions.z / 2f &&
            Mathf.Abs(xPlane.GetDistanceToPoint(transform.position)) <= cubeDimentions.x / 2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (done) return;
        if (insideCube()) { 
            setMaterialRecursive(inside, gameObject);
            secondsInside+=Time.deltaTime;
            Debug.Log(secondsInside);
            if (secondsInside >= waitSeconds)
            {
                transform.position = finalLocation;
                transform.rotation = Quaternion.identity;
                GetComponent<Rigidbody>().isKinematic = true; //so gravity doesn't affect the object anymore
                done = true;
            }
        }
        else {
            secondsInside =0;
            setMaterialRecursive(original, gameObject);
        }
    }



}
