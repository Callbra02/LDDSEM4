using UnityEngine;

public class BController
{
    [HideInInspector] public Transform playerTransform;
    private IBController _controller;
    private MovementConfig _config;
    private float _deltaTime;

    public bool jumping = false;
    public bool crouching = false;
    public float speed = 0.0f;

    public Transform camera;
    public float cameraYPos = 0.0f;

    private bool _uncrouchDown = false;
    private float _crouchLerp = 0.0f;
    private float _frictionMultiplier = 1.0f;

    private Vector3 _groundNormal = Vector3.up;

    public void HandleMovement(IBController controller, MovementConfig config, float deltaTime)
    {
        _controller = controller;
        _config = config;
        _deltaTime = deltaTime;

        if (_controller.movementData.velocity.y <= 0f)
        {
            jumping = false;
        }

        if (_controller.groundObject == null)
        {
            _controller.movementData.velocity.y -=
                (_controller.movementData.gravityMultiplier * _config.gravity * _deltaTime);
            _controller.movementData.velocity.y += _controller.baseVelocity.y * _deltaTime;
        }

        CheckGrounded();
        CalculateMovementVelocity();

        float yVelocity = _controller.movementData.velocity.y;

        _controller.movementData.velocity.y = 0.0f;
        _controller.movementData.velocity =
            Vector3.ClampMagnitude(_controller.movementData.velocity, _config.maxVelocity);
        speed = _controller.movementData.velocity.magnitude;
        _controller.movementData.velocity.y = yVelocity;

        if (_controller.movementData.velocity.sqrMagnitude == 0.0f)
        {
            BPhysics.ResolveCollisions(_controller.collider, ref _controller.movementData.origin,
                ref _controller.movementData.velocity, _controller.movementData.rigidbodyPushForce, 1.0f,
                _controller.movementData.stepOffset, _controller);
        }
        else
        {
            float maxDistancePerFrame = 0.2f;
            Vector3 velocityThisFrame = _controller.movementData.velocity * _deltaTime;
            float velocityDistanceLeft = velocityThisFrame.magnitude;
            float initialVelocity = velocityDistanceLeft;
            
            while (velocityDistanceLeft > 0.0f)
            {
                float loopAmount = Mathf.Min(maxDistancePerFrame, velocityDistanceLeft);

                velocityDistanceLeft -= loopAmount;

                Vector3 loopVelocity = velocityThisFrame * (loopAmount / initialVelocity);
                _controller.movementData.origin += loopVelocity;

                BPhysics.ResolveCollisions(_controller.collider, ref _controller.movementData.origin,
                    ref _controller.movementData.velocity, _controller.movementData.rigidbodyPushForce,
                    loopAmount / initialVelocity, _controller.movementData.stepOffset, _controller);
            }
        }

        _controller.movementData.isGroundedTemp = _controller.movementData.isGrounded;
        _controller = null;
    }

    private bool CheckGrounded()
    {
        _controller.movementData.surfaceFriction = 1.0f;
        bool movingUp = _controller.movementData.velocity.y > 0.0f;
        var trace = TraceToFloor();

        float groundSteepness = Vector3.Angle(Vector3.up, trace.planeNormal);

        if (trace.hitCollider == null || groundSteepness > _config.slopeLimit ||
            (jumping && _controller.movementData.velocity.y > 0.0f))
        {
            SetGround(null);
            if (movingUp)
            {
                _controller.movementData.surfaceFriction = _config.airFriction;
            }

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
            _controller.groundObject = obj;
            _controller.movementData.velocity.y = 0.0f;
        }
        else
        {
            _controller.groundObject = null;
        }
    }

    private void CalculateMovementVelocity()
    {
        
    }

    private Trace TraceToFloor()
    {
        var down = _controller.movementData.origin;
        down.y -= 0.15f;
        return Tracer.TraceCollider(_controller.collider, _controller.movementData.origin, down,
            BPhysics.groundLayerMask);
    }

    public void Crouch(IBController controller, MovementConfig config, float deltaTime)
    {
        _controller = controller;
        _config = config;
        _deltaTime = deltaTime;

        if (_controller == null || _controller.collider == null)
        {
            return;
        }

        bool grounded = _controller.groundObject != null;
        bool attemptingCrouch = _controller.movementData.crouching;

        float crouchingHeight = Mathf.Clamp(_controller.movementData.crouchingHeight, 0.01f, 1.0f);
        float heightDifference = _controller.movementData.defaultHeight -
                                 _controller.movementData.defaultHeight * crouchingHeight;

        if (grounded)
        {
            _uncrouchDown = false;
        }

        if (grounded)
        {
            _crouchLerp = Mathf.Lerp(_crouchLerp, attemptingCrouch ? 1.0f : 0.0f,
                _deltaTime * _controller.movementData.crouchingSpeed);
        }
        else if (!grounded && !attemptingCrouch && _crouchLerp < 0.95f)
        {
            _crouchLerp = 0.0f;
        }
        else if (!grounded && attemptingCrouch)
        {
            _crouchLerp = 1.0f;
        }
        
        if (_crouchLerp > 0.9f && !crouching)
    }
}
