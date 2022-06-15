using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float gravity = 20f;

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

        if(_characterController2D.below)
        {

        } 
        else
        {
            _moveDirection.y -= gravity * Time.deltaTime; //add gravity to y value
        }


        _characterController2D.Move(_moveDirection * Time.deltaTime);
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }
}
