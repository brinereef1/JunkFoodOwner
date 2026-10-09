using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private CharacterController controller;
    private float fixedY;

    private void Start()
    {
        // Get the character controller so we can move the player.
        controller = GetComponent<CharacterController>();

        // Store the starting Y position.
        fixedY = transform.position.y;
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical);

        // Keep the movement speed the same even when moving diagonally.
        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }

        // Move the player with the character controller.
        controller.Move(movement * moveSpeed * Time.deltaTime);

        // Freeze Y position.
        Vector3 position = transform.position;
        position.y = fixedY;
        transform.position = position;

        // Turn the player to face the direction of movement.
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}