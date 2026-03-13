using UnityEngine;

[System.Serializable]
public class MovementConfig
{
    public bool autoBunnyhop = false;
    public float gravity = 20.0f;
    public float jumpForce = 6.5f;

    public float friction = 6.0f;
    public float maxSpeed = 6.0f;
    public float maxVelocity = 50.0f;
    public float slopeLimit = 45.0f;

    public bool clampAirSpeed = true;
    public float airCap = 0.4f;
    public float airAcceleration = 12.0f;
    public float airFriction = 0.4f;

    public float walkSpeed = 7.0f;
    public float sprintSpeed = 12.0f;
    public float acceleration = 14.0f;
    public float deceleration = 10.0f;

    public float crouchSpeed = 4.0f;
    public float crouchAcceleration = 8.0f;
    public float crouchDeceleration = 4.0f;
    public float crouchFriction = 3.0f;

}
