using System.Diagnostics;
using UnityEngine;

public class BController
{
    [HideInInspector] public Transform playerTransform;
    private IBController _bController;
    private MovementConfig _config;
    private float _deltaTime;

    public bool jumping = false;
    public bool crouching = false;
    public float speed = 0.0f;

    public Transform camera;
    public float cameraYPos = 0.0f;

    private bool uncrouchDown = false;
    private float crouchLerp = 0.0f;
    private float frictionMulti = 1.0f;

    private Vector3 groundNormal = Vector3.up;

    public void ProcessMovement(IBController bController, MovementConfig config, float deltaTime)
    {
        _bController = bController;
        _config = config;
        _deltaTime = deltaTime;

        if (_bController.moveData.velocity.y <= 0f)
            jumping = false;

        if (_bController.groundObject == null)
        {
            _bController.moveData.velocity.y -= (_bController.moveData.gravityFactor * _config.gravity * _deltaTime);
            _bController.moveData.velocity.y += _bController.baseVelocity.y * _deltaTime;
        }

        CheckGrounded();
        CalculateMovementVelocity();

        float yVel = _bController.moveData.velocity.y;
        _bController.moveData.velocity.y = 0f;
        _bController.moveData.velocity = Vector3.ClampMagnitude(_bController.moveData.velocity, _config.maxVelocity);
        speed = _bController.moveData.velocity.magnitude;
        _bController.moveData.velocity.y = yVel;

        if (_bController.moveData.velocity.sqrMagnitude == 0.0f)
        {
            BPhysics.ResolveCollisions(_bController.collider, ref _bController.moveData.origin, ref _bController.moveData.velocity, _bController.moveData.rigidbodyPushForce, 1.0f, _bController.moveData.stepOffset, _bController);
        }
        else
        {
            float maxDistPerFrame = 0.2f;
            Vector3 velocityThisFrame = _bController.moveData.velocity * _deltaTime;
            float velocityDistLeft = velocityThisFrame.magnitude;
            float initialVel = velocityDistLeft;
            while (velocityDistLeft > 0.0f)
            {
                float amountThisLoop = Mathf.Min(maxDistPerFrame, velocityDistLeft);
                velocityDistLeft -= amountThisLoop;

                Vector3 velThisLoop = velocityThisFrame * (amountThisLoop / initialVel);
                _bController.moveData.origin += velThisLoop;
                
                BPhysics.ResolveCollisions(_bController.collider, ref _bController.moveData.origin, ref _bController.moveData.velocity, _bController.moveData.rigidbodyPushForce, amountThisLoop / initialVel, _bController.moveData.stepOffset, _bController);
            }
        }

        _bController.moveData.groundedTemp = _bController.moveData.grounded;
        _bController = null;
    }

    public void CalculateMovementVelocity()
    {
        switch (_bController.moveType)
        {
            case MoveType.Walk:
                if (_bController.groundObject == null)
                {
                    _bController.moveData.velocity += AirInputMovement();
                    BPhysics.Reflect(ref _bController.moveData.velocity, _bController.collider,
                        _bController.moveData.origin, _deltaTime);
                }
                else
                {
                    float fric = crouching ? _config.crouchFriction : _config.friction;
                    float accel = crouching ? _config.crouchAcceleration : _config.acceleration;
                    float decel = crouching ? _config.crouchDeceleration : _config.deceleration;

                    Vector3 forward = Vector3.Cross(groundNormal, -playerTransform.right);
                    Vector3 right = Vector3.Cross(groundNormal, forward);

                    float speed = _bController.moveData.sprinting ? _config.sprintSpeed : _config.walkSpeed;
                    if (crouching)
                        speed = _config.crouchSpeed;

                    Vector3 _wishDir;

                    if (_bController.moveData.wishJump)
                    {
                        ApplyFriction(0.0f, true, true);
                        Jump();
                        return;
                    }
                    else
                    {
                        ApplyFriction(1.0f * frictionMulti, true, true);
                    }

                    float forwardMove = _bController.moveData.verticalAxis;
                    float rightMove = _bController.moveData.horizontalAxis;

                    _wishDir = forwardMove * forward + rightMove * right;
                    _wishDir.Normalize();
                    Vector3 moveDirNorm = _wishDir;

                    Vector3 forwardVel = Vector3.Cross(groundNormal,
                        Quaternion.AngleAxis(-90, Vector3.up) * new Vector3(_bController.moveData.velocity.x, 0.0f,
                            _bController.moveData.velocity.z));

                    float _wishSpeed = _wishDir.magnitude;
                    _wishSpeed *= speed;

                    float yVel = _bController.moveData.velocity.y;
                    Accelerate(_wishDir, _wishSpeed, accel * Mathf.Min(frictionMulti, 1.0f), false);

                    float maxVelocityMagnitude = _config.maxVelocity;
                    _bController.moveData.velocity = Vector3.ClampMagnitude(
                        new Vector3(_bController.moveData.velocity.x, 0.0f, _bController.moveData.velocity.z),
                        maxVelocityMagnitude);
                    _bController.moveData.velocity.y = yVel;

                    float yVelocityNew = forwardVel.normalized.y * new Vector3(_bController.moveData.velocity.x,
                        0f, _bController.moveData.velocity.z).magnitude;

                    _bController.moveData.velocity.y = yVelocityNew * (_wishDir.y < 0f ? 1.2f : 1.0f);
                    float removableYVelocity = _bController.moveData.velocity.y - yVelocityNew;
                    
                }

                break;
        }
    }

