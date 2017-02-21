using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour {

    private const float SkinWidth = .02f;
    private const int TotalHorizontalRays = 8;
    private const int TotalVerticalRays = 4;

    private static readonly float SlopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);

    public LayerMask PlatformMask;  //for collidable, to decide which layer to collide with or not
    public ControllerParameters2D DefaultParameters; //to edit the default parameters in default ControllerParameters2D
    public GameObject StandingOn { get; private set; }

    public ControllerState2D State { get; private set; }
    public Vector2 Velocity { get { return _velocity; } }//make it not modifiable outside
    private Vector2 _velocity;
    public bool CanJump {
        get {
            if (Parameters.JumpRestrictions == ControllerParameters2D.JumpBehavior.CanJumpAnywhere)
                return _jumpIn <= 0;

            if (Parameters.JumpRestrictions == ControllerParameters2D.JumpBehavior.CanJumpOnGround)
                return State.IsGrounded;

            return false; } }
    public bool HandleCollisions { get; set; }
    public ControllerParameters2D Parameters { get { return _overrideParameters ?? DefaultParameters; } }

    private Transform _transform;
    private Vector3 _localScale;
    private BoxCollider2D _boxCollider;
    private ControllerParameters2D _overrideParameters;
    private float _jumpIn; //record the timelapse for the last jump, constraint by the jump freq
    private Vector3
        _raycastTopLeft,
        _raycastBottomLeft,
        _raycastBottomRight;

    private float
        _verticalDistanceBetweenRays,
        _horizontalDistanceBetweenRays;
    

    public void Awake() {
        HandleCollisions = true;
        State = new global::ControllerState2D();
        _transform = transform;
        _localScale = transform.localScale;
        _boxCollider = GetComponent<BoxCollider2D>();

        var colliderWidth = _boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2 * SkinWidth);
        _horizontalDistanceBetweenRays = colliderWidth / (TotalHorizontalRays - 1);

        var colliderHeight = _boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2 * SkinWidth);
        _verticalDistanceBetweenRays = colliderHeight / (TotalVerticalRays - 1);

    }
    public void AddForce(Vector2 force) {
        _velocity = force;
    }

    public void SetForce(Vector2 force) {
        _velocity += force;
    }

    public void SetHorizontalForce(float x) {
        _velocity.x = x;
    }

    public void SetVerticalForce(float y) {
        _velocity.y = y;
    }

    public void Jump() {
        //TODO:Moving Platform support
        AddForce(new Vector2(0, Parameters.JumpMagnitude));
        _jumpIn = Parameters.JumpFrequency;
    }

    public void LateUpdate() {
        _jumpIn -= Time.deltaTime;
        _velocity.y += Parameters.Gravity * Time.deltaTime; //set a constant gravity
        //_velocity.x += (-Parameters.Gravity * Time.deltaTime); //set a constant 
        Move(Velocity * Time.deltaTime);
    }

    private void Move(Vector2 deltaMovement) {
      
        var wasGrounded = State.IsCollidingBelow;
        State.Reset();

        if (HandleCollisions) { 

            HandlePlatforms();//moving platforms
            CalculateRaysOrigins();//each frame, the rays position depends on the player position

            if (deltaMovement.y < 0 && wasGrounded)  //moving down
                HandleVerticalSlope(ref deltaMovement); //possible modification

            if (Mathf.Abs(deltaMovement.x) > .001f)
                MoveHorizontally(ref deltaMovement);

            MoveVertically(ref deltaMovement);
        }

     
        _transform.Translate(deltaMovement, Space.World);

        //TODO: Additional moving platform code

        if (Time.deltaTime > 0)
            _velocity = deltaMovement / Time.deltaTime;

        _velocity.x = Mathf.Min(_velocity.x, Parameters.MaxVelocity.x);
        _velocity.y = Mathf.Min(_velocity.y, Parameters.MaxVelocity.y);

        if (State.IsMovingUpSlope)
            _velocity.y = 0;
    }

    private void HandlePlatforms() { }

    private void CalculateRaysOrigins() {

        var size = new Vector2(_boxCollider.size.x * Mathf.Abs(_localScale.x), _boxCollider.size.y * Mathf.Abs(_localScale.y)) / 2;
        var center = new Vector2(_boxCollider.offset.x * _localScale.x, _boxCollider.offset.y * _localScale.y);

        _raycastTopLeft = _transform.position + new Vector3(center.x - size.x + SkinWidth, center.y + size.y - SkinWidth);
        _raycastBottomRight = _transform.position + new Vector3(center.x + size.x - SkinWidth, center.y - size.y + SkinWidth);
        _raycastBottomLeft = _transform.position + new Vector3(center.x - size.x + SkinWidth, center.y - size.y + SkinWidth);

    }

    private void MoveHorizontally(ref Vector2 deltaMovement) {
        //deltaMovement is the velocity of the object in the frame
        //MoveHorizontally helps to contraint the x-movement of the object if collide with platform horizontally
        var isGoingRight = deltaMovement.x > 0;
        var rayDistance = Mathf.Abs(deltaMovement.x) + SkinWidth;
        var rayDirection = isGoingRight ? Vector2.right : - Vector2.right; //unit vector
        var rayOrigin = isGoingRight ? _raycastBottomRight : _raycastBottomLeft;


        //loop to draw ind rays
        for (var i = 0; i < TotalHorizontalRays; i++) {
            var rayVector = new Vector2(rayOrigin.x, rayOrigin.y + (i * _verticalDistanceBetweenRays));
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);

            var rayCastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask);
            if (!rayCastHit)
                continue;

            //handles the situation where the slope is not vertical
            if (i == 0 && HandleHorizontalSlope(ref deltaMovement, Vector2.Angle(rayCastHit.normal, Vector2.up), isGoingRight))
                break;

            deltaMovement.x = rayCastHit.point.x - rayVector.x;
            rayDistance = Mathf.Abs(deltaMovement.x);

            if (isGoingRight)
            {
                deltaMovement.x -= SkinWidth;
                State.IsCollidingRight = true;
            }
            else {
                deltaMovement.x += SkinWidth;
                State.IsCollidingLeft = true;
            }

            if (rayDistance < SkinWidth + .0001f)
                break;
       
        }
    }

    private void MoveVertically(ref Vector2 deltaMovement) {
        //occurs every frame, so that the player does not fall into the ground
        var isGoingUp = deltaMovement.y > 0;
        var rayDistance = Mathf.Abs(deltaMovement.y) + SkinWidth;
        var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
        var rayOrigin = isGoingUp ? _raycastTopLeft : _raycastBottomLeft;

        rayOrigin.x += deltaMovement.x;

        var standingOnDistance = float.MaxValue;
        for (var i = 0; i < TotalVerticalRays; i++) {
            var rayVector = new Vector2(rayOrigin.x + (i * _horizontalDistanceBetweenRays), rayOrigin.y);
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);

            var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask);
            if (!raycastHit)
                continue;

            if (!isGoingUp) {
                var verticalDistanceToHit = _transform.position.y - raycastHit.point.y;
                if (verticalDistanceToHit < standingOnDistance) {
                    standingOnDistance = verticalDistanceToHit;
                    StandingOn = raycastHit.collider.gameObject; // contain the platform standing on
                }
            }

            deltaMovement.y = raycastHit.point.y - rayVector.y;
            rayDistance = Mathf.Abs(deltaMovement.y);

            if (isGoingUp)
            {
                deltaMovement.y -= SkinWidth;
                State.IsCollidingAbove = true;
            }
            else {
                deltaMovement.y += SkinWidth;
                State.IsCollidingBelow = true;
            }

            if (!isGoingUp && deltaMovement.y > .0001f)
                State.IsMovingUpSlope = true;

            if (rayDistance < SkinWidth + .0001f)
                break;
        }
    }

    private void HandleVerticalSlope(ref Vector2 deltaMovement) { }

    private bool HandleHorizontalSlope(ref Vector2 deltaMovement, float angle, bool isGoingRight) {
        return false;
    }


    //handles the situation where the ControllerParameters2D need to be overrided, in dif volumes
    public void OnTriggerEnter2D(Collider2D other) { }

    public void OnTriggerExit2D(Collider2D other) { }
}
