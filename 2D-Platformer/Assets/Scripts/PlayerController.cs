using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GlobalTypes;
using System;

public class PlayerController : MonoBehaviour
{
    #region public properties
    [Header("Player Properties")]
    public float walkSpeed = 15f;
    public float creepSpeed = 7.5f;
    public float gravity = 35f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 10f;
    public float xWallJumpSpeed = 15f;
    public float yWallJumpSpeed = 15f;
    public float wallRunAmount = 8f;
    public float wallSlideAmount = .1f;
    public float glideTime = 2f;
    public float glideDescentAmount = 2f;
    public float powerJumpSpeed = 35f;
    public float powerJumpWaitTime = 1.5f;
    public float dashSpeed = 20f;
    public float dashTime = .2f;
    public float dashCooldownTime = 1f;
    public float groundSlamSpeed = 80f;
    public float deadzoneValue = .15f;
    public float swimSpeed = 150f;

    [Header("Player Abilites")]
    public bool canDoubleJump;
    public bool canWallJump;
    public bool canJumpAfterWallJump;
    public bool canWallRun;
    public bool canMultipleWallRun;
    public bool canWallSlide;
    public bool canGlide;
    public bool canGlideAfterWallContact;
    public bool canPowerJump;
    public bool canGroundDash;
    public bool canAirDash;
    public bool canGroundSlam;
    public bool canSwim;

    [Header("Player State")]
    public bool isJumping;
    public bool isDoubleJumping;
    public bool isWallJumping;
    public bool isWallRunning;
    public bool isWallSliding;
    public bool isDucking;
    public bool isCreeping;
    public bool isGliding;
    public bool isPowerJumping;
    public bool isDashing;
    public bool isGroundSlamming;
    public bool isSwimming;
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

    private bool _facingRight;
    private float _dashTimer;

    private float _jumpPadAmount = 15f;
    private float _jumpPadAdjustment = 0f;
    public Vector2 _tempVelocity;
    #endregion
    void Start()
    {
        _characterController2D = gameObject.GetComponent<CharacterController2D>();
        _capsuleCollider2D = gameObject.GetComponent<CapsuleCollider2D>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _originalColliderSize = _capsuleCollider2D.size;
    }

    void OnGround()
    {
        if(_characterController2D.hitGroundFrame)
        {
            _tempVelocity = _moveDirection;
        }
        //clear any downward movement
        _moveDirection.y = 0f;
        isGliding = false;
        ClearAirAbilityFlags();
        Jump();
        DuckingAndCreeping();
        JumpPad();
    }

