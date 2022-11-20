using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AirCurrentOptions : MonoBehaviour
{
    private BoxCollider2D currentCollider;
    public Vector3 colliderDimensions;
    // Start is called before the first frame update
    void Awake()
    {
        currentCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = colliderDimensions;
    }
}
