using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirCurrent : MonoBehaviour
{
    public Rigidbody2D playerRB;
    public PlayerJump playerJump;
    public float upwardForce;
    [SerializeField] private bool playerInside;

    [field: SerializeField] public Vector2 velocity { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        playerInside = false;

        Debug.Log(transform.up);


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("entered");

        if(collision.tag == "Player")
        {
            playerInside= true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            playerInside = false;
        }
    }

    private void FixedUpdate()
    {
        if (playerInside && playerJump.getGliding())
        {
            //playerRB.velocity += (Vector2)transform.up;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, velocity + (Vector2)transform.position);
    }

    /*private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("triggerd!");
        if(TryGetComponent(out Rigidbody2D rb))
        {
            rb.velocity = Vector3.zero;
        }
    }*/
}
