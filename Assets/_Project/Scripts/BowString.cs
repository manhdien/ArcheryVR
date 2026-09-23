using UnityEngine;

[ExecuteAlways]
public class BowString : MonoBehaviour
{
    public Transform stringTop;
    public Transform pullPoint;
    public Transform stringBottom;

    private LineRenderer line;

    private void OnEnable()
    {
        Setup();
        UpdateString();
    }

    private void Update()
    {
        UpdateString();
    }

    private void OnValidate()
    {
        Setup();
        UpdateString();
    }

    private void Setup()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        if (line != null)
            line.positionCount = 3;
    }

    private void UpdateString()
    {
        if (line == null)
            return;

        if (stringTop == null || pullPoint == null || stringBottom == null)
            return;

        line.SetPosition(0, stringTop.position);
        line.SetPosition(1, pullPoint.position);
        line.SetPosition(2, stringBottom.position);
    }
}