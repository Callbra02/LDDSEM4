using UnityEngine;
using UnityEngine.InputSystem;

public class ItemBob : MonoBehaviour
{
    public InputActionReference lookAction;
    [SerializeField] private float resetRecoverTime = 0.25f;
    [SerializeField] private float bobScale = 0.1f;
    [SerializeField] private float bobSpeed = 3.0f;
    [SerializeField] private float bobCrouchSpeed;

    private bool _isCrouching = false;
    private Vector3 _restPosition;
    private float _currentSpeed;
    
    void Start()
    {
        _restPosition = transform.localPosition;
        _currentSpeed = bobSpeed;
        bobCrouchSpeed = bobSpeed * 0.5f;
    }

    void Update()
    {
        // Set speed depending on crouch
        _currentSpeed = _isCrouching ? bobCrouchSpeed : bobSpeed;
        _isCrouching = GameManager.Instance.playerCharacter.isCrouching;
        
        // If we are moving at all, do mathgic and bob in figure-8 pattern
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
            // Go back to rest position if not moving
            transform.localPosition = Vector3.Lerp(transform.localPosition, _restPosition, Time.deltaTime * resetRecoverTime);
        }
    }
}
