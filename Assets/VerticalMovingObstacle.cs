using UnityEngine;

public class VerticalMovingObstacle : MonoBehaviour
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
        float yPosition = startPosition.y + maxOffset * Mathf.Sin(Time.time * speed);
        transform.position = new Vector3(startPosition.x, yPosition, startPosition.z);
    }
}

