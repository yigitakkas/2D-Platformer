using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class CharacterController2D : MonoBehaviour

{
    public float raycastDist = .2f;
    public LayerMask layerMask; //this is used for recast to only collide with LevelGeom layer

    public bool below; //if true, something is below the character
    public GroundType groundType;

    private Vector2 _moveAmount;
    private Vector2 _currPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody2D;
    private CapsuleCollider2D _capsuleCollider2D;

    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3]; //gives us info about the object we hit with ray

    private bool _disableCheckGround;

    void Start()
    {
        _rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider2D = gameObject.GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        
    }

    void FixedUpdate() //to put physics simulations in fixed update is a better practice than putting them in update method
    {
        _lastPosition = _rigidbody2D.position;
        _currPosition = _lastPosition + _moveAmount;
        _rigidbody2D.MovePosition(_currPosition);
        _moveAmount = Vector2.zero; //move amount did its job so reset the value to zero
        if(!_disableCheckGround) //if character is jumping, don't check the ground
        {
            CheckGround();
        }
    }

    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    private void CheckGround()
    {
        Vector2 raycastOrigin = _rigidbody2D.position - new Vector2(0, _capsuleCollider2D.size.y * .5f * 10); //positions on the bottom of collider
        _raycastPosition[0] = raycastOrigin + (Vector2.left * _capsuleCollider2D.size.x * .25f * 10 + Vector2.up * .1f); //gives offset to the left and up a little
        _raycastPosition[1] = raycastOrigin;
        _raycastPosition[2] = raycastOrigin + (Vector2.right * _capsuleCollider2D.size.x * .25f * 10 + Vector2.up * .1f); //gives offset to the right and up a little

        //DrawRays2Debug(Vector2.down, Color.green);

        int countGroundHits = 0;
        for(int i=0;i<_raycastPosition.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(_raycastPosition[i], Vector2.down, raycastDist, layerMask);

            if(hit.collider)
            {
                _raycastHits[i] = hit;
                countGroundHits++;
            }
        }
        if(countGroundHits>0)
        {
            if(_raycastHits[1].collider)
            {
                groundType = DetermineGroundType(_raycastHits[1].collider);
            }
            else
            {
                for(int i=0;i< _raycastHits.Length; i+=2) //if the middle raycast can't detect, check the left and right raycast
                {
                    if(_raycastHits[i].collider)
                    {
                        groundType = DetermineGroundType(_raycastHits[i].collider);
                    }
                }
            }
            below = true;
        }
        else
        {
            groundType = GroundType.None;
            below = false;
        }
    }

    public void DisableCheckGround()
    {
        below = false;
        _disableCheckGround = true;
        StartCoroutine("EnableCheckGround");
    }

    IEnumerator EnableCheckGround()
    {
        yield return new WaitForSeconds(.1f);
        _disableCheckGround = false;
    }

    private GroundType DetermineGroundType(Collider2D collider)
    {
        if(collider.GetComponent<GroundEffector>())
        {
            GroundEffector groundEffector = collider.GetComponent<GroundEffector>();
            return groundEffector.groundType;
        }
        else
        {
            return GroundType.LevelGeom;
        }
    }

    /*private void DrawRays2Debug(Vector2 direction,Color color)
    {
        for (int i=0; i < _raycastPosition.Length; i++)
        {
            Debug.DrawRay(_raycastPosition[i], direction * raycastDist, color);
        }
    }*/
}
