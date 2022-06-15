using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 15f;
    public float gravity = 35f;
    public float jumpSpeed = 15f;

    public bool isJumping;
    private bool _startJump;
    private bool _releaseJump;

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharacterController2D _characterController2D;
    // Start is called before the first frame update
    void Start()
    {
        _characterController2D = gameObject.GetComponent<CharacterController2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection.x = _input.x; //x value of input from the player
        _moveDirection.x *= walkSpeed; //affect the x value with walk speed

        if(_characterController2D.below) //if player is on the round
        {
            if(_startJump)
            {
                _startJump = false;
                _moveDirection.y = jumpSpeed;
                isJumping = true;
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
            _moveDirection.y -= gravity * Time.deltaTime; //add gravity to y value
        }

        _characterController2D.Move(_moveDirection * Time.deltaTime);
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
        }
        else if (context.canceled)
        {
            _releaseJump = true;
        }
    }

}
