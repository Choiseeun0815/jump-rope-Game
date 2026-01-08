using UnityEngine;

public class JumpRope : MonoBehaviour
{
    public Transform handleA;
    public Transform handleB;

    private int segmentCount = 20;
    public float ropeRadius = 2.0f;
    public float rotationSpeed = 300f;

    public bool isRotating = true;

    private LineRenderer lineRenderer;
    private float currentAngle = 0f;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount + 1;
        lineRenderer.useWorldSpace = true;
    }
    private void Update()
    {
        if(handleA == null || handleB == null) return;

        if(isRotating)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            currentAngle %= 360;
        }

        Vector3 controlPoint = CalculateControlPoint();

        DrawRope(controlPoint);
    }

    Vector3 CalculateControlPoint()
    {
        Vector3 center = (handleA.position + handleB.position) / 2f;
        Vector3 axis = (handleB.position - handleA.position).normalized;

        Vector3 perpendicular = Vector3.Cross(axis, Vector3.up).normalized;
        if (perpendicular == Vector3.zero) perpendicular = Vector3.right;
        Vector3 initialDir = Vector3.Cross(perpendicular, axis).normalized;

        Quaternion rotation = Quaternion.AngleAxis(currentAngle, axis);
        Vector3 dir = rotation * initialDir;

        return center + (dir * ropeRadius);
    }

    void DrawRope(Vector3 controlPoint)
    {
        for(int i=0; i<=segmentCount;i++)
        {
            float t = i / (float)segmentCount;

            Vector3 pixelPosition = CalculateBezierPoint(t, handleA.position, controlPoint, handleB.position);
            lineRenderer.SetPosition(i, pixelPosition);
        }
    }

    // 베지어 곡선 공식
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }
}
