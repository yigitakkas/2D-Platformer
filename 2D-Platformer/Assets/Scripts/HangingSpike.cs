using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingSpike : MonoBehaviour
{
    private Rigidbody2D spikeBody;
    [SerializeField] LayerMask playerLayer;
    private RaycastHit2D playerHit;
    private bool playerDetected;
    private SpriteRenderer spriteRenderer;
    public Sprite newSprite;
    public float raycastHeight = 10f;
    public float fallSpeed = 3f;

    private void Awake()
    {
        spikeBody = GetComponent<Rigidbody2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        if (playerDetected)
            return;
        playerHit = Physics2D.Raycast(transform.position, Vector2.down, raycastHeight, playerLayer);
        Debug.DrawRay(transform.position, Vector2.down * raycastHeight, Color.red);
        if(playerHit)
        {
            spriteRenderer.sprite = newSprite;
            playerDetected = true;
            spikeBody.gravityScale = fallSpeed;
            Invoke("DeactivateObject", 3f);
        }
    }

    void DeactivateObject()
    {
        gameObject.SetActive(false);
    }

}
