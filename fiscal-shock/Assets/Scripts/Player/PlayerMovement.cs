using UnityEngine;

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
    public LayerMask wallMask;

    public Vector3 velocity;
    bool isGrounded;

    // Update is called once per frame
    void Update()
    {
        //Creates sphere around object to check if it has collided with a ground layer
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance,groundMask | wallMask);
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
        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpBy * -2f * gravity);
        }

        //Velocity for when user is falling down, using gravity and Time.deltaTime.
        velocity.y += gravity * Time.deltaTime;

        //Lets user fall down based on velocity
        controller.Move(velocity * Time.deltaTime);
    }
}
