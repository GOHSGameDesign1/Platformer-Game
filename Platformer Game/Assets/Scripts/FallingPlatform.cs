using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float maxFallSpeed;

    private Rigidbody2D rb;
    private bool falling;
    // Start is called before the first frame update
    void Awake()
    {
        falling = false;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            StartCoroutine(aboutToFall());
        }
    }

    private void FixedUpdate()
    {
        if(falling) 
        {
            if(Mathf.Abs(rb.velocity.y) < maxFallSpeed)
            {
                rb.velocity += Vector2.down / 2;
            }
        }
    }

    IEnumerator aboutToFall()
    {
        yield return new WaitForSeconds(0.2f);
        falling = true;
    }
}
