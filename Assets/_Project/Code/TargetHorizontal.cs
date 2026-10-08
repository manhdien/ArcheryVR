using UnityEngine;

public class TargetHorizontal : MonoBehaviour
{
    public float speed = 3f;
    public float distance = 4f;

    private Vector3 startPosition;
    private int direction = 1;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position +=
            Vector3.right *
            direction *
            speed *
            Time.deltaTime;

        if (transform.position.x >=
            startPosition.x + distance)
        {
            transform.position = new Vector3(
                startPosition.x + distance,
                transform.position.y,
                transform.position.z
            );

            direction = -1;
        }

        if (transform.position.x <=
            startPosition.x - distance)
        {
            transform.position = new Vector3(
                startPosition.x - distance,
                transform.position.y,
                transform.position.z
            );

            direction = 1;
        }
    }
}