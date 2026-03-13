using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class bCharacter : MonoBehaviour, IBController
{
    public enum ColliderType
    {
        Capsule,
        Box
    }

    [Header("Physics")] 
    public Vector3 colliderSize = new Vector3(1.0f, 2.0f, 1.0f);
    [HideInInspector] public ColliderType collisionType
    {
        get { return ColliderType.Box; }
    }

    public float weight = 75.0f;
    public float rigidbodyPushForce = 2.0f;
    public bool solidCollider = false;

    [Header("View Settings")]
    public Transform viewTransform;
    public Transform playerRotationTransform;

    [Header("Crouch")]
    public float crouchingHeightMultiplier = 0.5f;
    public float crouchingSpeed = 10.0f;
    private float defaultHeight;
    private bool allowCrouch = true;

    [Header("Features")]
    public bool enableCrouching = true;
    public bool enableSliding = false;
    public bool enableLadders = false;

    [Header("Movement Config")] [SerializeField]
    public MovementConfig movementConfig;

    private GameObject _groundObject;
    private Vector3 _baseVelocity;
    private Collider _collider;
    private Vector3 _angles;
    private Vector3 _startPosition;
    private GameObject _colliderObject;
    
    private MoveData _moveData = new MoveData();
    private BController _controller = new BController();

    private Rigidbody rb;
    
    private List<Collider> triggers = new List<Collider>();
    private int numberOfTriggers = 0;
    
    // Properties
    public MoveType moveType { get { return MoveType.Walk; } }
    public MovementConfig moveConfig { get { return movementConfig; } }
    public MoveData moveData { get { return _moveData; } }
    public new Collider collider { get {return _collider; } }

    public GameObject groundObject
    {
        get { return _groundObject; }
        set { _groundObject = value; }
    }
    
    public Vector3 baseVelocity { get { return _baseVelocity; } }
    public Vector3 forward { get { return viewTransform.forward; } }
    public Vector3 right { get { return viewTransform.right; } }
    public Vector3 up { get { return viewTransform.up; } }

    private Vector3 previousPosition;

    public InputActionReference moveAction;
    public InputActionReference sprintAction;
    public InputActionReference crouchAction;
    public InputActionReference jumpAction;
    private bool isJumping = false;
    private bool isCrouching = false;
    private bool isSprinting = false;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, colliderSize);
    }

    private void Awake()
    {
        _controller.playerTransform = playerRotationTransform;

        if (viewTransform != null)
        {
            _controller.camera = viewTransform;
            _controller.cameraYPos = viewTransform.localPosition.y;
        }
    }

    private void Start()
    {
        sprintAction.action.started += ctx => isSprinting = true;
        sprintAction.action.canceled += ctx => isSprinting = false;

        crouchAction.action.started += ctx => isCrouching = true;
        crouchAction.action.canceled += ctx => isCrouching = false;
        
        jumpAction.action.started += ctx => isJumping = true;
        jumpAction.action.canceled += ctx => isJumping = false;
        
        _colliderObject = new GameObject("PlayerCollider");
        _colliderObject.layer = gameObject.layer;
        _colliderObject.transform.SetParent(transform);
        _colliderObject.transform.rotation = Quaternion.identity;
        _colliderObject.transform.localPosition = Vector3.zero;
        _colliderObject.transform.SetSiblingIndex(0);

        previousPosition = transform.position;

        if (viewTransform == null)
        {
            viewTransform = Camera.main.transform;
        }

        if (playerRotationTransform == null && transform.childCount > 0)
        {
            playerRotationTransform = transform.GetChild(0);
        }
        
        _collider = gameObject.GetComponent<Collider>();

        if (_collider != null)
        {
            GameObject.Destroy(_collider);
        }
        
        rb = gameObject.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        allowCrouch = enableCrouching;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.angularDamping = 0.0f;
        rb.linearDamping = 0.0f;
        rb.mass = weight;

        switch (collisionType)
        {
            case ColliderType.Box :
                _collider = _colliderObject.AddComponent<BoxCollider>();
                var boxc = (BoxCollider)_collider;
                boxc.size = colliderSize;
                defaultHeight = boxc.size.y;
                break;
            
            case ColliderType.Capsule:
                break;
            
        }

        _moveData.slopeLimit = movementConfig.slopeLimit;
        _moveData.rigidbodyPushForce = rigidbodyPushForce;
        _moveData.playerTransform = transform;
        _moveData.viewTransform = viewTransform;
        _moveData.viewTransformDefaultLocalPosition = viewTransform.localPosition;
        
        _moveData.defaultHeight = defaultHeight;
        _moveData.crouchingHeight = crouchingHeightMultiplier;
        _moveData.crouchingSpeed = crouchingSpeed;

        _collider.isTrigger = !solidCollider;
        _moveData.origin = transform.position;
        _startPosition = transform.position;
    }
    
    private void Update()
    {
        _colliderObject.transform.rotation = Quaternion.identity;

        UpdateMoveData();

        Vector3 positionalMovement = transform.position - previousPosition;
        transform.position = previousPosition;
        moveData.origin += positionalMovement;

        if (numberOfTriggers != triggers.Count)
        {
            numberOfTriggers = triggers.Count;
            
            triggers.RemoveAll(item => item == null);
            foreach (Collider trigger in triggers)
            {
                if (trigger == null)
                    continue;
            }
        }

        if (allowCrouch)
        {
            _controller.Crouch(this, movementConfig, Time.deltaTime);
        }
        _controller.ProcessMovement(this, movementConfig, Time.deltaTime);
        transform.position = moveData.origin;
        previousPosition = transform.position;
        _colliderObject.transform.rotation = Quaternion.identity;
    }

    private void UpdateMoveData()
    {
        _moveData.verticalAxis = moveAction.action.ReadValue<Vector2>().y;
        _moveData.horizontalAxis = moveAction.action.ReadValue<Vector2>().x;

        _moveData.sprinting = isSprinting;
        _moveData.crouching = isCrouching;
        
        bool moveLeft = _moveData.horizontalAxis < 0;
        bool moveRight = _moveData.horizontalAxis > 0;
        bool moveForward = _moveData.verticalAxis > 0;
        bool moveBackwards = _moveData.verticalAxis < 0;
        bool jump = isJumping;

        if (!moveLeft && !moveRight)
            _moveData.sideMove = 0.0f;
        else if (moveLeft)
            _moveData.sideMove = -moveConfig.acceleration;
        else if (moveRight)
            _moveData.sideMove = moveConfig.acceleration;

        if (!moveForward && !moveBackwards)
            _moveData.forwardMove = 0.0f;
        else if (moveForward)
            _moveData.forwardMove = moveConfig.acceleration;
        else if (moveBackwards)
            _moveData.forwardMove = -moveConfig.acceleration;

        _moveData.wishJump = isJumping;

        _moveData.viewAngles = _angles;
    }

    private static float ClampAngle(float angle, float from, float to)
    {
        if (angle < 0f)
            angle = 360 + angle;

        if (angle > 180f)
            return Mathf.Max(angle, 360 + from);

        return Mathf.Min(angle, to);
    }
    
    private void OnCollisionStay(Collision collision)
    {
        if (collision.rigidbody == null)
            return;

        Vector3 relativeVel = collision.relativeVelocity * collision.rigidbody.mass / 50.0f;
        Vector3 impactVel = new Vector3 (relativeVel.x * 0.0025f, relativeVel.y * 0.0025f, relativeVel.z * 0.0025f);

        float maxYVel = Mathf.Max(moveData.velocity.y, 10.0f);
        Vector3 newVel = new Vector3(moveData.velocity.x + impactVel.x,
            Mathf.Clamp(moveData.velocity.y + Mathf.Clamp(impactVel.y, -0.5f, 0.5f), -maxYVel, maxYVel),
            moveData.velocity.z + impactVel.z);
        
        newVel = Vector3.ClampMagnitude(newVel, Mathf.Max(moveData.velocity.magnitude, 30.0f));
        moveData.velocity = newVel;
    }
}
