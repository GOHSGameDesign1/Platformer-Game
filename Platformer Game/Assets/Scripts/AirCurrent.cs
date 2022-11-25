using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

public class AirCurrent : MonoBehaviour
{ 
    [field: SerializeField] public Vector3 velocity { get; private set; }
    [field: SerializeField][Tooltip("How fast the wind pushes the player (and the particles)")] public float magnitude { get; private set; }
    [field: SerializeField][Tooltip("How many particles there are (divided by 10 and multiplied by width")] public float particleCount { get; private set; }
    [field: SerializeField][Tooltip("The curve for the z rotation of the particles")] public AnimationCurve aniCurve { get; private set; }

    private ParticleSystem airParticles;
    // Start is called before the first frame update
    void Awake()
    {
        airParticles = transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        velocity = transform.up * magnitude;

        EditParticles();
    }

    void EditParticles()
    {
        var main = airParticles.main;
        var velLifetime = airParticles.velocityOverLifetime;
        var shape = airParticles.shape;
        var emission = airParticles.emission;

        float particleVelocity = magnitude * 10;

        main.startLifetime = Mathf.Clamp(transform.lossyScale.y / particleVelocity * 0.9f, 0.1f, 5f);

        velLifetime.y = particleVelocity;
        velLifetime.orbitalZ = new ParticleSystem.MinMaxCurve((0.007f * magnitude), aniCurve);

        shape.position = new Vector3(0, transform.lossyScale.y / -2 + 3, 0);
        shape.scale = new Vector3(transform.lossyScale.x, 6, 0);

        emission.rateOverTime = particleCount / 10f * transform.lossyScale.x;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, velocity + transform.position);
    }
}