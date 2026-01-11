using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class JumpRope : MonoBehaviour
{
    //줄을 돌리는 점이 될 A와 B의 위치
    public Transform handleA;
    public Transform handleB;

    public float ropeRadius = 8.0f;
    public float rotationSpeed = 250f;
    [SerializeField] int speedUpTerm = 20; //속도를 몇 점마다 올려줄 것인지
    [SerializeField] float speedUpValue = 25f; //속도를 열마나 올려줄 것인지
    public bool isRotating = true;

    [Range(10, 100)]
    public int visualSegmentCount = 20;

    public int collisionSegmentCount = 40;

    [Range(0f, 0.5f)]
    public float ignoreHandleRatio = 0.2f; 

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
        lineRenderer.useWorldSpace = true; //줄이 월드 좌표계로 그려짐

        visualPositions = new Vector3[visualSegmentCount + 1]; //줄의 점들을 저장할 배열
        lineRenderer.positionCount = visualSegmentCount + 1;//LineRenderer가 그릴 점의 개수를 설정(클수록 부드러움)
    }

    private void Update()
    {
        if (handleA == null || handleB == null ) return;
        if (IsGameOver) return;

        UpdateRotation(); //줄의 회전 각도 업데이트

        Vector3 controlPoint = GetControlPoint(); //현재 각도에 맞는 줄의 가장 불룩한 부분의 위치 계산

        UpdateVisuals(controlPoint); //계산된 위치를 기반으로 줄을 그림
        CheckCollisionOptimized(controlPoint); //플레이어와 줄이 충돌했는지 검사
    }

    void UpdateRotation()
    {
        if (isRotating && !IsGameOver)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            if (!hasScoredThisRound && currentAngle >= 180f) //180f => 줄이 바닥에 닿는 시점
            {
                ScoreManager.Instance.SetScoreText();
                hasScoredThisRound = true;

                if (ScoreManager.Instance.currentScore % speedUpTerm == 0)
                {
                    //속도 최대치는 500, 최소치는 250
                    rotationSpeed = Mathf.Clamp(rotationSpeed + speedUpValue, 250, 550); 
                }
            }

            if (currentAngle >= 360f) //줄이 한 바퀴 돈 시점
            {
                currentAngle -= 360f;
                hasScoredThisRound = false;  //false로 변경하여 점수 누적 가능하게
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
        bool isPerpect = false;

        //180도가 딱 바닥에 줄이 내려왔을 때를 기준으로 범위 지정
        if (currentAngle >= 125f && currentAngle < 175f) //Perfect
        {
            isPerpect = true;
            ScoreManager.Instance.SetPerpectComboText(isPerpect);
            return ("Perfect!!", Color.yellow);
        }    
        else if (currentAngle >= 90f && currentAngle < 135f) //Great
        {
            isPerpect = false;
            ScoreManager.Instance.SetPerpectComboText(isPerpect);
            return ("Great!", Color.green);
        }
        else
        {
            isPerpect = false;
            ScoreManager.Instance.SetPerpectComboText(isPerpect);
            return ("Too Slow...", Color.cyan);
        }
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