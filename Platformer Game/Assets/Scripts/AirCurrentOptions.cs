using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AirCurrentOptions : MonoBehaviour
{
    [Header("Components")]
    private BoxCollider2D currentCollider;
    //private ParticleSystem airParticles;

    [Header("Options")]
    public Vector3 colliderDimensions;

    // Start is called before the first frame update
    void Awake()
    {
        currentCollider = GetComponent<BoxCollider2D>();
        //airParticles = transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = colliderDimensions;
    }
}
