using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager Instance = null;

    [SerializeField] GameObject _FallingPlatformPrefab;
    // Start is called before the first frame update
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        } else if (Instance != this)
        {
            Destroy(gameObject);
        }

       // DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartSpawnPlatform(float waitTime, Vector2 position, float rotAngle)
    {
        StartCoroutine(SpawnPlatform(waitTime, position, rotAngle));
    }

    IEnumerator SpawnPlatform(float waitTime, Vector2 position, float rotAngle)
    {
        yield return new WaitForSeconds(waitTime);
        Instantiate(_FallingPlatformPrefab, position, Quaternion.Euler(Vector3.forward * rotAngle));
    }
    

    
}
