﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    private bool _isFacingRight;
    private CharacterController2D _controller;

    //public float MaxSpeed = 8;  //per second
    public static float attentionScore ;
    public static float attentionLvl;
    public static float CurrentSpeed;
    public static float MaxSpeed = 8;
    public float SpeedAccelerationOnGround = 10f;
    public float SpeedAccelerationInAir = 5f;
    private float _normalizedHorizontalSpeed;  //1 for moving right, -1 for moving left, 0 stops

    public void Start()
    {
        _controller = GetComponent<CharacterController2D>();
        _isFacingRight = transform.localScale.x > 0; //not flipped; localScale==1 if facing right, -1 facing left
        CurrentSpeed = 0;  // MaxSpeed = 8;
        attentionScore = 0;
        attentionLvl = 0;
        _normalizedHorizontalSpeed = 1;
    }

    public static void UpdateHorizontalSpeed() {
        int threshold = 3;

        /*if (attentionScore > 100) {
            attentionScore = 100;
        }*/

        CurrentSpeed = attentionLvl / threshold * MaxSpeed;
       // Debug.Log("Current Speed = " + CurrentSpeed);
    }


    public void Update() {

        HandleInput();  //so that input is handled only when the player is alive

        var movementFactor = _controller.State.IsGrounded ? SpeedAccelerationOnGround : SpeedAccelerationInAir;

        //lerp(cur_velocity, target_velocity, time)
        //print("Velocity.x = " + _controller.Velocity.x + "_normalizedHorizontalSpeed * CurrentSpeed"+ _normalizedHorizontalSpeed * CurrentSpeed);
        var horizontalForce = Mathf.Lerp(_controller.Velocity.x, _normalizedHorizontalSpeed * CurrentSpeed, Time.deltaTime * movementFactor);
        //print("horizontalForce = " + horizontalForce);
        _controller.SetHorizontalForce(horizontalForce);
    }


    public void HandleInput() {
        if (Input.GetKey(KeyCode.D))
        {
            _normalizedHorizontalSpeed = 1;
            if (!_isFacingRight)
                Flip();
        }
        else if (Input.GetKey(KeyCode.A))
        {
            _normalizedHorizontalSpeed = -1;
            if (_isFacingRight)
                Flip();
        }
        /*else {
            _normalizedHorizontalSpeed = 1; 
        }*/

        if (_controller.CanJump && Input.GetKeyDown(KeyCode.Space)) {
            _controller.Jump();
        }

    }

        private void Flip() {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            _isFacingRight = transform.localScale.x > 0;
        }
    
}
