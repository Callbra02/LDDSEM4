using UnityEngine;

public enum MoveType
{
    None,
    Walk,
    Noclip,
    Ladder,
}

public class MoveData
{
    public Transform playerTransform;
    public Transform viewTransform;
    public Vector3 viewTransformDefaultLocalPosition;

    public Vector3 origin;
    public Vector3 viewAngles;
    public Vector3 velocity;
    public float forwardMove;
    public float sideMove;
    public float upMove;
    public float surfaceFriction = 1.0f;
    public float gravityFactor = 1.0f;
    public float walkFactor = 1.0f;
    public float verticalAxis = 0.0f;
    public float horizontalAxis = 0.0f;
    public bool wishJump = false;
    public bool crouching = false;
    public bool sprinting = false;

    public float slopeLimit = 45.0f;
    
    public float rigidbodyPushForce = 1.0f;

    public float defaultHeight = 2.0f;
    public float crouchingHeight = 1.0f;
    public float crouchingSpeed = 10.0f;
    public bool toggleCrouch = false;

    public bool grounded = false;
    public bool groundedTemp = false;
    public float fallingVelocity = 0.0f;

    public bool useStepOffset = false;
    public float stepOffset = 0.0f;

}
