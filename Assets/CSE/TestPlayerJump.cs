using UnityEngine;
using UnityEngine.InputSystem;
public class TestPlayerJump : MonoBehaviour
{
    public float jumpforce = 5f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private float distToGround;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        distToGround = GetComponent<BoxCollider>().bounds.extents.y;
    }

    private void Update()
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded())
        {
            rb.AddForce(Vector3.up * jumpforce, ForceMode.Impulse);
        }
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 
            distToGround + 0.1f, groundLayer);
    }
}
