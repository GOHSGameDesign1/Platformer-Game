using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlideReset : MonoBehaviour
{
    private bool isActive;

    private void Awake()
    {
        isActive = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            collision.TryGetComponent<PlayerJump>(out var playerJump);

            if (isActive && (playerJump.glideCounter < playerJump.glideTime))
            {
                //collision.TryGetComponent<PlayerJump>(out var playerJump);

                playerJump.glideCounter = playerJump.glideTime;

                isActive = false;

                StartCoroutine(Cooldown());
            }
        }
    }

    IEnumerator Cooldown() 
    {
        //yield return new WaitForSeconds(5f);
        

        float coolDownCounter = 0;

        while(coolDownCounter < 5f)
        {
            coolDownCounter += Time.deltaTime;

            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, coolDownCounter / 5);
            yield return null;
        }

        isActive = true;
    }
}
