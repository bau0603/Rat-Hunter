using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingCanMover : MonoBehaviour
{
    public float speed = 3f;
    public float leftLimit = -12f;
    public float rightLimit = 12f;

    private int direction = 1;

    void Update()
    {
        // Move horizontally
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        // Turn around when reaching edges
        if (transform.position.x > rightLimit)
        {
            direction = -1;
        }
        else if (transform.position.x < leftLimit)
        {
            direction = 1;
        }
    }
}
