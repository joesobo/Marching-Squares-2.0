using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase]
public class PlayerController : MonoBehaviour {
    private PlayerInput playerInput;
    private InputActions playerInputAction;
    private Rigidbody2D rb;
    private Vector2 velocity = Vector2.zero;

    public int speed = 10;
    public int acceleration = 1;

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        playerInputAction = new InputActions();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable() {
        playerInputAction.Enable();
    }

    private void Start() {
        playerInputAction.Player.Move.performed += MovementPerformed;
    }

    void OnDisable() {
        playerInputAction.Player.Move.performed -= MovementPerformed;
        playerInputAction.Disable();
    }

    private void MovementPerformed(InputAction.CallbackContext context) {
        if (playerInput) {
            Vector3 movement = playerInput.actions["Move"].ReadValue<Vector2>();

            velocity.x = Mathf.MoveTowards(velocity.x, (speed / 100) * movement.x, acceleration * Time.deltaTime);
            velocity.y = Mathf.MoveTowards(velocity.y, (speed / 100) * movement.y, acceleration * Time.deltaTime);
            rb.velocity = velocity;
        }
    }
}
