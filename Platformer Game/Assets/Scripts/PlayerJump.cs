using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerJump : MonoBehaviour
{
    [Header("Components")]
    characterGround ground;
    Rigidbody2D rb;
    Image glideBar;
    SpriteRenderer sr;

    PlayerInputActions playerActions;

    [field: SerializeField] public Sprite walkSprite { get; private set; }
    [field: SerializeField] public Sprite glideSprite { get; private set; }

    [Header("Jumping Stats")]
    [SerializeField, Range(2f, 5.5f)][Tooltip("Maximum jump height")] public float jumpHeight = 7.3f;
    [SerializeField, Range(0.2f, 1.25f)][Tooltip("How long it takes to reach that height before coming back down")] public float timeToJumpApex;
    [SerializeField, Range(0f, 5f)][Tooltip("Gravity multiplier to apply when going up")] public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f)][Tooltip("Gravity multiplier to apply when coming down")] public float downwardMovementMultiplier = 6.17f;
    [SerializeField, Range(0, 1)][Tooltip("How many times can you jump in the air?")] public int maxAirJumps = 0;

    [Header("Options")]
    [SerializeField, Range(0f, 0.3f)][Tooltip("How long should coyote time last?")] public float coyoteTime = 0.15f;
    [SerializeField, Range(0f, 0.3f)][Tooltip("How far from ground should we cache your jump?")] public float jumpBuffer = 0.15f;
    [SerializeField][Tooltip("How fast the character can fall while not gliding")] public float fallSpeedLimit;
    [SerializeField][Tooltip("The fastest speed the character can fall")] public float glideSpeedLimitY;
    [SerializeField][Tooltip("The fastest horizontal speed")] public float glideSpeedLimitX;
    [SerializeField][Tooltip("How fast the player falls when gliding")] public float glideFallSpeedLimit;
    [SerializeField][Tooltip("How long it takes for the character to reduce to gliding fall speed")] public float glideDragRampTime;
    [SerializeField][Tooltip("How long character can glide for (currently in frames)")] public float glideTime;

    [Header("Calculations")]
    public Vector2 velocity;
    public float jumpSpeed;
    private float defaultGravityScale;
    private float counter;
    [HideInInspector] public float glideCounter;
    public float gravMultiplier;
    private float refVelocity = 1;

    [Header("Current State")]
    [SerializeField] private bool desiredJump;
    [SerializeField] public float inputGliding;
    public bool onGround;
    [SerializeField] private bool currentlyJumping;
    [SerializeField] private bool ascendingFromJump;
    [SerializeField] private bool gliding;
    private float jumpBufferCounter;
    private float coyoteTimeCounter = 0;
    [field:SerializeField] public HashSet<GameObject> airCurrentsAffecting { get; private set; }

    void Awake()
    {
        // finds components, set variables, and makes a new instance of input
        ground = GetComponent<characterGround>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        glideBar = transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();
        playerActions = new PlayerInputActions();
        airCurrentsAffecting= new HashSet<GameObject>();
        defaultGravityScale = 1f;
        counter = 0;
        gliding = false;
        ascendingFromJump = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && !gliding)
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

        //Jump buffer allows us to queue up a jump, which will play when we next hit the ground
        if (jumpBuffer > 0)
        {
            //Instead of immediately turning off "desireJump", start counting up...
            //All the while, the DoAJump function will repeatedly be fired off
            if (desiredJump)
            {
                jumpBufferCounter += Time.deltaTime;

                if (jumpBufferCounter > jumpBuffer)
                {
                    //If time exceeds the jump buffer, turn off "desireJump"
                    desiredJump = false;
                    jumpBufferCounter = 0;
                }
            }
        }

        //If we're not on the ground and we're not currently jumping, that means we've stepped off the edge of a platform.
        //So, start the coyote time counter...
        if (!currentlyJumping && !onGround)
        {
            coyoteTimeCounter += Time.deltaTime;
        }
        else
        {
            //Reset it when we touch the ground, or jump
            coyoteTimeCounter = 0;
        }

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

        ManageGliding();

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

    void ManageGliding()
    {
        //Reduce glideCounter if gliding, reset if touched ground
        if (onGround)
        {
            glideCounter = glideTime;

            //hide glide bar
            glideBar.CrossFadeAlpha(0, 0.3f, false);
        }
        else if (gliding)
        {
            //show glide bar
            glideBar.CrossFadeAlpha(1, 0.2f, false);

            glideCounter--;
        }

        //Manage glide bar;
        glideBar.fillAmount = Mathf.MoveTowards(glideBar.fillAmount, glideCounter / glideTime, 10 * Time.deltaTime);
        glideBar.color = Color.Lerp(Color.red, Color.white, glideCounter / glideTime);

        //If in air, not jumping, and inputting gliding, set gliding to true
        if (!onGround && !ascendingFromJump && (inputGliding != 0) && (glideCounter > 0))
        {
            gliding = true;
            sr.sprite = glideSprite;
        }
        else
        {
            gliding = false;
            sr.sprite = walkSprite;
        }
    }

    private void CalculateGravity()
    {
        //We change the character's gravity based on her Y direction

        if(gliding)
        {
            int downCurrents = 0;

            foreach(GameObject current in airCurrentsAffecting)
            {
                if(current.GetComponent<AirCurrent>().velocity.y < -0.01f)
                {
                    rb.velocity = new Vector3(rb.velocity.x, Mathf.Clamp(velocity.y, -glideSpeedLimitY , glideSpeedLimitY));
                    downCurrents++;
                    //glideSpeedLimit = -4;
                }

                rb.velocity += (Vector2)current.GetComponent<AirCurrent>().velocity;
            }

            if(rb.velocity.y < glideFallSpeedLimit && (downCurrents == 0))
            {
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Lerp(rb.velocity.y, 0, counter / glideDragRampTime));
                counter += Time.fixedDeltaTime;
            }

            //glide-specific clamp
            rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -glideSpeedLimitX, glideSpeedLimitX), Mathf.Clamp(rb.velocity.y, -glideSpeedLimitY, glideSpeedLimitY));

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
            //No longer ascending if falling 
            if(ascendingFromJump) { ascendingFromJump= false; }

            if (onGround)
            //Don't change it if Kit is stood on something (such as a moving platform)
            {
                gravMultiplier = defaultGravityScale;
            }
            else
            {
                
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
        rb.velocity = new Vector3(velocity.x, Mathf.Clamp(velocity.y, -fallSpeedLimit, 100));
    }

    private void DoAJump()
    {
        //Create the jump, provided we are on the ground or in coyote time.
        if (onGround || (coyoteTimeCounter > 0.03f && coyoteTimeCounter < coyoteTime))
        {
            desiredJump = false;
            ascendingFromJump = true;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;

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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Current")
        {
            airCurrentsAffecting.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Current")
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
