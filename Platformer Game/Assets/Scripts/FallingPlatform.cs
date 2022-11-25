using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float maxFallSpeed;
    [field: SerializeField] public float fallDelay { get; private set; }
    [field: SerializeField] public float deathDelay { get; private set; }

    [Header("Components")]
    private Rigidbody2D rb;
    private PlatformEffector2D pe;
    private BoxCollider2D collide;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pe = GetComponent<PlatformEffector2D>();
        collide = GetComponent<BoxCollider2D>();
    } 

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            StartCoroutine(aboutToFall());
        }
    }

    IEnumerator aboutToFall()
    {
        yield return new WaitForSeconds(fallDelay);
        pe.useOneWay = true;

        //Change the layer so player doesn't get onGround from the falling platform;
        gameObject.layer = 7; // 7 is the falling LayerMask

        // disable the collider for one frame so the oneway can trigger for the player if its already touching the platform
        collide.enabled = false;
        yield return null;
        collide.enabled = true;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 4.0f;

        Destroy(gameObject, deathDelay);
    }
}
