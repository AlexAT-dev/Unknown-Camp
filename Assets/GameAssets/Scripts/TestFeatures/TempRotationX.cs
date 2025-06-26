using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempRotationX : MonoBehaviour
{
    private Camera mainCamera;
    [Range(0, 10)] public int temp;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(mainCamera.transform.rotation.eulerAngles.x, 0, 0);
    }
}
