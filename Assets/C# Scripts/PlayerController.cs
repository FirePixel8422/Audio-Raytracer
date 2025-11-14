using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckLength = 0.5f;

    [SerializeField] Transform camTransform;
    [SerializeField] private float mouseSensitivity = 100f;

    private Rigidbody rb;
    private float xRotation = 0f;
    private float yRotation = 0f;


    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnRegisterUpdate(OnUpdate);

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnUpdate()
    {
        Move();

        LookAround();
    }

    private void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");


        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;
        Vector3 velocity = transform.TransformDirection(moveDir) * moveSpeed + new Vector3(0f, rb.linearVelocity.y, 0f);
        

        if (rb.linearVelocity.y > -0.1f && Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, groundCheckLength))
        {
            velocity = AlignVelocityToRamp(velocity, hit.normal);
        }

        rb.linearVelocity = velocity;
    }

    private float3 AlignVelocityToRamp(float3 velocity, float3 normal)
    {
        float3 n = math.normalize(normal);

        // Remove the component pushing into the ramp
        float3 projected = velocity - n * math.dot(velocity, n);

        float mag = math.length(projected);
        if (mag < 1e-5f)
        {
            // Velocity is basically perpendicular to the ramp
            return float3.zero;
        }

        return projected * (math.length(velocity) / mag);
    }

    private void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        yRotation += mouseX;

        transform.localRotation = Quaternion.Euler(0, yRotation, 0f);
        camTransform.localRotation = Quaternion.Euler(xRotation, 0, 0f);
    }
}
