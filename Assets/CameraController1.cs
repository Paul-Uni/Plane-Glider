using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController1 : MonoBehaviour
{
    public Transform playerTransform; // Assign your player's transform here

    public float offset = 100; // The initial distance between the camera and the player

    void Start()
    {
        // Calculate the initial distance between the camera and the player
        offset = transform.position.x - playerTransform.position.x;
    }

    void LateUpdate()
    {
        // Make the camera follow the player's x coordinate, while keeping its original y and z coordinates
        transform.position = new Vector3(playerTransform.position.x + offset, transform.position.y, transform.position.z);
    }
}
