using UnityEngine;

public class HandMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 50f;

    private Vector3 movementInput;
    private Vector3 rotationInput;

    void Update()
    {
        // Disable movement when Shift is held (reserved for curling)
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            return;

        // === Movement Input (WASD + R/F) ===
        float moveX = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.S) ? 1 : 0;
        float moveY = Input.GetKey(KeyCode.R) ? 1 : Input.GetKey(KeyCode.F) ? -1 : 0;
        float moveZ = Input.GetKey(KeyCode.Q) ? 1 : Input.GetKey(KeyCode.W) ? -1 : 0;

        movementInput = new Vector3(moveX, moveY, moveZ).normalized * moveSpeed;

        // === Rotation Input (E/D = Pitch, Z/X = Yaw, C/V = Roll) ===
        float rotX = Input.GetKey(KeyCode.E) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0; // Pitch
        float rotY = Input.GetKey(KeyCode.Z) ? -1 : Input.GetKey(KeyCode.X) ? 1 : 0; // Yaw
        float rotZ = Input.GetKey(KeyCode.C) ? -1 : Input.GetKey(KeyCode.V) ? 1 : 0; // Roll

        rotationInput = new Vector3(rotX, rotY, rotZ).normalized * rotationSpeed;

        // === Apply movement and rotation directly ===
        transform.position += movementInput * Time.deltaTime;
        transform.Rotate(rotationInput * Time.deltaTime, Space.Self);
    }
}
