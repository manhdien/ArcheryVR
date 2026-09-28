using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BowString : MonoBehaviour
{
    [Header("String Points")]
    public Transform stringTop;
    public Transform pullPoint;
    public Transform stringBottom;

    private LineRenderer line;

    private void Awake()
    {
        Setup();
    }

    private void OnEnable()
    {
        Setup();
        UpdateString();
    }

    private void LateUpdate()
    {
        UpdateString();
    }

    private void Setup()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        line.positionCount = 3;

        // Chúng ta truyền WORLD POSITION bên dưới
        line.useWorldSpace = true;
    }

    private void UpdateString()
    {
        if (line == null)
            return;

        if (stringTop == null ||
            pullPoint == null ||
            stringBottom == null)
            return;

        line.SetPosition(0, stringTop.position);
        line.SetPosition(1, pullPoint.position);
        line.SetPosition(2, stringBottom.position);
    }
}