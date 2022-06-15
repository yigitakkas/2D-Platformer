using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour

{
    private Vector2 _moveAmount;
    private Vector2 _currPosition;
    private Vector2 _lastPosition;
    private Rigidbody2D _rigidbody2D;
    private CapsuleCollider2D _capsuleCollider2D;

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
    }

    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }
}
