using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")] 
    public Transform playerTransform;
    
    [Header("Input System")]
    public InputActionReference mouseInput;
    
    [Header("Sensitivity Settings")]
    public float sensitivityMultiplier = 1.0f;
    public float horizontalSensitivity = 1.0f;
    public float verticalSensitivity = 1.0f;
    
    [Header("Settings")]
    public float minXAngle = -88.0f;
    public float maxXAngle = 88.0f;
    public bool invertYInput = false;

    private Vector3 _rotation;

    // Get reference and lock cursor
    private void Start()
    {
        playerTransform = this.transform.parent;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Break from logic if game is paused
        if (Mathf.Abs(Time.timeScale) <= 0)
        {
            return;
        }

        // Get Mouse input
        Vector2 mouseMovement = mouseInput.action.ReadValue<Vector2>();

        // Invert Y if toggled
        if (!invertYInput)
        {
            mouseMovement.y = -mouseMovement.y;
        }
        
        // Set rotation vector
        _rotation = new Vector3(Mathf.Clamp(_rotation.x + mouseMovement.y, minXAngle, maxXAngle),
            _rotation.y + mouseMovement.x, _rotation.z);
        _rotation.z = Mathf.Lerp(_rotation.z, 0.0f, Time.deltaTime * 4.0f);

        // Apply rotation vector to playerTransform y
        playerTransform.eulerAngles = Vector3.Scale(_rotation, new Vector3(0.0f, 1.0f, 0.0f));

        // Apply rotation to camera
        transform.eulerAngles = _rotation;
    }
}
