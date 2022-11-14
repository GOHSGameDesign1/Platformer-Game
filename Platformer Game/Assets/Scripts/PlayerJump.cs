using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [Header("Components")]
    characterGround ground;
    Rigidbody2D rb;

    PlayerInputActions playerActions;

    [Header("Jumping Stats")]
    [SerializeField, Range(2f, 5.5f)][Tooltip("Maximum jump height")] public float jumpHeight = 7.3f;
    [SerializeField, Range(0.2f, 1.25f)][Tooltip("How long it takes to reach that height before coming back down")] public float timeToJumpApex;
    [SerializeField, Range(0f, 5f)][Tooltip("Gravity multiplier to apply when going up")] public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f)][Tooltip("Gravity multiplier to apply when coming down")] public float downwardMovementMultiplier = 6.17f;
    [SerializeField, Range(0, 1)][Tooltip("How many times can you jump in the air?")] public int maxAirJumps = 0;

    [Header("Options")]
    [SerializeField][Tooltip("The fastest speed the character can fall")] public float speedLimit;
    [SerializeField][Tooltip("How fast the player falls when gliding")] public float glideSpeedLimit;

    [Header("Calculations")]
    public Vector2 velocity;
    public float jumpSpeed;
    private float defaultGravityScale;
    public float gravMultiplier;

    [Header("Current State")]
    private bool desiredJump;
    [SerializeField] public float inputGliding;
    public bool onGround;
    private bool currentlyJumping;

    void Awake()
    {
        // finds components and makes a new instance of input
        ground = GetComponent<characterGround>();
        rb = GetComponent<Rigidbody2D>();
        playerActions = new PlayerInputActions();
        defaultGravityScale = 1f;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log(context);
        if (context.started)
        {
            desiredJump = true;
        }
    }

    public void OnGlide(InputAction.CallbackContext context)
    {

    }

    // Update is called once per frame
    void Update()
    {
        SetPhysics();

        //Get current ground status from ground script
        onGround = ground.GetOnGround();

        inputGliding = playerActions.Player.Glide.ReadValue<float>();
    }

    private void SetPhysics()
    {
        //Determine the character's gravity scale, using the stats provided. Multiply it by a gravMultiplier, used later
        Vector2 newGravity = new Vector2(0, (-2 * jumpHeight) / (timeToJumpApex * timeToJumpApex));
        rb.gravityScale = (newGravity.y / Physics2D.gravity.y) * gravMultiplier;
    }

    private void FixedUpdate()
    {
        //Get velocity from Rigidbody 
        velocity = rb.velocity;

        if (desiredJump)
        {
            DoAJump();
            rb.velocity = velocity;

            //Skip gravity calculations this frame, so currentlyJumping doesn't turn off
            //This makes sure you can't do the coyote time double jump bug
            return;
        }

        CalculateGravity();
    }

    private void CalculateGravity()
    {
        //We change the character's gravity based on her Y direction

        //If player is going up...
        if (rb.velocity.y > 0.01f)
        {
            if (onGround)
            {
                //Don't change it if Kit is stood on something (such as a moving platform)
                gravMultiplier = defaultGravityScale;
            }
            else
            {
                gravMultiplier = upwardMovementMultiplier;
            }
        }

        //Else if going down...
        else if (rb.velocity.y < -0.01f)
        {

            if (onGround)
            //Don't change it if Kit is stood on something (such as a moving platform)
            {
                gravMultiplier = defaultGravityScale;
            }
            else
            {
                // if falling and inputting a glide clamp the velocity to -3
                if (inputGliding != 0) 
                { 
                    rb.velocity = new Vector2(velocity.x, Mathf.Clamp(rb.velocity.y, glideSpeedLimit, 100));

                    gravMultiplier = downwardMovementMultiplier;

                    //Return to ignore the other clamp down below
                    return;
                }
                

                //Otherwise, apply the downward gravity multiplier as Kit comes back to Earth
                gravMultiplier = downwardMovementMultiplier;
            }

        }

        //Else not moving vertically at all
        else
        {
            if (onGround)
            {
                currentlyJumping = false;
            }

            gravMultiplier = defaultGravityScale;
        }
        //Set the character's Rigidbody's velocity
        //But clamp the Y variable within the bounds of the speed limit, for the terminal velocity assist option
        rb.velocity = new Vector3(velocity.x, Mathf.Clamp(velocity.y, -speedLimit, 100));
    }

    private void DoAJump()
    {
        if (onGround)
        {
            desiredJump = false;

            //Determine the power of the jump, based on our gravity and stats
            jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * rb.gravityScale * jumpHeight);

            //If player is moving up or down when she jumps (such as when doing a double jump), change the jumpSpeed;
            //This will ensure the jump is the exact same strength, no matter your velocity.
            if (velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            else if (velocity.y < 0f)
            {
                jumpSpeed += Mathf.Abs(rb.velocity.y);
            }

            //Apply the new jumpSpeed to the velocity. It will be sent to the Rigidbody in FixedUpdate;
            velocity.y += jumpSpeed;
            currentlyJumping = true;
        }

        desiredJump = false;
    }

    private void OnEnable()
    {
        playerActions.Player.Enable();
        playerActions.Player.Jump.started += OnJump;
        playerActions.Player.Glide.performed += OnGlide;
    }

    private void OnDisable()
    {
        playerActions.Player.Jump.started -= OnJump;
        playerActions.Player.Disable();
    }
}
