using UnityEngine;
using UnityEngine.InputSystem;

public class ItemBob : MonoBehaviour
{
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private float resetRecoverTime = 0.25f;
    [SerializeField] private float bobScale = 0.1f;
    [SerializeField] private float bobSpeed = 3.0f;
    [SerializeField] private float bobCrouchSpeed;

    private bool _isCrouching = false;
    private Vector3 _restPosition;
    private float _currentSpeed;

    void Awake()
    {
        crouchAction.action.started += ctx => _isCrouching = true;
        crouchAction.action.canceled += ctx => _isCrouching = false;
    }
    
    void Start()
    {
        _restPosition = transform.localPosition;

        _currentSpeed = bobSpeed;
        bobCrouchSpeed = bobSpeed * 0.5f;
    }

    void Update()
    {
        _currentSpeed = _isCrouching ? bobCrouchSpeed : bobSpeed;
        
        if (lookAction.action.ReadValue<Vector2>().magnitude > Mathf.Epsilon)
        {
            float scale = 2 / (3 - Mathf.Cos(2 * Time.time));
            
            transform.localPosition = new Vector3(
                transform.localPosition.x + bobScale * (scale * Mathf.Cos(Time.time * _currentSpeed) * Time.deltaTime), 
                transform.localPosition.y + bobScale * (scale * Mathf.Sin(2 * Time.time * _currentSpeed) / 2 * Time.deltaTime), 
                transform.localPosition.z
                );
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _restPosition, Time.deltaTime * resetRecoverTime);
        }
    }
}