    private void JumpPad()
    {
        if(_characterController2D.groundType == GroundType.JumpPad)
        {
            _jumpPadAmount = _characterController2D.jumpPadAmount;
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
            if(_moveDirection.y > _characterController2D.jumpPadUpperLimit)
            {
                _moveDirection.y = _characterController2D.jumpPadUpperLimit;
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
                    Vector2.up, _originalColliderSize.y * 10, _characterController2D.layerMask);
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
            if (canPowerJump && isDucking && _characterController2D.groundType != GroundType.OneWayPlatform && (_powerJumpTimer > powerJumpWaitTime))
            {
                _moveDirection.y = powerJumpSpeed;
                StartCoroutine("PowerJumpWaiter");
            }
            //check to see if we are on a one way platform
            else if(isDucking && _characterController2D.groundType == GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(true));
            }
            else
            {
                _moveDirection.y = jumpSpeed;
            }
            isJumping = true;
            _characterController2D.DisableCheckGround();
            _characterController2D.ClearMovingPlatform();
            _ableToWallRun = true;
        }
    }

    private void ClearAirAbilityFlags()
    {
        //clearing flags
        isJumping = false;
        isDoubleJumping = false;
        isWallJumping = false;
        _currentGlideTime = glideTime;
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
        if (canWallRun && (_characterController2D.left || _characterController2D.right))
        {
            if(_characterController2D.left && _characterController2D.leftWallEffector && !_characterController2D.leftIsRunnable)
            {
                return;
            }
            else if(_characterController2D.right && _characterController2D.rightWallEffector && !_characterController2D.rightIsRunnable)
            {
                return;
            }
            if (_input.y > 0 && _ableToWallRun)
            {
                _moveDirection.y = wallRunAmount;
                if (_characterController2D.left)
                {
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                else if (_characterController2D.right)
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
                StartCoroutine("WallRunWaiter");
            }
        }
        else
        {
            //player can jump off to other wall and continue wall running there
            if (canMultipleWallRun)
            {
                StopCoroutine("WallRunWaiter");
                _ableToWallRun = true;
                isWallRunning = false;
            }
        }
        //canGlideAfterWallContact
        if ((_characterController2D.left || _characterController2D.right) && canWallRun)
        {
            if (canGlideAfterWallContact)
            {
                _currentGlideTime = glideTime;
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
            if (canDoubleJump && (!_characterController2D.left && !_characterController2D.right))
            //check if there is nothing on left or right side of the character
            {
                if (!isDoubleJumping)
                {
                    _moveDirection.y = doubleJumpSpeed;
                    isDoubleJumping = true;
                }
            }

            //jump in water
            if(_characterController2D.inWater)
            {
                isDoubleJumping = false;
                _moveDirection.y = jumpSpeed;
            }
            //wall jump
            if (canWallJump && (_characterController2D.left || _characterController2D.right))
            {
                if(_characterController2D.left && _characterController2D.leftWallEffector && !_characterController2D.leftIsJumpable)
                {
                    return;
                }
                else if(_characterController2D.right && _characterController2D.rightWallEffector && !_characterController2D.rightIsJumpable)
                {
                    return;
                }
                if (_moveDirection.x <= 0 && _characterController2D.left)
                {
                    _moveDirection.x = xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
                else if (_moveDirection.x >= 0 && _characterController2D.right)
                {
                    _moveDirection.x = -xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                StartCoroutine("WallJumpWaiter");
                if (canJumpAfterWallJump)
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
                    Vector2.up, _originalColliderSize.y * 10, _characterController2D.layerMask);
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
        if (!isWallJumping)
        {
            _moveDirection.x = _input.x; //x value of input from the player

            if (_moveDirection.x < 0)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                _facingRight = false;
            }
            else if (_moveDirection.x > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                _facingRight = true;
            }
            if (isDashing)
            {
                if (_facingRight)
                {
                    _moveDirection.x = dashSpeed;
                }
                else
                {
                    _moveDirection.x = -dashSpeed;
                }
                _moveDirection.y = 0;
            }
            else if(isCreeping)
            {
                _moveDirection.x *= creepSpeed;
            }
            else
            {
                _moveDirection.x *= walkSpeed; //affect the x value with walk speed
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
        if (_characterController2D.below)
        {
            OnGround();
        }
        else if(_characterController2D.inWater)
        {
            InWater();
        }
        //if player is in the air
        else
        {
            InAir();
        }
        _characterController2D.Move(_moveDirection * Time.deltaTime);
    }

    private void InWater()
    {
        ClearGroundAbilityFlags();
        AirJump();
        if(_input.y !=0f && canSwim && !_holdJump)
        {
            if(!_characterController2D.isSubmerged && _input.y>0)
            {
                _moveDirection.y = 0f;
            }
            else
            {
                _moveDirection.y = (_input.y * swimSpeed) * Time.deltaTime;
            }
        }
        else if(_moveDirection.y < 0f && _input.y ==0f)
        {
            _moveDirection.y += 2f;
        }
    }

    private void ApplyDeadzones()
    {
        if(_input.x > -deadzoneValue && _input.x < deadzoneValue)
        {
            _input.x = 0f;
        }
        if (_input.y > -deadzoneValue && _input.y < deadzoneValue)
        {
            _input.y = 0f;
        }
    }

    void GravityCalculations()
    {
        //detects if something above player
        if(_moveDirection.y > 0f && _characterController2D.above) //if character detects something above, reset the momentum to upside
        {
            if(_characterController2D.ceilingType == GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(false));
            }
            else
            {
                _moveDirection.y = 0f;
            }
        }

        //if we are wall sliding, gravity affect can be different
        if(canWallSlide && (_characterController2D.right || _characterController2D.left))
        {
            if(_characterController2D.hitWallFrame)
            {
                _moveDirection.y = 0f;
            }
            if(_moveDirection.y <=0)
            {
                if(_characterController2D.left && _characterController2D.leftWallEffector)
                {
                    _moveDirection.y -= (gravity * _characterController2D.leftSlideModifier) * Time.deltaTime;
                }
                else if(_characterController2D.right && _characterController2D.rightWallEffector)
                {
                    _moveDirection.y -= (gravity * _characterController2D.rightSlideModifier) * Time.deltaTime;
                }
                else
                {
                    _moveDirection.y -= (gravity * wallSlideAmount) * Time.deltaTime;
                }
            }
            else
            {
                _moveDirection.y -= gravity * Time.deltaTime;
            }
        }
        //if we are gliding, gravity affect can be different
        else if(canGlide && _input.y > 0f && _moveDirection.y < 0.2f)
        {
            if(_currentGlideTime>0f)
            {
                isGliding = true;
                if(_startGlide)
                {
                    _moveDirection.y = 0;
                    _startGlide = false;
                }
                _moveDirection.y -= glideDescentAmount * Time.deltaTime;
                _currentGlideTime -= Time.deltaTime;
            }
            //gliding is over
            else
            {
                isGliding = false;
                _moveDirection.y -= gravity * Time.deltaTime;
            }
        }
        //ground slamming
        else if(isGroundSlamming && !isPowerJumping && _moveDirection.y <0f)
        {
            _moveDirection.y = -groundSlamSpeed;
        }
        //regular gravity affect
        else if(!isDashing)
        {
            _moveDirection.y -= gravity * Time.deltaTime; //add gravity to y value
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
            if((canAirDash && !_characterController2D.below) 
                || (canGroundDash && _characterController2D.below))
            {
                StartCoroutine("Dash");
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.performed && _input.y <0f)
        {
            if(canGroundSlam)
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
        yield return new WaitForSeconds(.4f);
        isWallJumping = false;
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
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        _dashTimer = dashCooldownTime;
    }

    IEnumerator DisableOneWayPlatform(bool checkBelow)
    {
        bool originalCanGroundSlam = canGroundSlam;
        GameObject tempOneWayPlatform = null;

        if(checkBelow)
        {
            Vector2 raycastBelow = transform.position - new Vector3(0, _capsuleCollider2D.size.y * .5f * 10, 0f);
            RaycastHit2D hit = Physics2D.Raycast(raycastBelow, Vector2.down, _characterController2D.raycastDist*2, _characterController2D.layerMask);
            if(hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }
        }
        else
        {
            Vector2 raycastAbove = transform.position + new Vector3(0, _capsuleCollider2D.size.y * .5f * 10, 0f);
            RaycastHit2D hit = Physics2D.Raycast(raycastAbove, Vector2.up, _characterController2D.raycastDist*2, _characterController2D.layerMask);
            if (hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }
        }
        if(tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = false;
            canGroundSlam = false;
        }
        yield return new WaitForSeconds(.25f);
        if(tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = true;
            canGroundSlam = originalCanGroundSlam;
        }
    }
    #endregion
}
