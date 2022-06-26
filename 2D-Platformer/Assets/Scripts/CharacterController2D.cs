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

    public WallType leftWallType;
    public bool leftIsRunnable;
    public bool leftIsJumpable;
    public float leftSlideModifier;

    public WallType rightWallType; 
    public bool rightIsRunnable;
    public bool rightIsJumpable;
    public float rightSlideModifier;

    public GroundType ceilingType;

    public WallEffector leftWallEffector;
    public WallEffector rightWallEffector;

    public bool hitGroundFrame;
    public bool hitWallFrame;

    public float jumpPadAmount;
    public float jumpPadUpperLimit;

    public bool inWater;
    public bool isSubmerged;

    //air effector
    public bool inAirEffector;
    public AirEffectorType airEffectorType;
    public float airEffectorSpeed;
    public Vector2 airEffectorDirection;

    private Vector2 _moveAmount;
    private Vector2 _currPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody2D;
    private CapsuleCollider2D _capsuleCollider2D;

    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3]; //gives us info about the object we hit with ray

    private Vector2 _slopeNormal;
    private float _slopeAngle;

    private bool _disableCheckGround;
    private bool _inAirLastFrame;
    private bool _noSideCollisionsLastFrame;

    private Transform _tempMovingPlatform;
    private Vector2 _movingPlatformVelocity;

    private AirEffector _airEffector;


    void Start()
    {
        _rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider2D = gameObject.GetComponent<CapsuleCollider2D>();
    }

    void Update() //to put physics simulations in fixed update is a better practice than putting them in update method, but we made simulation mode to be "update"
    {
        _inAirLastFrame = !below;
        _lastPosition = _rigidbody2D.position;
        _noSideCollisionsLastFrame = (!right && !left);

        //slope adjustment
        if (_slopeAngle != 0 && below == true)
        {
            if((_moveAmount.x > 0f && _slopeAngle > 0f) || (_moveAmount.x < 0f && _slopeAngle <0f))
            {
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(_slopeAngle * Mathf.Deg2Rad) * _moveAmount.x);
                _moveAmount.y *= downForceAdjustment;
            }
        }
        //moving platform adjustment
        if(groundType == GroundType.MovingPlatform)
        {
            //offset the player's movement on the X with moving platform velocity
            _moveAmount.x += MovingPlatformAdjust().x;

            //if platform is moving down
            if (MovingPlatformAdjust().y <0f)
            {
                //offset the player's movement on the Y
                _moveAmount.y += MovingPlatformAdjust().y;
                _moveAmount.y *= downForceAdjustment;
            }
        }

        if(groundType == GroundType.CollapsablePlatform)
        {
            if(MovingPlatformAdjust().y < 0f)
            {
                _moveAmount.y += MovingPlatformAdjust().y;
                _moveAmount.y *= downForceAdjustment*4;
            }
        }
        if(!inWater)
        {
            _currPosition = _lastPosition + _moveAmount;
            _rigidbody2D.MovePosition(_currPosition);
        }
        else
        {
            if(_rigidbody2D.velocity.magnitude < 10f)
            {
                _rigidbody2D.AddForce(_moveAmount * 300f);
            }
        }

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

        if ((right || left) && _noSideCollisionsLastFrame)
        {
            hitWallFrame = true;
        }
        else
        {
            hitWallFrame = false;
        }
    }

    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    private void CheckGround()
    {
        //DrawRays2Debug(Vector2.down, Color.green);
        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider2D.bounds.center, _capsuleCollider2D.size * 8, CapsuleDirection2D.Vertical, 0f, Vector2.down, raycastDist, layerMask);
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
            if(groundType == GroundType.JumpPad)
            {
                JumpPad jumpPad = hit.collider.GetComponent<JumpPad>();
                jumpPadAmount = jumpPad.jumpPadAmount;
                jumpPadUpperLimit = jumpPad.jumpPadUpperLimit;
            }
        }
        else
        {
            groundType = GroundType.None;
            below = false;
            if(_tempMovingPlatform)
            {
                _tempMovingPlatform = null;
            }
        }
    }

    private void CheckOtherCollisions()
    {
        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider2D.bounds.center, _capsuleCollider2D.size *10 *.5f, 0f, Vector2.left , 
            raycastDist*2, layerMask);
        if(leftHit.collider)
        {
            leftWallType = DetermineWallType(leftHit.collider);
            left = true;
            leftWallEffector = leftHit.collider.GetComponent<WallEffector>();
            if(leftWallEffector)
            {
                leftIsRunnable = leftWallEffector.isRunnable;
                leftIsJumpable = leftWallEffector.isJumpable;
                leftSlideModifier = leftWallEffector.wallSlideAmount;
            }
        }
        else
        {
            leftWallType = WallType.None;
            left = false;
        }

        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider2D.bounds.center, _capsuleCollider2D.size *10 *.5f, 0f, Vector2.right , 
            raycastDist*2, layerMask);
        if (rightHit.collider)
        {
            rightWallType = DetermineWallType(rightHit.collider);
            right = true;
            rightWallEffector = rightHit.collider.GetComponent<WallEffector>();
            if (rightWallEffector)
            {
                rightIsRunnable = rightWallEffector.isRunnable;
                rightIsJumpable = rightWallEffector.isJumpable;
                rightSlideModifier = rightWallEffector.wallSlideAmount;
            }
        }
        else
        {
            rightWallType = WallType.None;
            right = false;
        }

        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_capsuleCollider2D.bounds.center, _capsuleCollider2D.size * 10, CapsuleDirection2D.Vertical, 0f, Vector2.up,
            raycastDist, layerMask);

        if(aboveHit.collider)
        {
            ceilingType = DetermineGroundType(aboveHit.collider);
            above = true;
        }
        else
        {
            ceilingType = GroundType.None;
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
            if(groundType == GroundType.MovingPlatform || groundType == GroundType.CollapsablePlatform)
            {
                if(!_tempMovingPlatform)
                {
                    _tempMovingPlatform = collider.transform;
                    if(groundType == GroundType.CollapsablePlatform)
                    {
                        _tempMovingPlatform.GetComponent<CollapsablePlatform>().CollapsePlatform();
                    }
                }
            }
            return groundEffector.groundType;
        }
        else
        {
            if(_tempMovingPlatform)
            {
                _tempMovingPlatform = null;
            }
            return GroundType.LevelGeom;
        }
    }

    private WallType DetermineWallType(Collider2D collider)
    {
        if(collider.GetComponent<WallEffector>())
        {
            WallEffector wallEffector = collider.GetComponent<WallEffector>();
            return wallEffector.wallType;
        }
        else
        {
            return WallType.Normal;
        }
    }

    private Vector2 MovingPlatformAdjust()
    {
        if(_tempMovingPlatform && groundType == GroundType.MovingPlatform)
        {
            _movingPlatformVelocity = _tempMovingPlatform.GetComponent<MovingPlatform>().difference;
            return _movingPlatformVelocity;
        }
        else if(_tempMovingPlatform && groundType == GroundType.CollapsablePlatform)
        {
            _movingPlatformVelocity = _tempMovingPlatform.GetComponent<CollapsablePlatform>().difference;
            return _movingPlatformVelocity;
        }
        else
        {
            return Vector2.zero;
        }
    }

    public void ClearMovingPlatform()
    {
        if(_tempMovingPlatform)
        {
            _tempMovingPlatform = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<BuoyancyEffector2D>())
        {
            inWater = true;
        }
        if(collision.gameObject.GetComponent<AirEffector>())
        {
            inAirEffector = true;
            _airEffector = collision.gameObject.GetComponent<AirEffector>(); //this lets us call methods on that component
            airEffectorType = _airEffector.airEffectorType;
            airEffectorSpeed = _airEffector.speed;
            airEffectorDirection = _airEffector.direction;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.bounds.Contains(_capsuleCollider2D.bounds.min) && collision.bounds.Contains(_capsuleCollider2D.bounds.max)
            && collision.gameObject.GetComponent<BuoyancyEffector2D>())
        {
            isSubmerged = true;
        }
        else
        {
            isSubmerged = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<BuoyancyEffector2D>())
        {
            _rigidbody2D.velocity = Vector2.zero;
            inWater = false;
        }
        if(collision.gameObject.GetComponent<AirEffector>())
        {
            inAirEffector = false;
            _airEffector = null;
            airEffectorType = AirEffectorType.None;
            airEffectorSpeed = 0f;
            airEffectorDirection = Vector2.zero;
        }
    }
    public void DeactivateAirEffector()
    {
        if (_airEffector)
            _airEffector.DeactivateEffector();
    }
}
