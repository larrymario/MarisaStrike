using UnityEngine;
using System;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform target;

    public Vector4 levelEdges;      //XYZW represents Up Down Left Right
    public Vector4 moveEdges;

    Camera camera;

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

	void LateUpdate () {

        Vector3 cameraPos = camera.transform.position;

        if (Math.Abs(cameraPos.x - target.position.x) >= 0.0000001f)
        {
            camera.transform.position = new Vector3(target.position.x, cameraPos.y, cameraPos.z);
        }
        
	}
}
