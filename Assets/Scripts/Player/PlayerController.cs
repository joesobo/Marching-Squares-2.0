using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase]
public class PlayerController : MonoBehaviour {
    private PlayerInput playerInput;
    private Rigidbody2D rb;
    private Vector2 movement;
    private bool sprinting = false;

    public int speed = 15;
    public int sprint = 25;

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        if (movement.x != 0 || movement.y != 0) {
            var tempSpeed = speed;
            if (sprinting) {
                tempSpeed = sprint;
            }
            rb.velocity = new Vector2(movement.x * tempSpeed, movement.y * tempSpeed);
        } else {
            rb.velocity = Vector2.zero;
        }
    }

    public void MovementPerformed(InputAction.CallbackContext context) {
        if (playerInput) {
            movement = playerInput.actions["Move"].ReadValue<Vector2>();
        }
    }

    public void SprintPerformed(InputAction.CallbackContext context) {
        if (playerInput) {
            if (context.performed) {
                sprinting = true;
            } else {
                sprinting = false;
            }
        }
    }
}
