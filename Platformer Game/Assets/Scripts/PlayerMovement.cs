using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    characterGround ground;
    Rigidbody2D rb;
    PlayerJump playerJump;
    SpriteRenderer sr;

    PlayerInputActions playerActions;

    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)][Tooltip("Maximum movement speed")] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed")] public float maxAcceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop after letting go")] public float maxDecceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction")] public float maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed when in mid-air")] public float maxAirAcceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when no direction is used")] public float maxAirDeceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction when in mid-air")] public float maxAirTurnSpeed = 80f;

    [Header("Calculations")]
    public float directionX;
    private Vector2 desiredVelocity;
    public Vector2 velocity;
    private float maxSpeedChange;
    private float acceleration;
    private float deceleration;
    private float turnSpeed;

    [Header("Current State")]
    public bool onGround;
    public bool pressingKey;
    private HashSet<GameObject> currentAirCurrents;
    public bool running;


    // Start is called before the first frame update
    void Awake()
    {
        // finds components and makes a new instance of input
        ground = GetComponent<characterGround>();
        rb = GetComponent<Rigidbody2D>();
        playerJump= GetComponent<PlayerJump>();
        playerActions = new PlayerInputActions();
        sr = GetComponent<SpriteRenderer>();
        running = false;
    }

    private void Start()
    {
        currentAirCurrents = playerJump.airCurrentsAffecting;
    }

    public void OnHorizontal(InputAction.CallbackContext context)
    {
        //This is called when you input a direction on a valid input type, such as arrow keys or analogue stick
        //The value will read -1 when pressing left, 0 when idle, and 1 when pressing right.

        //directionX = context.ReadValue<float>();
    }

    // Update is called once per frame
    void Update()
    {
        //Used to flip the character's sprite when she changes direction
        //Also tells us that we are currently pressing a direction button

        directionX = playerActions.Player.Horizontal.ReadValue<float>();

        if (directionX != 0)
        {
            //transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
            //transform.localScale = new Vector3(directionX > 0 ? 0.363895f : -0.363895f, 0.35261f, 1);
            sr.flipX = directionX > 0 ? false : true;
            running = ground.GetOnGround() ? true : false;
            pressingKey = true;
        }
        else
        {
            running = false;
            pressingKey = false;
        }

        //Calculate's the character's desired velocity - which is the direction you are facing, multiplied by the character's maximum speed
        //Friction is not used in this game
        desiredVelocity = new Vector2(directionX, 0f) * Mathf.Max(maxSpeed, 0f);
    }

    private void FixedUpdate()
    {
        //Fixed update runs in sync with Unity's physics engine

        //Get current ground status from ground script
        onGround = ground.GetOnGround();

        //Get the Rigidbody's current velocity
        velocity = rb.velocity;

        Run();
    }

    private void Run()
    {
        //Set acceleration, deceleration, and turnspeed stats based on whether in-air or not
        acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        deceleration = onGround ? maxDecceleration : maxAirDeceleration;
        turnSpeed = onGround ? maxTurnSpeed : maxAirTurnSpeed;

        if (pressingKey)
        {
            //If the sign (i.e. positive or negative) of our input direction doesn't match our movement, it means we're turning around and so should use the turn speed stat.
            if (Mathf.Sign(directionX) != Mathf.Sign(velocity.x))
            {
                maxSpeedChange = turnSpeed * Time.deltaTime;
            }
            else
            {
                //If they match, it means we're simply running along and so should use the acceleration stat
                maxSpeedChange = acceleration * Time.deltaTime;
            }
        }
        else
        {
            //And if we're not pressing a direction at all, use the deceleration stat
            maxSpeedChange = deceleration * Time.deltaTime;
        }

        //Move our velocity towards the desired velocity, at the rate of the number calculated above
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);

        foreach(GameObject current in currentAirCurrents)
        {
            //velocity.x += transform.up.x;
        }

        //Update the Rigidbody with this new velocity
        rb.velocity = velocity;
    }

    private void OnEnable()
    {
        // Enables horizontal input
        playerActions.Player.Enable();
        playerActions.Player.Horizontal.performed += OnHorizontal;
    }

    private void OnDisable()
    {
        // Disables horizontal input
        playerActions.Player.Disable();
        playerActions.Player.Horizontal.performed -= OnHorizontal;
    }
}
