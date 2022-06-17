using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Properties")]
    public float walkSpeed = 15f;
    public float gravity = 35f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 10f;
    public float xWallJumpSpeed = 15f;
    public float yWallJumpSpeed = 15f;
    public float wallRunAmount = 8f;
    public float wallSlideAmount = .1f;

    [Header("Player Abilites")]
    public bool canDoubleJump;
    public bool canWallJump;
    public bool canJumpAfterWallJump;
    public bool canWallRun;
    public bool canMultipleWallRun;
    public bool canWallSlide;

    [Header("Player State")]
    public bool isJumping;
    public bool isDoubleJumping;
    public bool isWallJumping;
    public bool isWallRunning;
    public bool isWallSliding;

    private bool _startJump;
    private bool _releaseJump;

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharacterController2D _characterController2D;

    private bool _ableToWallRun=true;
    void Start()
    {
        _characterController2D = gameObject.GetComponent<CharacterController2D>();
    }

    void Update()
    {
        if(!isWallJumping)
        {
            _moveDirection.x = _input.x; //x value of input from the player
            _moveDirection.x *= walkSpeed; //affect the x value with walk speed
        }

        if(_moveDirection.x <0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else if(_moveDirection.x >0)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        if(_characterController2D.below) //if player is on the round
        {
            _moveDirection.y = 0f;

            //clearing flags
            isJumping = false;
            isDoubleJumping = false;
            isWallJumping = false;

            if(_startJump)
            {
                _startJump = false;
                _moveDirection.y = jumpSpeed;
                isJumping = true;
                _characterController2D.DisableCheckGround();
                _ableToWallRun = true;
            }
        } 
        else //if player is in the air
        {
            if(_releaseJump)
            {
                _releaseJump = false;
                if(_moveDirection.y>0)
                {
                    _moveDirection.y *= .5f;
                }
            }
            //if pressed jump button in the air
            if(_startJump)
            {
                //double jump
                if(canDoubleJump && (!_characterController2D.left && !_characterController2D.right)) 
                    //check if there is nothing on left or right side of the character
                {
                    if(!isDoubleJumping)
                    {
                        _moveDirection.y = doubleJumpSpeed;
                        isDoubleJumping = true;
                    }
                }
                //wall jump
                if(canWallJump && (_characterController2D.left || _characterController2D.right))
                {
                    if(_moveDirection.x <=0 && _characterController2D.left)
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
                    if(canJumpAfterWallJump)
                    {
                        isDoubleJumping = false;
                    }
                }
                _startJump = false;
            }
            //wall running
            if(canWallRun && (_characterController2D.left || _characterController2D.right))
            {
                if(_input.y > 0 && _ableToWallRun)
                {
                    _moveDirection.y = wallRunAmount;
                    if(_characterController2D.left)
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
                if(canMultipleWallRun)
                {
                    StopCoroutine("WallRunWaiter");
                    _ableToWallRun = true;
                    isWallRunning = false;
                }
            }
            GravityCalculations();
        }

        _characterController2D.Move(_moveDirection * Time.deltaTime);
    }

    void GravityCalculations()
    {
        if(_moveDirection.y > 0f && _characterController2D.above) //if character detects something above, reset the momentum to upside
        {
            _moveDirection.y = 0f;
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
                _moveDirection.y -= (gravity * wallSlideAmount) * Time.deltaTime;
            }
            else
            {
                _moveDirection.y -= gravity * Time.deltaTime;
            }
        }
        else
        {
            _moveDirection.y -= gravity * Time.deltaTime; //add gravity to y value
        }

    }
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
        }
        else if (context.canceled)
        {
            _releaseJump = true;
            _startJump = false;
        }
    }

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
}
