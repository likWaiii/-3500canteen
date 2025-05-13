using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainWindowSwing : MonoBehaviour
{
    public Camera mainCamera;
    public float rotateSpeed = 1f;
    private float lastPressTime = -1;

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
}
