using UnityEngine;
using UnityEngine.InputSystem;

/**
Here is the tutorial I followed:
Player Movement: https://www.youtube.com/watch?v=_QajrabyTJc&t=1s

Character Controller documentation: https://docs.unity3d.com/Manual/class-CharacterController.html
*/
public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    //References to control the attributes of movement
    public float speed = 12f;
    public float gravity = -50f;
    public float jumpBy = 1.5f;

    //Reference to ground body that checks if it has touched a ground layer.
    public Transform groundCheck;
    public float groundDistance = 0.4f;

    //Reference to ground layer
    public LayerMask groundMask;
    public LayerMask obstacleMask;
    public LayerMask decorationMask;

    //References for the keyboard input, check if you're touching the ground and velocity
    public Vector3 velocity;
    bool isGrounded;
    public InputActionAsset inputActions;

    //References to movement and jumping by player
    private Vector2 movement;
    private bool jumping;

    //Player gets the player input and performs the action. 
    void Awake() {
        gameObject.GetComponent<PlayerInput>().actions = inputActions;
    }

    //Player moves based off Input action
    public void OnMovement(InputAction.CallbackContext cont) {
        movement = cont.ReadValue<Vector2>();
    }

    //Player jumps based off Input action
    public void OnJump(InputAction.CallbackContext cont) {
        jumping = cont.phase == InputActionPhase.Performed;
    }

    // Update is called once per frame
    void Update()
    {

        //Creates sphere around object to check if it has collided with a ground layer
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance,groundMask | obstacleMask | decorationMask);
        //Resets velocity, so it doesnt go down forever
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //Gets input from user
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //Creates direction where user wants to move
        Vector3 move = transform.right * x + transform.forward * z;

        //Moves the player using move, speed and Time.deltaTime(Frame Rate independent)
        controller.Move(move * speed * Time.deltaTime);

        //Only Jump if player is grounded, velocity.y brings player down
        //if(Input.GetButtonDown("Jump") && isGrounded)
        if(jumping && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpBy * -2f * gravity);
        }

        //Velocity for when user is falling down, using gravity and Time.deltaTime.
        velocity.y += gravity * Time.deltaTime;

        //Lets user fall down based on velocity
        controller.Move(velocity * Time.deltaTime);
    }

    //Teleports the player to a specific destination
    public void teleport(Vector3 destination) {
        controller.enabled = false;
        transform.position = destination;
        controller.enabled = true;
        Debug.Log($"Teleported to {destination}");
    }
}
