using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private Collider2D col;
    public LayerMask spikeLayer;
    // Start is called before the first frame update
    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Spike")
        {
            //spike platform has a normal platform collider and a spike child with a spikelayer collider
            //if touching the spike portion of the spike platform, then die
            if (col.IsTouchingLayers(spikeLayer))
            {
                Destroy(gameObject);
            }
        }
    }
}
