using UnityEngine;
using System;

namespace MarisaStrike {

    public class CustomCamera : MonoBehaviour {

        public Transform target;

        new Camera camera;

        void Awake() {
            camera = GetComponent<Camera>();
        }

        void LateUpdate() {

            Vector3 cameraPos = camera.transform.position;

            if (Math.Abs(cameraPos.x - target.position.x) >= 0.0000001f) {
                camera.transform.position = new Vector3(target.position.x, cameraPos.y, cameraPos.z);
            }

        }
    }

}