    private void Accelerate(Vector3 wishDir, float wishSpeed, float acceleration, bool yMovement)
    {
        float addSpeed;
        float accelerationSpeed;
        float currentSpeed;

        currentSpeed = Vector3.Dot(_bController.moveData.velocity, wishDir);
        addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0)
            return;

        accelerationSpeed = Mathf.Min(acceleration * _deltaTime * wishSpeed, addSpeed);

        _bController.moveData.velocity.x += accelerationSpeed * wishDir.x;
        if (yMovement)
        {
            _bController.moveData.velocity.y += accelerationSpeed * wishDir.y;
        }

        _bController.moveData.velocity.z += accelerationSpeed * wishDir.z;
    }

    private void ApplyFriction(float t, bool yAffected, bool grounded)
    {
        Vector3 vel = _bController.moveData.velocity;
        float speed, newSpeed, control, drop;

        vel.y = 0.0f;
        speed = vel.magnitude;
        drop = 0.0f;

        float fric = crouching ? _config.crouchFriction : _config.friction;
        float accel = crouching ? _config.crouchAcceleration : _config.acceleration;
        float decel = crouching ? _config.crouchDeceleration : _config.deceleration;

        if (grounded)
        {
            vel.y = _bController.moveData.velocity.y;
            control = speed < decel ? decel : speed;
            drop = control * fric * _deltaTime * t;
        }

        newSpeed = Mathf.Max(speed - drop, 0.0f);
        if (speed > 0.0f)
            newSpeed /= speed;

        _bController.moveData.velocity.x *= newSpeed;
        if (yAffected == true)
        {
            _bController.moveData.velocity.y *= newSpeed;
        }

        _bController.moveData.velocity.z *= newSpeed;
    }

    private Vector3 AirInputMovement()
    {
        Vector3 wishVel, wishDir;
        float wishSpeed;
        
        GetWishValues(out wishVel, out wishDir, out wishSpeed);

        if (_config.clampAirSpeed && (wishSpeed != 0.0f && (wishSpeed > _config.maxSpeed)))
        {
            wishVel = wishVel * (_config.maxSpeed / wishSpeed);
            wishSpeed = _config.maxSpeed;
        }

        return BPhysics.AirAccelerate(_bController.moveData.velocity, wishDir, wishSpeed, _config.airAcceleration,
            _config.airCap, _deltaTime);
    }

    private void GetWishValues(out Vector3 wishVel, out Vector3 wishDir, out float wishSpeed)
    {
        wishVel = Vector3.zero;
        wishDir = Vector3.zero;
        wishSpeed = 0.0f;

        Vector3 forward = _bController.forward, right = _bController.right;

        forward[1] = 0;
        right[1] = 0;
        forward.Normalize();
        right.Normalize();

        for (int i = 0; i < 3; i++)
        {
            wishVel[i] = forward[i] * _bController.moveData.forwardMove + right[i] * _bController.moveData.sideMove;
        }

        wishVel[1] = 0;

        wishSpeed = wishVel.magnitude;
        wishDir = wishVel.normalized;
    }

    private void Jump()
    {
        if (!_config.autoBunnyhop)
            _bController.moveData.wishJump = false;

        _bController.moveData.velocity.y += _config.jumpForce;
        jumping = true;
    }

    private bool CheckGrounded()
    {
        _bController.moveData.surfaceFriction = 1.0f;
        var movingUp = _bController.moveData.velocity.y > 0.0f;
        var trace = TraceToFloor();

        float groundSteepness = Vector3.Angle(Vector3.up, trace.planeNormal);

        if (trace.hitCollider == null || groundSteepness > _config.slopeLimit ||
            (jumping && _bController.moveData.velocity.y > 0.0f))
        {
            SetGround(null);
            if (movingUp && _bController.moveType != MoveType.Noclip)
                _bController.moveData.surfaceFriction = _config.airFriction;
            return false;
        }
        else
        {
            groundNormal = trace.planeNormal;
            SetGround(trace.hitCollider.gameObject);
            return true;
        }
    }

    private void SetGround(GameObject obj)
    {
        if (obj != null)
        {
            _bController.groundObject = obj;
            _bController.moveData.velocity.y = 0;
        }
        else
        {
            _bController.groundObject = null;
        }
    }

    private Trace TraceBounds(Vector3 start, Vector3 end, int layerMask)
    {
        return Tracer.TraceCollider(_bController.collider, start, end, layerMask);
    }

    private Trace TraceToFloor()
    {
        var down = _bController.moveData.origin;
        down.y -= 0.15f;
        return Tracer.TraceCollider(_bController.collider, _bController.moveData.origin, down,
            BPhysics.groundLayerMask);
    }
    
    public void Crouch(IBController bController, MovementConfig config, float deltaTime)
    {
        _bController = bController;
        _config = config;
        _deltaTime = deltaTime;

        if (_bController == null)
            return;
        if (_bController.collider == null)
            return;
        
        //------------------------------------------------------------------

        bool grounded = _bController.groundObject != null;
        bool wantsToCrouch = _bController.moveData.crouching;

        float crouchingHeight = Mathf.Clamp(_bController.moveData.crouchingHeight, 0.01f, 1.0f);
        float heightDifference = _bController.moveData.defaultHeight - _bController.moveData.defaultHeight * crouchingHeight;

        if (grounded)
            uncrouchDown = false;

        if (grounded)
            crouchLerp = Mathf.Lerp(crouchLerp, wantsToCrouch ? 1.0f : 0.0f, _deltaTime * _bController.moveData.crouchingSpeed);
        else if (!grounded && !wantsToCrouch && crouchLerp < 0.95f)
            crouchLerp = 0.0f;
        else if (!grounded && wantsToCrouch)
            crouchLerp = 1.0f;

        if (crouchLerp > 0.9f && !crouching)
        {
            crouching = true;
            if (_bController.collider.GetType() == typeof(BoxCollider))
            {
                BoxCollider boxCollider = (BoxCollider)_bController.collider;
                boxCollider.size = new Vector3 (boxCollider.size.x, _bController.moveData.defaultHeight * crouchingHeight, boxCollider.size.z);
                
            }
            
            /*
            else if (_bController.collider.GetType() == typeof(CapsuleCollider))
            {
                CapsuleCollider capsuleCollider = (CapsuleCollider)_bController.collider;
                capsuleCollider.height = _bController.moveData.defaultHeight  * crouchingHeight;
            }
            */
            
            _bController.moveData.origin += heightDifference / 2 * (grounded ? Vector3.down : Vector3.up);
            foreach (Transform child in playerTransform)
            {
                if (child == _bController.moveData.viewTransform)
                    continue;
                
                child.localPosition = new Vector3 (child.localPosition.x , child.localPosition.y * crouchingHeight, child.localPosition.z);
            }

            uncrouchDown = !grounded;
        }
        else if (crouching)
        {
            bool canUncrouch = true;
            if (_bController.collider.GetType() == typeof(BoxCollider))
            {
                BoxCollider boxCollider = (BoxCollider)_bController.collider;
                Vector3 halfExtents = boxCollider.size * 0.5f;
                Vector3 startPos = boxCollider.transform.position;
                Vector3 endPos = boxCollider.transform.position +
                                 (uncrouchDown ? Vector3.down : Vector3.up) * heightDifference;

                Trace trace = Tracer.TraceBox(startPos, endPos, halfExtents, boxCollider.contactOffset,
                    BPhysics.groundLayerMask);
                
                if (trace.hitCollider != null)
                    canUncrouch = false;
            }
            /*
            else if (_bController.collider.GetType() == typeof(CapsuleCollider).collider)
            */

            if (canUncrouch && crouchLerp <= 0.9f)
            {
                crouching = false;
                if (_bController.collider.GetType() == typeof(BoxCollider))
                {
                    BoxCollider boxCollider = (BoxCollider)_bController.collider;
                    boxCollider.size = new Vector3 (boxCollider.size.x, _bController.moveData.defaultHeight, boxCollider.size.z);
                }
                
                _bController.moveData.origin += heightDifference / 2 * (uncrouchDown ? Vector3.down : Vector3.up);
                foreach (Transform child in playerTransform)
                {
                    child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y / crouchingHeight, child.localPosition.z);
                }
                    
            }

            if (!canUncrouch)
                crouchLerp = 1.0f;

        }

        if (!crouching)
            _bController.moveData.viewTransform.localPosition = Vector3.Lerp(
                _bController.moveData.viewTransformDefaultLocalPosition,
                _bController.moveData.viewTransformDefaultLocalPosition * crouchingHeight +
                Vector3.down * heightDifference * 0.5f, crouchLerp);
        else
            _bController.moveData.viewTransform.localPosition = Vector3.Lerp(
                _bController.moveData.viewTransformDefaultLocalPosition - Vector3.down * heightDifference * 0.5f,
                _bController.moveData.viewTransformDefaultLocalPosition * crouchingHeight, crouchLerp);



    }
}
