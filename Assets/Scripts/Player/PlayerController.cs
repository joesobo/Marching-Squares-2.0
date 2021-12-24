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
    private float zoom;
    private Camera playerCam;

    public int speed = 15;
    public int sprint = 25;
    public int minZoom = 3;
    public int maxZoom = 20;

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        playerCam = FindObjectOfType<Camera>();
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

        if (zoom > 0) {
            playerCam.orthographicSize = Mathf.Clamp(playerCam.orthographicSize - 2, minZoom, maxZoom);
        } else if (zoom < 0) {
            playerCam.orthographicSize = Mathf.Clamp(playerCam.orthographicSize + 2, minZoom, maxZoom);
        }
        zoom = 0;
    }

    public void MovementPerformed(InputAction.CallbackContext context) {
        movement = playerInput.actions["Move"].ReadValue<Vector2>();
    }

    public void SprintPerformed(InputAction.CallbackContext context) {
        if (context.performed) {
            sprinting = true;
        } else {
            sprinting = false;
        }
    }

    public void ZoomPerformed(InputAction.CallbackContext context) {
        if (context.performed) {
            zoom = playerInput.actions["Zoom"].ReadValue<Vector2>().y;
            Debug.Log(zoom);
        }
    }
}
