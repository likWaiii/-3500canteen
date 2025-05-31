using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainWindowSwing : MonoBehaviour
{
    public Camera mainCamera;
    public float rotateSpeed = 1f;

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            if (transform.rotation.y < -0.10) return;
            transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (transform.rotation.y > 0.10) return;
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
}
