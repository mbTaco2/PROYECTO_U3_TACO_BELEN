using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.8f;

    [Header("Cámara")]
    public Transform cameraTransform; // Asigna la cámara aquí en el Inspector
    public float mouseSensitivity = 220f;
    private float xRotation = 0f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 inputMovimiento;


    void Start()
    {
        controller = GetComponent<CharacterController>();


        // Bloquea el cursor en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        MovePlayer();
        RotateCamera();
    }

    void MovePlayer()
    {
        // Gravedad
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Movimiento con WASD
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Sprint
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Aplicar gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }

    void RotateCamera()
    {
        // Obtener entrada del mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotación horizontal (Y) del jugador
        transform.Rotate(Vector3.up * mouseX);

        // Rotación vertical (X) de la cámara
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Evita rotaciones extremas
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
