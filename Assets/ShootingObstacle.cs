using UnityEngine;

public class ShootingObstacle : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
    }
}

