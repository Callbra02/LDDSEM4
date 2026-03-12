using UnityEngine;

public class MovementData
{
    public Transform playerTransform;
    public Transform cameraTransform;
    public Vector3 cameraTransformDefaultLocalPosition;

    public Vector3 origin;
    public Vector3 cameraAngles;
    public Vector3 velocity;
    public float forwardMovement;
    public float sideMovement;
    public float upMovement;
    public float surfaceFriction = 1.0f;
    public float gravityMultiplier = 1.0f;
    public float walkMultiplier = 1.0f;
    public float verticalAxis = 0.0f;
    public float horizontalAxis = 0.0f;
    public bool wishJump = false;
    public bool crouching = false;
    public bool sprinting = false;

    public float maxSlope = 45.0f;

    public float rigidbodyPushForce = 1.0f;

    public float defaultHeight = 2.0f;
    public float crouchingHeight = 1.0f;
    public float crouchingSpeed = 10.0f;
    public bool toggleCrouch = false;

    public bool isGrounded = false;
    public bool isGroundedTemp = false;
    public float fallingVelocity = 0.0f;

    public bool useStepOffset = false;
    public float stepOffset = 0.0f;
}
