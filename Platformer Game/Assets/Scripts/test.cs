using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    characterGround ground;

    // Start is called before the first frame update
    void Start()
    {
        ground = GetComponent<characterGround>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(ground.GetOnGround());
    }
}
