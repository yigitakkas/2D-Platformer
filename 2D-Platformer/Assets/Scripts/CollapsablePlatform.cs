using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsablePlatform : GroundEffector
{
    public float fallSpeed = 10f;
    public float delayTime = 0.5f;

    public Vector3 difference;

    private bool _platformCollapsing = false;
    private Rigidbody2D _rigidbody2D;
    private Vector3 _lastPosition;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _lastPosition = transform.position;
        if(_platformCollapsing)
        {
            _rigidbody2D.AddForce(Vector2.down * fallSpeed);
            if(_rigidbody2D.velocity.y == 0)
            {
                _platformCollapsing = false;
                _rigidbody2D.bodyType = RigidbodyType2D.Static;
            }
        }
    }

    private void LateUpdate()
    {
        difference = transform.position - _lastPosition;
    }

    public void CollapsePlatform()
    {
        StartCoroutine("CollapsePlatformCoroutine");
    }

    public IEnumerator CollapsePlatformCoroutine()
    {
        yield return new WaitForSeconds(delayTime);
        _platformCollapsing = true;
        _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rigidbody2D.freezeRotation = true;
        _rigidbody2D.gravityScale = 1f;
        _rigidbody2D.mass = 1000f;
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
    }
}
