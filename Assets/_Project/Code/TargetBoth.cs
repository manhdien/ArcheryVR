using UnityEngine;

public class TargetBoth : MonoBehaviour
{
    public float speed = 2f;

    public float horizontalDistance = 3f;
    public float verticalDistance = 2f;

    private Vector3 startPosition;

    private int horizontalDirection = 1;
    private int verticalDirection = 1;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        MoveHorizontal();
        MoveVertical();
    }

    void MoveHorizontal()
    {
        transform.position +=
            Vector3.right *
            horizontalDirection *
            speed *
            Time.deltaTime;

        if (transform.position.x >=
            startPosition.x + horizontalDistance)
        {
            transform.position = new Vector3(
                startPosition.x + horizontalDistance,
                transform.position.y,
                transform.position.z
            );

            horizontalDirection = -1;
        }

        if (transform.position.x <=
            startPosition.x - horizontalDistance)
        {
            transform.position = new Vector3(
                startPosition.x - horizontalDistance,
                transform.position.y,
                transform.position.z
            );

            horizontalDirection = 1;
        }
    }

    void MoveVertical()
    {
        transform.position +=
            Vector3.up *
            verticalDirection *
            speed *
            Time.deltaTime;

        if (transform.position.y >=
            startPosition.y + verticalDistance)
        {
            transform.position = new Vector3(
                transform.position.x,
                startPosition.y + verticalDistance,
                transform.position.z
            );

            verticalDirection = -1;
        }

        if (transform.position.y <=
            startPosition.y - verticalDistance)
        {
            transform.position = new Vector3(
                transform.position.x,
                startPosition.y - verticalDistance,
                transform.position.z
            );

            verticalDirection = 1;
        }
    }
}