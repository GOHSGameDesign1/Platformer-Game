using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    characterGround ground;
    [SerializeField] float gliding;
    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        ground = GetComponent<characterGround>();
        gliding = GetComponent<PlayerJump>().inputGliding;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(ground.GetOnGround());
        gliding = GetComponent<PlayerJump>().inputGliding;

        spriteRenderer.color = (gliding != 0) ? Color.red : Color.green;
    }
}
