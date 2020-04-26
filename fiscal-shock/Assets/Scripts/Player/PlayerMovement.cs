using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Here is the tutorial I followed:
/// Player Movement: https://www.youtube.com/watch?v=_QajrabyTJc&t=1s
/// Character Controller documentation: https://docs.unity3d.com/Manual/class-CharacterController.html
/// </summary>

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    /// <summary>
    /// References to control the attributes of movement
    /// </summary>
    public float speed = 12f;
    public float gravity = -50f;
    public float jumpBy = 1.5f;

    
    /// <summary>
    /// Reference to ground body that checks if it has touched a ground layer.
    /// </summary>
    public Transform groundCheck;
    public float groundDistance = 0.4f;

    /// <summary>
    /// Reference to ground layer, obstacle and decoration masks
    /// </summary>
    public LayerMask groundMask;
    public LayerMask obstacleMask;
    public LayerMask decorationMask;

    /// <summary>
    /// References for the keyboard input, check if you're touching the ground and velocity
    /// </summary>
    public Vector3 velocity;
    bool isGrounded;
    public InputActionAsset inputActions;

    /// <summary>
    /// References to movement and jumping by player
    /// </summary>
    private Vector2 movement;
    private bool jumping;

    
    /// <summary>
    /// Assigns the input system configuration to the player's input controller
    /// </summary>
    /// <returns></returns>
    void Awake() {
        gameObject.GetComponent<PlayerInput>().actions = inputActions;
    }


    /// <summary>
    /// Player moves based off Input action
    /// </summary>
    /// <param name="cont"></param>
    /// <returns></returns>
    public void OnMovement(InputAction.CallbackContext cont) {
        movement = cont.ReadValue<Vector2>();
    }

    /// <summary>
    /// Player jumps based off Input action
    /// </summary>
    /// <param name="cont"></param>
    /// <returns></returns>
    public void OnJump(InputAction.CallbackContext cont) {
        jumping = cont.phase == InputActionPhase.Performed;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    /// <returns></returns>
    void Update()
    {

        /// <summary>
        /// Creates sphere around object to check if it has collided with a ground layer
        /// </summary>
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance,groundMask | obstacleMask | decorationMask);

        /// <summary>
        /// Resets velocity, so it doesnt go down forever
        /// </summary>
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        /// <summary>
        /// Gets input from user
        /// </summary>
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        /// <summary>
        /// Creates direction where user wants to move
        /// </summary>
        Vector3 move = transform.right * x + transform.forward * z;

        ///<summary>
        /// Moves the player using move, speed and Time.deltaTime(Frame Rate independent)
        /// </summary>
        controller.Move(move * speed * Time.deltaTime);

        ///<summary>
        /// Only Jump if player is grounded, velocity.y brings player down
        /// </summary>
        if(jumping && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpBy * -2f * gravity);
        }

        /// <summary>
        /// Velocity for when user is falling down, using gravity and Time.deltaTime.
        /// </summary>
        velocity.y += gravity * Time.deltaTime;

        ///<summary>
        /// Lets user fall down based on velocity
        /// </summary>
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Teleports the player to a specific destination
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    public void teleport(Vector3 destination) {
        controller.enabled = false;
        transform.position = destination;
        controller.enabled = true;
        Debug.Log($"Teleported to {destination}");
    }
}
