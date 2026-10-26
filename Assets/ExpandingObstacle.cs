using UnityEngine;

public class ExpandingObstacle : MonoBehaviour
{
    public float speed = 2f;
    public float maxScale = 2f;

    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        float scale = startScale.x + maxScale * Mathf.Abs(Mathf.Sin(Time.time * speed));
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
