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
    [SerializeField][Tooltip("The fastest speed the character can fall")] public float speedLimitY;
    [SerializeField][Tooltip("The fastest horizontal speed")] public float speedLimitX;
    [SerializeField][Tooltip("How fast the player falls when gliding")] public float glideSpeedLimit;
    [SerializeField] public float glideDragRampTime;

    [Header("Calculations")]
    public Vector2 velocity;
    public float jumpSpeed;
    private float defaultGravityScale;
    private float counter;
    public float gravMultiplier;
    private float refVelocity = 1;

    [Header("Current State")]
    [SerializeField] private bool desiredJump;
    [SerializeField] public float inputGliding;
    public bool onGround;
    [SerializeField] private bool currentlyJumping;
    [SerializeField] private bool gliding;
    [field:SerializeField] public HashSet<GameObject> airCurrentsAffecting { get; private set; }

    void Awake()
    {
        // finds components, set variables, and makes a new instance of input
        ground = GetComponent<characterGround>();
        rb = GetComponent<Rigidbody2D>();
        playerActions = new PlayerInputActions();
        airCurrentsAffecting= new HashSet<GameObject>();
        defaultGravityScale = 1f;
        counter = 0;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            desiredJump = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        SetPhysics();

        //Get current ground status from ground script
        onGround = ground.GetOnGround();

        //Get whether player is inputting glide action or not
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

        //If in air, not jumping, and inputting gliding, set gliding to true
        if(!onGround && !currentlyJumping && (inputGliding != 0)) 
        { 
            gliding = true;
        } else
        {
            gliding = false;
        }

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

        if(gliding)
        {
             //rb.velocity = new Vector3(velocity.x, Mathf.Clamp(velocity.y, glideSpeedLimit, 10));

            //rb.velocity.y = Mathf.MoveTowards(velocity.y, glideSpeedLimit, Time.deltaTime * 800);

            gravMultiplier = 0.7f;
            int downCurrents = 0;

            foreach(GameObject current in airCurrentsAffecting)
            {
                if(current.GetComponent<AirCurrent>().velocity.y < 0)
                {
                    rb.velocity = new Vector3(rb.velocity.x, Mathf.Clamp(velocity.y, -speedLimitY -3, speedLimitY + 3));
                    downCurrents++;
                    //glideSpeedLimit = -4;
                }

                rb.velocity += (Vector2)current.GetComponent<AirCurrent>().velocity;
            }

            if(rb.velocity.y < glideSpeedLimit && (downCurrents == 0))
            {
                //rb.velocity = new Vector3(velocity.x, Mathf.SmoothDamp(velocity.y, glideSpeedLimit, ref refVelocity, Time.fixedDeltaTime));
                //rb.velocity += new Vector2(0, Mathf.Lerp(0, Mathf.Abs(rb.velocity.y), 1));
                //rb.AddForce(new Vector2(0, rb.gravityScale + glideDragForce));

                //rb.velocity = new Vector2(velocity.x, Mathf.MoveTowards(rb.velocity.y, 0, (15 * Mathf.Abs(rb.velocity.y)) * Time.deltaTime)); GOOD

                //rb.velocity += new Vector2(0, Mathf.Abs(rb.velocity.y) / Mathf.Lerp(20, 1, counter/1f));

                rb.velocity = new Vector2(rb.velocity.x, Mathf.Lerp(rb.velocity.y, 0, counter / glideDragRampTime));
                counter += Time.fixedDeltaTime;
            }

            rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -speedLimitX, speedLimitX), Mathf.Clamp(rb.velocity.y, -speedLimitY - 5, speedLimitY + 3));

            //return to ignore the clamp down below
            return;
        }
        counter = 0; //reset counter if not gliding

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
                gravMultiplier = upwardMovementMultiplier; //If still jumping, have normal jumping gravity
            }
        }

        //Else if going down...
        else if (rb.velocity.y < -0.01f)
        {
            if(currentlyJumping) { currentlyJumping= false; }

            if (onGround)
            //Don't change it if Kit is stood on something (such as a moving platform)
            {
                gravMultiplier = defaultGravityScale;
            }
            else
            {
                /*// if falling and inputting a glide clamp the velocity to -3
                if (gliding) 
                {
                    Debug.Log("gliding");
                    rb.velocity = new Vector2(velocity.x, Mathf.Clamp(rb.velocity.y, glideSpeedLimit, 100));

                    gravMultiplier = 0.1f;

                    //Return to ignore the other clamp down below
                    return;
                }*/
                

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
        rb.velocity = new Vector3(velocity.x, Mathf.Clamp(velocity.y, -speedLimitY, 100));
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("triggered");

        airCurrentsAffecting.Add(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject != null)
        {
            airCurrentsAffecting.Remove(collision.gameObject);
        }
    }

    private void OnEnable()
    {
        playerActions.Player.Enable();
        playerActions.Player.Jump.started += OnJump;
    }

    private void OnDisable()
    {
        playerActions.Player.Jump.started -= OnJump;
        playerActions.Player.Disable();
    }

    public bool getGliding()
    {
        return gliding;
    }
}
