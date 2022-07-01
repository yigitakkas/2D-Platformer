using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GlobalTypes;
using System;

public class PlayerController : MonoBehaviour
{
    #region public properties
    public PlayerProfile profile;

    [Header("Player State")]
    [SerializeField] bool isJumping;
    [SerializeField] bool isDoubleJumping;
    [SerializeField] bool isWallJumping;
    [SerializeField] bool isWallRunning;
    [SerializeField] bool isWallSliding;
    [SerializeField] bool isDucking;
    [SerializeField] bool isCreeping;
    [SerializeField] bool isGliding;
    [SerializeField] bool isPowerJumping;
    [SerializeField] bool isDashing;
    [SerializeField] bool isGroundSlamming;
    [SerializeField] bool isSwimming;
    #endregion

    #region private properties
    private bool _startJump;
    private bool _releaseJump;
    private bool _holdJump;

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharacterController2D _characterController2D;

    private bool _ableToWallRun=true;

    private CapsuleCollider2D _capsuleCollider2D;
    private Vector2 _originalColliderSize;
    private SpriteRenderer _spriteRenderer; //when I add sprite, I'll remove this.

    private float _currentGlideTime;
    private bool _startGlide = true;

    private float _powerJumpTimer;

    private float _dashTimer;

    private float _jumpPadAmount = 15f;
    private float _jumpPadAdjustment = 0f;
    private Vector2 _tempVelocity;

    private Animator _animator;

    private bool _inAirControl = true;

    #endregion

    public bool IsJumping { get => isJumping; }
    public bool IsDoubleJumping { get => isDoubleJumping; }
    public bool IsWallJumping { get => isWallJumping; }
    public bool IsWallRunning { get => isWallRunning; }
    public bool IsWallSliding { get => isWallSliding; }
    public bool IsDucking { get => isDucking; }
    public bool IsCreeping { get => isCreeping; }
    public bool IsGliding { get => isGliding; }
    public bool IsPowerJumping { get => isPowerJumping; }
    public bool IsDashing { get => isDashing; }
    public bool IsGroundSlamming { get => isGroundSlamming; }
    public bool IsSwimming { get => isSwimming; }
    void Start()
    {
        _characterController2D = gameObject.GetComponent<CharacterController2D>();
        _capsuleCollider2D = gameObject.GetComponent<CapsuleCollider2D>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _originalColliderSize = _capsuleCollider2D.size;
        _animator = GetComponentInChildren<Animator>();
    }

    void OnGround()
    {
        if(_characterController2D.AirEffectorType == AirEffectorType.Ladder)
        {
            InAirEffector();
            return;
        }
        if(_characterController2D.HitGroundFrame)
        {
            _tempVelocity = _moveDirection;
        }
        //clear any downward movement
        _moveDirection.y = 0f;
        isGliding = false;
        profile.canDoubleJump = true;
        ClearAirAbilityFlags();
        Jump();
        DuckingAndCreeping();
        JumpPad();
    }

    private void JumpPad()
    {
        if(_characterController2D.GroundType == GroundType.JumpPad)
        {
            _jumpPadAmount = _characterController2D.JumpPadAmount;
            //if downwards velocity(inverted) is greater than jump pad amount
            if(-_tempVelocity.y > _jumpPadAmount)
            {
                _moveDirection.y = -_tempVelocity.y * .9f;
            }
            else
            {
                _moveDirection.y = _jumpPadAmount;
            }

            //if holding jump button add a little height each time character bounces
            if(_holdJump)
            {
                _jumpPadAdjustment += _moveDirection.y * .1f;
                _moveDirection.y += _jumpPadAdjustment;
            }
            else
            {
                _jumpPadAdjustment = 0;
            }
            if(_moveDirection.y > _characterController2D.JumpPadUpperLimit)
            {
                _moveDirection.y = _characterController2D.JumpPadUpperLimit;
            }
        }
    }

