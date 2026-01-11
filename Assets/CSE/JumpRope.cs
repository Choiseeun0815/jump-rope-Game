using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class JumpRope : MonoBehaviour
{
    //줄을 돌리는 점이 될 A와 B의 위치
    public Transform handleA;
    public Transform handleB;

    [Header("기본 설정")]
    public float ropeRadius = 8.0f;
    public float rotationSpeed = 300f;
    public bool isRotating = true;

    [Header("해상도 설정")]
    [Range(10, 100)]
    public int visualSegmentCount = 20;

    public int collisionSegmentCount = 40;

    [Header("최적화 설정")]
    [Range(0f, 0.5f)]
    public float ignoreHandleRatio = 0.2f; 

    [Header("바닥 및 충돌")]
    public bool useFloorCollision = true;
    public float floorY = 0f;
    public float hitThickness = 0.1f;
    public LayerMask playerLayer;

    private LineRenderer lineRenderer;
    private float currentAngle = 0f;
    private bool hasScoredThisRound = false;
    public bool IsGameOver { get; private set; } = false;

    private Vector3[] visualPositions;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;

        visualPositions = new Vector3[visualSegmentCount + 1];
        lineRenderer.positionCount = visualSegmentCount + 1;
    }

    private void Update()
    {
        if (handleA == null || handleB == null ) return;
        if (IsGameOver) return;

        UpdateRotation();

        Vector3 controlPoint = GetControlPoint();

        UpdateVisuals(controlPoint);
        CheckCollisionOptimized(controlPoint);
    }

    void UpdateRotation()
    {
        if (isRotating && !IsGameOver)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            if (!hasScoredThisRound && currentAngle >= 180f) // 180f => 줄이 최초로 바닥에 닿는 시점
            {
                ScoreManager.Instance.SetScoreText();
                hasScoredThisRound = true;
            }

            if (currentAngle >= 360f) //줄이 한 바퀴 돈 시점
            {
                currentAngle -= 360f;
                hasScoredThisRound = false;  // false로 변경하여 점수 누적 가능하게
            }
        }
    }

    Vector3 GetControlPoint()
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

    void UpdateVisuals(Vector3 controlPoint)
    {
        if (visualPositions.Length != visualSegmentCount + 1)
        {
            visualPositions = new Vector3[visualSegmentCount + 1];
            lineRenderer.positionCount = visualSegmentCount + 1;
        }

        for (int i = 0; i <= visualSegmentCount; i++)
        {
            float t = i / (float)visualSegmentCount;
            Vector3 pos = GetBezierPoint(t, handleA.position, controlPoint, handleB.position);

            if (useFloorCollision) pos.y = Mathf.Max(pos.y, floorY);

            visualPositions[i] = pos;
        }
        lineRenderer.SetPositions(visualPositions);
    }
    public (string text, Color color) GetJumpTimingCheck()
    {
        //180도가 딱 바닥에 줄이 내려왔을 때.

        if (currentAngle >= 135f && currentAngle < 175f)
            return ("Perfect!", Color.yellow);
        else if (currentAngle >= 95f && currentAngle < 135f)
            return ("Great", Color.green);
        else return ("Too Slow", Color.cyan);
    }
    void CheckCollisionOptimized(Vector3 controlPoint)
    {
        float startT = ignoreHandleRatio;
        float endT = 1.0f - ignoreHandleRatio;

        Vector3 prevPos = GetBezierPoint(startT, handleA.position, controlPoint, handleB.position);
        if (useFloorCollision) prevPos.y = Mathf.Max(prevPos.y, floorY);

        int startStep = Mathf.FloorToInt(collisionSegmentCount * startT);
        int endStep = Mathf.CeilToInt(collisionSegmentCount * endT);

        for (int i = startStep + 1; i <= endStep; i++)
        {
            float t = i / (float)collisionSegmentCount;
            Vector3 currentPos = GetBezierPoint(t, handleA.position, controlPoint, handleB.position);

            if (useFloorCollision) currentPos.y = Mathf.Max(currentPos.y, floorY);

            if (Physics.CheckCapsule(prevPos, currentPos, hitThickness, playerLayer))
            {
                Debug.Log("Game Over");
                IsGameOver = true;
                ScoreManager.Instance.SetGameOverText();
                return;
            }

            prevPos = currentPos;
        }
    }

    Vector3 GetBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return (uu * p0) + (2 * u * t * p1) + (tt * p2);
    }
}