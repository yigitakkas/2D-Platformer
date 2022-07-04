using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingSpike : MonoBehaviour
{
    private Rigidbody2D spikeBody;
    [SerializeField] LayerMask playerLayer;
    private RaycastHit2D playerHit;
    private bool playerDetected;

    private void Awake()
    {
        spikeBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        if (playerDetected)
            return;
        playerHit = Physics2D.Raycast(transform.position, Vector2.down, 20f, playerLayer);
        Debug.DrawRay(transform.position, Vector2.down * 20f, Color.red);
        if(playerHit)
        {
            playerDetected = true;
            Invoke("DeactivateObject", 3f);
            spikeBody.gravityScale = 1f;
        }
    }

    void DeactivateObject()
    {
        gameObject.SetActive(false);
    }

}
