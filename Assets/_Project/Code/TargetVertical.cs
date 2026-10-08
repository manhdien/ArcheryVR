using UnityEngine;

public class TargetVertical : MonoBehaviour
{
    public float speed = 2f;
    public float distance = 2f;

    private Vector3 startPosition;
    private int direction = 1;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position +=
            Vector3.up *
            direction *
            speed *
            Time.deltaTime;

        if (transform.position.y >=
            startPosition.y + distance)
        {
            transform.position = new Vector3(
                transform.position.x,
                startPosition.y + distance,
                transform.position.z
            );

            direction = -1;
        }

        if (transform.position.y <=
            startPosition.y - distance)
        {
            transform.position = new Vector3(
                transform.position.x,
                startPosition.y - distance,
                transform.position.z
            );

            direction = 1;
        }
    }
}