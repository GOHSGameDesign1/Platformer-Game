using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

public class AirCurrent : MonoBehaviour
{
    [SerializeField] private bool playerInside;

    [field: SerializeField] public Vector3 velocity;
    [Tooltip("How fast the wind pushes the player (and the particles)")] public float magnitude;
    [Tooltip("How many particles there are (divided by 10 and multiplied by width")] public float particleCount;
    [Tooltip("The curve for the z rotation of the particles")] public AnimationCurve aniCurve;

    private ParticleSystem airParticles;
    // Start is called before the first frame update
    void Awake()
    {
        playerInside = false;
        airParticles = transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        velocity = transform.up * magnitude;

        var main = airParticles.main;

        var velLifetime = airParticles.velocityOverLifetime;
        //velLifetime.x = transform.up.x * magnitude;
        velLifetime.y = magnitude * 10/3;
        velLifetime.orbitalZ = new ParticleSystem.MinMaxCurve((0.007f * magnitude), aniCurve);

        main.startLifetime = Mathf.Clamp(transform.lossyScale.y / (magnitude * 10 / 3) * 0.9f, 0.1f, 5f);

        var shape = airParticles.shape;
        shape.position = new Vector3(0, transform.lossyScale.y/-2 + 3, 0);
        //transform.GetChild(0).localPosition = new Vector3(0, transform.localScale.y * -0.41f/33f, 0);

        var emission = airParticles.emission;
        emission.rateOverTime = particleCount/10f * transform.lossyScale.x;
        shape.scale = new Vector3(transform.lossyScale.x, 6, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("entered");

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

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, velocity + transform.position);
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

//Handle for the velocity vector;
#if UNITY_EDITOR
[CustomEditor(typeof(AirCurrent))]
[CanEditMultipleObjects]
public class AirCurrentEditor : Editor
{
    //SerializedObject linkedObject;
    public void OnSceneGUI()
    {
        /*var linkedObject = target as AirCurrent;
        //ParticleSystem.MainModule airMain = linkedObject.airParticles.main;

        Handles.color = Color.yellow;

        Handles.DrawLine(linkedObject.transform.position, linkedObject.velocity + linkedObject.transform.position);
        
        EditorGUI.BeginChangeCheck();

        Vector3 newVelocity = Handles.PositionHandle(linkedObject.velocity + linkedObject.transform.position, Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Update position");
            //linkedObject.velocity = newVelocity - linkedObject.transform.position;
            //linkedObject.airParticles.main.startLifetime = 20 / linkedObject.velocity.y * 0.7f;

        }*/
    }
}
#endif //UNITY_EDITOR
