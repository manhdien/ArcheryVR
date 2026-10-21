using UnityEngine;

[ExecuteAlways]
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

    private void OnValidate()
    {
        Setup();
        UpdateString();
    }

    public void Setup()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        if (line != null)
        {
            line.positionCount = 3;
            line.useWorldSpace = true;
        }
    }

    public void UpdateString()
    {
        if (line == null)
            Setup();

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
