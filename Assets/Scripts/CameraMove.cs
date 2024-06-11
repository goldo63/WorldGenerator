using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float speed = 10f; // Speed of camera movement

    void Update()
    {
        // Get input from the arrow keys or WASD keys
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Calculate the movement vector
        Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0f);

        // Move the camera
        transform.position += movement * speed * Time.deltaTime;
    }
}
