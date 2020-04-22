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

    public Vector3 velocity;
    bool isGrounded;
    public InputActionAsset inputActions;

    private Vector2 movement;
    private bool jumping;

    void Awake() {
        gameObject.GetComponent<PlayerInput>().actions = inputActions;
    }

    public void OnMovement(InputAction.CallbackContext cont) {
        movement = cont.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext cont) {
        jumping = cont.phase == InputActionPhase.Performed;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tdm = (transform.right * movement.x) + (transform.forward * movement.y);
        controller.Move(tdm * speed * Time.deltaTime);

        //Creates sphere around object to check if it has collided with a ground layer
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance,groundMask | obstacleMask | decorationMask);
        //Resets velocity, so it doesnt go down forever
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

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

    public void teleport(Vector3 destination) {
        controller.enabled = false;
        transform.position = destination;
        controller.enabled = true;
        Debug.Log($"Teleported to {destination}");
    }
}
