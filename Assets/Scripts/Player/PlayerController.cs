using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody rb;
    public Transform leftTarget;   // 빨간 공(또는 그 근처 빈 오브젝트)
    public Transform rightTarget;  // 초록 공(또는 그 근처 빈 오브젝트)

    [Header("Move")]
    public float moveSpeed = 4f;
    public float stopDistance = 0.1f;

    [Header("Jump Charge")]
    public float maxChargeTime = 0.8f;
    public float minJumpImpulse = 4f;
    public float maxJumpImpulse = 9f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundRayLength = 0.25f;

    [Header("UI")]
    public GameObject gaugeRoot; // 게이지 묶음(기본 off)
    public Image gaugeFill;      // Filled Image
    public Button leftBtn, jumpBtn, rightBtn; // 있으면 자동 비활성 처리용(선택)

    private bool _leftHeld;
    private bool _rightHeld;
    private bool _charging;
    private float _chargeTime;
    private bool _grounded;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _grounded = CheckGrounded();

        // 점프 차징 게이지 업데이트
        if (_charging)
        {
            _chargeTime += Time.deltaTime;
            float t = Mathf.Clamp01(_chargeTime / maxChargeTime);
            if (gaugeFill) gaugeFill.fillAmount = t;
        }

        // 버튼 비활성(원하면)
        if (leftBtn) leftBtn.interactable = _grounded && !_charging;
        if (rightBtn) rightBtn.interactable = _grounded && !_charging;
        if (jumpBtn) jumpBtn.interactable = _grounded && !_charging; // 점프중(공중)이면 _grounded=false
    }

    void FixedUpdate()
    {
        // 공중(점프중)에는 이동 불가 + 차징중에도 이동 불가
        if (!_grounded || _charging) return;

        Transform target = null;
        if (_leftHeld) target = leftTarget;
        else if (_rightHeld) target = rightTarget;

        if (target == null) return;

        // 타겟 방향을 지면(수평)으로만 투영해서 “앞/뒤(사선 카메라)” 문제 제거
        Vector3 delta = target.position - rb.position;
        Vector3 planar = Vector3.ProjectOnPlane(delta, Vector3.up);
        float dist = planar.magnitude;

        if (dist <= stopDistance) return;

        Vector3 dir = planar / dist;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);

        // 바라보는 방향(선택)
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, look, 12f * Time.fixedDeltaTime));
        }
    }

    bool CheckGrounded()
    {
        Vector3 origin = rb.position + Vector3.up * 0.05f;
        return Physics.Raycast(origin, Vector3.down, groundRayLength, groundMask, QueryTriggerInteraction.Ignore);
    }

    // --- UI에서 호출될 함수들 ---
    public void LeftDown()
    {
        if (!_grounded || _charging) return;
        _leftHeld = true;
        _rightHeld = false;
    }
    public void LeftUp() => _leftHeld = false;

    public void RightDown()
    {
        if (!_grounded || _charging) return;
        _rightHeld = true;
        _leftHeld = false;
    }
    public void RightUp() => _rightHeld = false;

    public void JumpDown()
    {
        if (!_grounded || _charging) return;

        _charging = true;
        _chargeTime = 0f;

        if (gaugeRoot) gaugeRoot.SetActive(true);
        if (gaugeFill) gaugeFill.fillAmount = 0f;

        // 차징 중에는 이동 입력 막고 싶으면 여기서 강제로 해제
        _leftHeld = false;
        _rightHeld = false;
    }

    public void JumpUp()
    {
        if (!_charging) return;

        _charging = false;
        if (gaugeRoot) gaugeRoot.SetActive(false);

        float t = Mathf.Clamp01(_chargeTime / maxChargeTime);
        float impulse = Mathf.Lerp(minJumpImpulse, maxJumpImpulse, t);

        rb.AddForce(Vector3.up * impulse, ForceMode.Impulse);
    }
}