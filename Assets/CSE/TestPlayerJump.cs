using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class TestPlayerJump : MonoBehaviour
{
    public float jumpforce = 15f;
    public LayerMask groundLayer;
    public float gravityScale = 2.5f; //중력 증가

    private Rigidbody rb;
    private float distToGround;

    [SerializeField] JumpRope jumpRope;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        distToGround = GetComponent<BoxCollider>().bounds.extents.y;
    }

    private void Update()
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpforce, ForceMode.Impulse);

            StartCoroutine(CheckJumpTimingRoutine());
        }
    }

    private void FixedUpdate()
    {
        Vector3 extraGravity = Physics.gravity * gravityScale;
        rb.AddForce(extraGravity, ForceMode.Acceleration);
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 
            distToGround + 0.1f, groundLayer);
    }

    IEnumerator CheckJumpTimingRoutine()
    {
        var res = jumpRope.GetJumpTimingCheck(); //점프 판정 결과

        yield return new WaitForSeconds(.2f); //점프 후 줄과 충돌할 수도 있으니 잠시 대기

        if (!jumpRope.IsGameOver)
            ScoreManager.Instance.ShowJumpTimingText(res.text, res.color);
    }
}