    private void DuckingAndCreeping()
    {
        //ducking or creeping
        if (_input.y < 0f)
        {
            if (!isDucking && !isCreeping)
            {
                _capsuleCollider2D.size = new Vector2(_capsuleCollider2D.size.x, _capsuleCollider2D.size.y / 1.5f);
                _capsuleCollider2D.offset = new Vector2(0f, -.02f);
                transform.position = new Vector2(transform.position.x, transform.position.y - (_originalColliderSize.y / 4));
                isDucking = true;
                _spriteRenderer.sprite = Resources.Load<Sprite>("Adventurer_Ducking");
            }
            _powerJumpTimer += Time.deltaTime;
        }
        else
        {
            //when there is no input, go back to standing state
            if (isDucking || isCreeping)
            {
                RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider2D.bounds.center, transform.localScale / 10f, CapsuleDirection2D.Vertical, 0f,
                    Vector2.up, _originalColliderSize.y * 10, _characterController2D.LayerMask);
                //check if there is something above to prevent from going back to standing state
                if (!hitCeiling.collider)
                {
                    _capsuleCollider2D.size = _originalColliderSize;
                    _capsuleCollider2D.offset = new Vector2(0f, 0f);
                    transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y / 4));
                    _spriteRenderer.sprite = Resources.Load<Sprite>("Adventurer");
                    isDucking = false;
                    isCreeping = false;
                }
            }
            _powerJumpTimer = 0f;
        }
        if (isDucking && _moveDirection.x != 0)
        {
            isCreeping = true;
            _powerJumpTimer = 0f;
        }
        else
        {
            isCreeping = false;
        }
    }

    private void Jump()
    {
        //jumping
        if (_startJump)
        {
            _startJump = false;
            //power jump
            if (profile.canPowerJump && isDucking && _characterController2D.GroundType != GroundType.OneWayPlatform && (_powerJumpTimer > profile.powerJumpWaitTime))
            {
                _moveDirection.y = profile.powerJumpSpeed;
                StartCoroutine("PowerJumpWaiter");
            }
            //check to see if we are on a one way platform
            else if(isDucking && _characterController2D.GroundType == GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(true));
            }
            else
            {
                _moveDirection.y = profile.jumpSpeed;
            }
            isJumping = true;
            _characterController2D.DisableCheckGround();
            //_characterController2D.ClearMovingPlatform();
            _ableToWallRun = true;
        }
    }

    private void ClearAirAbilityFlags()
    {
        //clearing flags
        isJumping = false;
        isDoubleJumping = false;
        isWallJumping = false;
        _currentGlideTime = profile.glideTime;
        isGroundSlamming = false;
        _startGlide = true;
    }

    void InAir()
    {
        ClearGroundAbilityFlags();
        AirJump();
        WallRunning();
        GravityCalculations();
    }

    private void WallRunning()
    {
        //wall running
        isGliding = false;
        if (profile.canWallRun && (_characterController2D.Left || _characterController2D.Right))
        {
            if(_characterController2D.Left && _characterController2D.LeftWallEffector && !_characterController2D.LeftIsRunnable)
            {
                return;
            }
            else if(_characterController2D.Right && _characterController2D.RightWallEffector && !_characterController2D.RightIsRunnable)
            {
                return;
            }
            if (_input.y > 0 && _ableToWallRun)
            {
                ClearAirAbilityFlags();
                _moveDirection.y = profile.wallRunAmount;
                if (_characterController2D.Left && !isWallJumping)
                {
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                else if (_characterController2D.Right && !isWallJumping)
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
                StartCoroutine("WallRunWaiter");
            }
        }
        else
        {
            //player can jump off to other wall and continue wall running there
            if (profile.canMultipleWallRun)
            {
                StopCoroutine("WallRunWaiter");
                _ableToWallRun = true;
                isWallRunning = false;
            }
        }
        //canGlideAfterWallContact
        if ((_characterController2D.Left || _characterController2D.Right) && profile.canWallRun)
        {
            if (profile.canGlideAfterWallContact)
            {
                _currentGlideTime = profile.glideTime;
            }
            else
            {
                _currentGlideTime = 0f;
            }
        }
    }

    private void AirJump()
    {
        if (_releaseJump)
        {
            _releaseJump = false;
            if (_moveDirection.y > 0)
            {
                _moveDirection.y *= .5f;
            }
        }
        //if pressed jump button in the air
        if (_startJump)
        {
            //double jump
            if (profile.canDoubleJump && (!_characterController2D.Left && !_characterController2D.Right))
            //check if there is nothing on left or right side of the character
            {
                if (!isDoubleJumping)
                {
                    _moveDirection.y = profile.doubleJumpSpeed;
                    isDoubleJumping = true;
                }
            }

            //jump in water
            if(_characterController2D.InWater)
            {
                isDoubleJumping = false;
                _moveDirection.y = profile.jumpSpeed;
            }
            //wall jump
            if (profile.canWallJump && (_characterController2D.Left || _characterController2D.Right))
            {
                if(_characterController2D.Left && _characterController2D.LeftWallEffector && !_characterController2D.LeftIsJumpable)
                {
                    return;
                }
                else if(_characterController2D.Right && _characterController2D.RightWallEffector && !_characterController2D.RightIsJumpable)
                {
                    return;
                }
                if (_moveDirection.x <= 0 && _characterController2D.Left)
                {
                    _moveDirection.x = profile.xWallJumpSpeed;
                    _moveDirection.y = profile.yWallJumpSpeed;
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
                else if (_moveDirection.x >= 0 && _characterController2D.Right)
                {
                    _moveDirection.x = -profile.xWallJumpSpeed;
                    _moveDirection.y = profile.yWallJumpSpeed;
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                StartCoroutine("WallJumpWaiter");
                if (profile.canJumpAfterWallJump)
                {
                    isDoubleJumping = false;
                }
            }
            _startJump = false;
        }
    }

    private void ClearGroundAbilityFlags()
    {
        //if we jump when we're ducking
        if (isDucking || isCreeping && _moveDirection.y > 0)
        {
            RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider2D.bounds.center, transform.localScale / 10f, CapsuleDirection2D.Vertical, 0f,
                    Vector2.up, _originalColliderSize.y * 10, _characterController2D.LayerMask);
            //if there is nothing above, return back to standing pose 
            if (!hitCeiling.collider)
            {
                _capsuleCollider2D.size = _originalColliderSize;
                _capsuleCollider2D.offset = new Vector2(0f, 0f);
                _spriteRenderer.sprite = Resources.Load<Sprite>("Adventurer");
                isDucking = false;
                isCreeping = false;
            }
        }
        _powerJumpTimer = 0f;
    }

    private void ProcessHorizontalMovement()
    {
        if (!_inAirControl || (isWallJumping && _input.x ==0))
            return;
        else
        {
            _moveDirection.x = _input.x; //x value of input from the player

            if (_moveDirection.x < 0)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else if (_moveDirection.x > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            if (isDashing)
            {
                if (transform.rotation.y == 0)
                {
                    _moveDirection.x = profile.dashSpeed;
                }
                else
                {
                    _moveDirection.x = -profile.dashSpeed;
                }
                _moveDirection.y = 0;
            }
            else if(isCreeping)
            {
                _moveDirection.x *= profile.creepSpeed;
            }
            else
            {
                _moveDirection.x *= profile.walkSpeed; //affect the x value with walk speed
            }
        }
    }

    void Update()
    {
        //counting down from dash cooldown time 
        if(_dashTimer>0)
            _dashTimer -= Time.deltaTime;

        ApplyDeadzones();

        ProcessHorizontalMovement();
        //if player is on the ground
        if (_characterController2D.Below)
        {
            OnGround();
        }
        else if(_characterController2D.InAirEffector)
        {
            InAirEffector();
        }
        else if(_characterController2D.InWater)
        {
            InWater();
        }
        //if player is in the air
        else
        {
            InAir();
        }
        _characterController2D.Move(_moveDirection * Time.deltaTime);
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        _animator.SetFloat("movementX", Mathf.Abs(_moveDirection.x / profile.walkSpeed));
        _animator.SetFloat("movementY", _moveDirection.y);
        _animator.SetBool("isGrounded", _characterController2D.Below);
        _animator.SetBool("isJumping", isJumping);
        _animator.SetBool("doubleJumped", isDoubleJumping);
        _animator.SetBool("wallJumped", isWallJumping);
        _animator.SetBool("isWallRunning", isWallRunning);
        _animator.SetBool("isGliding", isGliding);
        _animator.SetBool("isDucking", isDucking);
        _animator.SetBool("isCreeping", isCreeping);
        _animator.SetBool("isPowerJumping", isPowerJumping);
        _animator.SetBool("isStomping", isGroundSlamming);
        _animator.SetBool("isSwimming", _characterController2D.InWater);
    }

    private void InAirEffector()
    {
        if(_startJump)
        {
            _characterController2D.DeactivateAirEffector();
            Jump();
        }
        //process movement when on ladder
        if(_characterController2D.AirEffectorType==AirEffectorType.Ladder)
        {
            if(_input.y > 0f)
            {
                _moveDirection.y = _characterController2D.AirEffectorSpeed;
            }
            else if(_input.y < 0f)
            {
                _moveDirection.y = -_characterController2D.AirEffectorSpeed;
            }
            else
            {
                _moveDirection.y = 0f;
            }
        }
        //process movement when in tractor beam
        if(_characterController2D.AirEffectorType==AirEffectorType.TractorBeam)
        {
            if (_moveDirection.y != 0f)
                _moveDirection.y = Mathf.Lerp(_moveDirection.y, 0f, Time.deltaTime * 4f);
        }
        //process movement when gliding in an updraft
        if(_characterController2D.AirEffectorType==AirEffectorType.Updraft)
        {
            if(_input.y <=0f)
            {
                isGliding = false;
            }
            if(isGliding)
            {
                _moveDirection.y = _characterController2D.AirEffectorSpeed;
            }
            else
            {
                InAir();
            }
        }

    }

    private void InWater()
    {
        ClearGroundAbilityFlags();
        ClearAirAbilityFlags();
        AirJump();
        profile.canDoubleJump = false;
        if(_input.y !=0f && profile.canSwim && !_holdJump)
        {
            if(!_characterController2D.IsSubmerged && _input.y>0)
            {
                _moveDirection.y = 0f;
            }
            else
            {
                _moveDirection.y = (_input.y * profile.swimSpeed) * Time.deltaTime;
            }
        }
        else if(_moveDirection.y < 0f && _input.y ==0f)
        {
            _moveDirection.y += 2f;
        }
    }

    private void ApplyDeadzones()
    {
        if(_input.x > -profile.deadzoneValue && _input.x < profile.deadzoneValue)
        {
            _input.x = 0f;
        }
        if (_input.y > -profile.deadzoneValue && _input.y < profile.deadzoneValue)
        {
            _input.y = 0f;
        }
    }

    void GravityCalculations()
    {
        //detects if something above player
        if(_moveDirection.y > 0f && _characterController2D.Above) //if character detects something above, reset the momentum to upside
        {
            if(_characterController2D.CeilingType == GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(false));
            }
            else
            {
                _moveDirection.y = 0f;
            }
        }

        //if we are wall sliding, gravity affect can be different
        if(profile.canWallSlide && (_characterController2D.Right || _characterController2D.Left))
        {
            if(_characterController2D.HitWallFrame)
            {
                _moveDirection.y = 0f;
            }
            if(_moveDirection.y <=0)
            {
                if(_characterController2D.Left && _characterController2D.LeftWallEffector)
                {
                    _moveDirection.y -= (profile.gravity * _characterController2D.LeftSlideModifier) * Time.deltaTime;
                }
                else if(_characterController2D.Right && _characterController2D.RightWallEffector)
                {
                    _moveDirection.y -= (profile.gravity * _characterController2D.RightSlideModifier) * Time.deltaTime;
                }
                else
                {
                    _moveDirection.y -= (profile.gravity * profile.wallSlideAmount) * Time.deltaTime;
                }
            }
            else
            {
                _moveDirection.y -= profile.gravity * Time.deltaTime;
            }
        }
        //if we are gliding, gravity affect can be different
        else if(profile.canGlide && _input.y > 0f && _moveDirection.y < 0.2f)
        {
            if(_currentGlideTime>0f)
            {
                isGliding = true;
                if(_startGlide)
                {
                    _moveDirection.y = 0;
                    _startGlide = false;
                }
                _moveDirection.y -= profile.glideDescentAmount * Time.deltaTime;
                _currentGlideTime -= Time.deltaTime;
            }
            //gliding is over
            else
            {
                isGliding = false;
                _moveDirection.y -= profile.gravity * Time.deltaTime;
            }
        }
        //ground slamming
        else if(isGroundSlamming && !isPowerJumping && _moveDirection.y <0f)
        {
            _moveDirection.y = -profile.groundSlamSpeed;
        }
        //regular gravity affect
        else if(!isDashing)
        {
            _moveDirection.y -= profile.gravity * Time.deltaTime; //add gravity to y value
        }

    }
    #region Input Methods
    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnJump (InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _startJump = true;
            _releaseJump = false;
            _holdJump = true;
        }
        else if (context.canceled)
        {
            _releaseJump = true;
            _startJump = false;
            _holdJump = false;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if(context.started && _dashTimer <=0)
        {
            if((profile.canAirDash && !_characterController2D.Below) 
                || (profile.canGroundDash && _characterController2D.Below))
            {
                StartCoroutine("Dash");
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.performed && _input.y <0f)
        {
            if(profile.canGroundSlam)
            {
                isGroundSlamming = true;
            }
        }
    }
    #endregion

    #region Coroutines
    IEnumerator WallJumpWaiter()
    {
        isWallJumping = true;
        _inAirControl = false;
        yield return new WaitForSeconds(profile.wallJumpDelay);
        _inAirControl = true;
        //isWallJumping = false;
    }

    IEnumerator WallRunWaiter()
    {
        isWallRunning = true;
        yield return new WaitForSeconds(.5f);
        isWallRunning = false;
        if(!isWallJumping)
        {
            _ableToWallRun = false;
        }
    }

    IEnumerator PowerJumpWaiter()
    {
        isPowerJumping = true;
        yield return new WaitForSeconds(.8f);
        isPowerJumping = false;
    }

    IEnumerator Dash()
    {
        isDashing = true;
        yield return new WaitForSeconds(profile.dashTime);
        isDashing = false;
        _dashTimer = profile.dashCooldownTime;
    }

    IEnumerator DisableOneWayPlatform(bool checkBelow)
    {
        bool originalCanGroundSlam = profile.canGroundSlam;
        GameObject tempOneWayPlatform = null;

        if(checkBelow)
        {
            Vector2 raycastBelow = transform.position - new Vector3(0, _capsuleCollider2D.size.y * .5f * 10, 0f);
            RaycastHit2D hit = Physics2D.Raycast(raycastBelow, Vector2.down, _characterController2D.RaycastDist*2, _characterController2D.LayerMask);
            if(hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }
        }
        else
        {
            Vector2 raycastAbove = transform.position + new Vector3(0, _capsuleCollider2D.size.y * .5f * 10, 0f);
            RaycastHit2D hit = Physics2D.Raycast(raycastAbove, Vector2.up, _characterController2D.RaycastDist*2, _characterController2D.LayerMask);
            if (hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }
        }
        if(tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = false;
            profile.canGroundSlam = false;
        }
        yield return new WaitForSeconds(.4f);

        if(tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = true;
            profile.canGroundSlam = originalCanGroundSlam;
        }
    }
    #endregion
}
