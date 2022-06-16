using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class CharacterController2D : MonoBehaviour

{
    public float raycastDist = .2f;
    public LayerMask layerMask; //this is used for recast to only collide with LevelGeom layer
    public float downForceAdjustment = 1.2f;
    public float slopeAngleLimit = 45f;

    public bool below; //if true, something is below the character
    public bool right;
    public bool left;
    public bool above;
    public GroundType groundType;
    public bool hitGroundFrame;

    private Vector2 _moveAmount;
    private Vector2 _currPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody2D;
    private CapsuleCollider2D _capsuleCollider2D;

    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3]; //gives us info about the object we hit with ray

    private bool _disableCheckGround;
    private bool _inAirLastFrame;

    private Vector2 _slopeNormal;
    private float _slopeAngle;

    void Start()
    {
        _rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider2D = gameObject.GetComponent<CapsuleCollider2D>();
    }

    void Update() //to put physics simulations in fixed update is a better practice than putting them in update method, but we made simulation mode to be "update"
    {
        _inAirLastFrame = !below;
        _lastPosition = _rigidbody2D.position;
        if (_slopeAngle != 0 && below == true)
        {
            if((_moveAmount.x > 0f && _slopeAngle > 0f) || (_moveAmount.x < 0f && _slopeAngle <0f))
            {
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(_slopeAngle * Mathf.Deg2Rad) * _moveAmount.x);
                _moveAmount.y *= downForceAdjustment;
            }
        }
        _currPosition = _lastPosition + _moveAmount;
        _rigidbody2D.MovePosition(_currPosition);
        _moveAmount = Vector2.zero; //move amount did its job so reset the value to zero
        if(!_disableCheckGround) //if character is jumping, don't check the ground
        {
            CheckGround();
        }
        CheckOtherCollisions();

        if(below && _inAirLastFrame)
        {
            hitGroundFrame = true;
        }
        else
        {
            hitGroundFrame = false;
        }
    }

    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    private void CheckGround()
    {
        //DrawRays2Debug(Vector2.down, Color.green);
        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider2D.bounds.center, _capsuleCollider2D.size * 10, CapsuleDirection2D.Vertical, 0f, Vector2.down, raycastDist, layerMask);
        if (hit.collider)
        {
            groundType = DetermineGroundType(hit.collider);
            _slopeNormal = hit.normal;
            _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);

            if(_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)
            {
                below = false;
            }
            else
            {
                below = true;
            }
        }
        else
        {
            groundType = GroundType.None;
            below = false;
        }
    }

    private void CheckOtherCollisions()
    {
        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider2D.bounds.center, _capsuleCollider2D.size *10 *.5f, 0f, Vector2.left , 
            raycastDist*2, layerMask);
        if(leftHit.collider)
        {
            left = true;
        }
        else
        {
            left = false;
        }

        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider2D.bounds.center, _capsuleCollider2D.size *10 *.5f, 0f, Vector2.right , 
            raycastDist*2, layerMask);
        if (rightHit.collider)
        {
            right = true;
        }
        else
        {
            right = false;
        }

        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_capsuleCollider2D.bounds.center, _capsuleCollider2D.size * 10, CapsuleDirection2D.Vertical, 0f, Vector2.up,
            raycastDist, layerMask);

        if(aboveHit.collider)
        {
            above = true;
        }
        else
        {
            above = false;
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
