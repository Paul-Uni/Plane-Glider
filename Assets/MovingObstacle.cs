using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    public float speed = 2f;
    public float maxOffset = 5f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float xPosition = startPosition.x + maxOffset * Mathf.Sin(Time.time * speed);
        float yPosition = startPosition.y + maxOffset * Mathf.Sin(Time.time * speed);
        transform.position = new Vector3(xPosition, yPosition, startPosition.z);
    }
}

