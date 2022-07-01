using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class CharacterController2D : MonoBehaviour

{
    [Header("General Settings")]
    [SerializeField] float raycastDist = .2f;
    [SerializeField] LayerMask layerMask; //this is used for recast to only collide with LevelGeom layer
    [SerializeField] ControllerMoveType moveType = ControllerMoveType.nonPhysicsBased;
    [SerializeField] float downForceAdjustment = 1.2f;
    [SerializeField] float slopeAngleLimit = 45f;

    [Header("Collision Flags")]
    [SerializeField] bool below; //if true, something is below the character
    [SerializeField] bool right;
    [SerializeField] bool left;
    [SerializeField] bool above;
    [SerializeField] bool hitGroundFrame;
    [SerializeField] bool hitWallFrame;

    [Header("Collision Information")]
    [SerializeField] GroundType groundType;
    [SerializeField] WallType leftWallType;
    [SerializeField] bool leftIsRunnable;
    [SerializeField] bool leftIsJumpable;
    [SerializeField] float leftSlideModifier;
    [SerializeField] WallType rightWallType;
    [SerializeField] bool rightIsRunnable;
    [SerializeField] bool rightIsJumpable;
    [SerializeField] float rightSlideModifier;
    [SerializeField] GroundType ceilingType;
    [SerializeField] WallEffector leftWallEffector;
    [SerializeField] WallEffector rightWallEffector;
    [SerializeField] float jumpPadAmount;
    [SerializeField] float jumpPadUpperLimit;

    [Header("Air Effector Information")]
    [SerializeField] bool inAirEffector;
    [SerializeField] AirEffectorType airEffectorType;
    [SerializeField] float airEffectorSpeed;
    [SerializeField] Vector2 airEffectorDirection;

    [Header("Water Effector Information")]
    [SerializeField] bool inWater;
    [SerializeField] bool isSubmerged;

    Vector2 _moveAmount;
    Vector2 _currPosition;
    Vector2 _lastPosition;
    Rigidbody2D _rigidbody2D;
    CapsuleCollider2D _capsuleCollider2D;
    Vector2[] _raycastPosition = new Vector2[3];
    RaycastHit2D[] _raycastHits = new RaycastHit2D[3]; //gives us info about the object we hit with ray
    Vector2 _slopeNormal;
    float _slopeAngle;
    bool _disableCheckGround;
    bool _inAirLastFrame;
    bool _noSideCollisionsLastFrame;
    Transform _tempMovingPlatform;
    Vector2 _movingPlatformVelocity;
    AirEffector _airEffector;

    #region properties
    public float RaycastDist { get => raycastDist; }
    public LayerMask LayerMask { get => layerMask; }
    public float DownForceAdjustment { get => downForceAdjustment; }
    public float SlopeAngleLimit { get => slopeAngleLimit; }
    public bool Below { get => below; }
    public bool Right { get => right; }
    public bool Left { get => left; }
    public bool Above { get => above; }
    public bool HitGroundFrame { get => hitGroundFrame; }
    public bool HitWallFrame { get => hitWallFrame; }
    public GroundType GroundType { get => groundType; }
    public WallType LeftWallType { get => leftWallType; }
    public bool LeftIsRunnable { get => leftIsRunnable; }
    public bool LeftIsJumpable { get => leftIsJumpable; }
    public float LeftSlideModifier { get => leftSlideModifier; }
    public WallType RightWallType { get => rightWallType; }
    public bool RightIsRunnable { get => rightIsRunnable; }
    public bool RightIsJumpable { get => rightIsJumpable; }
    public float RightSlideModifier { get => rightSlideModifier; }
    public GroundType CeilingType { get => ceilingType; }
    public WallEffector LeftWallEffector { get => leftWallEffector; }
    public WallEffector RightWallEffector { get => rightWallEffector; }
    public float JumpPadAmount { get => jumpPadAmount; }
    public float JumpPadUpperLimit { get => jumpPadUpperLimit; }
    public bool InAirEffector { get => inAirEffector; }
    public AirEffectorType AirEffectorType { get => airEffectorType; }
    public float AirEffectorSpeed { get => airEffectorSpeed; }
    public Vector2 AirEffectorDirection { get => airEffectorDirection; }
    public bool InWater { get => inWater; }
    public bool IsSubmerged { get => isSubmerged; }
    public ControllerMoveType MoveType { get => moveType;}
    #endregion

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
            Vector2 _moveAdjustment = MovingPlatformAdjust();
            //offset the player's movement on the X with moving platform velocity
            _moveAmount.x += _moveAdjustment.x;

            //if platform is moving down
            if (_moveAdjustment.y <0f)
            {
                //offset the player's movement on the Y
                _moveAmount.y += _moveAdjustment.y;
                //_moveAmount.y *= downForceAdjustment;
            }
        }
        if(groundType == GroundType.CollapsablePlatform)
        {
            if(MovingPlatformAdjust().y < 0f)
            {
                _moveAmount.y += MovingPlatformAdjust().y;
                if(!_disableCheckGround && below)
                    _moveAmount.y *= downForceAdjustment*4;
            }
        }
        //tractor beam adjustment
        if (_airEffector && airEffectorType == AirEffectorType.TractorBeam)
        {
            Vector2 airEffectorVector = airEffectorDirection * airEffectorSpeed;
            _moveAmount = Vector2.Lerp(_moveAmount, airEffectorVector, Time.deltaTime);
        }
        if (moveType.Equals(ControllerMoveType.nonPhysicsBased))
        {
            _currPosition = _lastPosition + _moveAmount;
            _rigidbody2D.MovePosition(_currPosition);
        }
        else if(moveType.Equals(ControllerMoveType.physicsBased))
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
        Vector2 raycastLeftOrigin = _rigidbody2D.position - new Vector2(_capsuleCollider2D.size.x * 10 / 2, 0);
        Vector2 raycastUpperLeft = raycastLeftOrigin + (Vector2.up * _capsuleCollider2D.size.y * 10 * .35f);
        Vector2 raycastLowerLeft = raycastLeftOrigin + (Vector2.down * _capsuleCollider2D.size.y * 10 * .35f);
        //Debug.DrawRay(raycastUpperLeft, Vector2.left * raycastDist, Color.green);
        //Debug.DrawRay(raycastLowerLeft, Vector2.left * raycastDist, Color.green);
        RaycastHit2D hitUpperLeft = Physics2D.Raycast(raycastUpperLeft, Vector2.left, raycastDist, layerMask);
        RaycastHit2D hitLowerLeft = Physics2D.Raycast(raycastLowerLeft, Vector2.left, raycastDist, layerMask);
        if(hitUpperLeft.collider&&hitLowerLeft.collider)
        {
            leftWallType = DetermineWallType(hitLowerLeft.collider);
            left = true;
            leftWallEffector = hitLowerLeft.collider.GetComponent<WallEffector>();
            if (leftWallEffector)
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

        Vector2 raycastRightOrigin = _rigidbody2D.position + new Vector2(_capsuleCollider2D.size.x * 10 / 2, 0);
        Vector2 raycastUpperRight = raycastRightOrigin + (Vector2.up * _capsuleCollider2D.size.y * 10 * .35f);
        Vector2 raycastLowerRight = raycastRightOrigin + (Vector2.down * _capsuleCollider2D.size.y * 10 * .35f);
        //Debug.DrawRay(raycastUpperRight, Vector2.right * raycastDist, Color.green);
        //Debug.DrawRay(raycastLowerRight, Vector2.right * raycastDist, Color.green);
        RaycastHit2D hitUpperRight = Physics2D.Raycast(raycastUpperRight, Vector2.right, raycastDist, layerMask);
        RaycastHit2D hitLowerRight = Physics2D.Raycast(raycastLowerRight, Vector2.right, raycastDist, layerMask);
        if (hitUpperRight.collider && hitLowerRight.collider)
        {
            rightWallType = DetermineWallType(hitLowerRight.collider);
            right = true;
            rightWallEffector = hitLowerRight.collider.GetComponent<WallEffector>();
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
                //if(!_tempMovingPlatform)
                //{
                _tempMovingPlatform = collider.transform;
                if(groundType == GroundType.CollapsablePlatform)
                {
                    if (_tempMovingPlatform.TryGetComponent<CollapsablePlatform>(out CollapsablePlatform cp))
                        cp.CollapsePlatform();
                }
                //}
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
            if(_tempMovingPlatform.TryGetComponent<MovingPlatform>(out MovingPlatform mp))
            {
                _movingPlatformVelocity = mp.difference;
                return _movingPlatformVelocity;
            }
            else
            {
                return Vector2.zero;
            }
        }
        else if(_tempMovingPlatform && groundType == GroundType.CollapsablePlatform)
        {
            if(_tempMovingPlatform.TryGetComponent<CollapsablePlatform>(out CollapsablePlatform cp))
            {
                _movingPlatformVelocity = cp.difference;
                return _movingPlatformVelocity;
            }
            else
            {
                return Vector2.zero;
            }
        }
        else
        {
            return Vector2.zero;
        }
    }

    /*public void ClearMovingPlatform()
    {
        if(_tempMovingPlatform)
        {
            _tempMovingPlatform = null;
        }
    }*/

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<BuoyancyEffector2D>())
        {
            inWater = true;
            moveType = ControllerMoveType.physicsBased;
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
            moveType = ControllerMoveType.nonPhysicsBased;
        }
        if(collision.gameObject.GetComponent<AirEffector>())
        {
            inAirEffector = false;
            _airEffector.DeactivateEffector();
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
