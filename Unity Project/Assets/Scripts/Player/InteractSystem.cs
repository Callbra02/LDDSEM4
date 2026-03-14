using UnityEngine;
using UnityEngine.InputSystem;

public class InteractSystem : MonoBehaviour
{
    public InputActionReference interactAction;
    public Vector3 raycastOffset = Vector3.zero;
    private IInteractable _currentInteractable;
    public Transform cameraTransform;
    public float maxInteractDistance = 2.0f;

    private void FixedUpdate()
    {
        HandleItemDetection();
    }
    
    private void Update()
    {
        HandleInteraction();
    }

    private void HandleItemDetection()
    {
        RaycastHit hit;

        // Sphere cast ahead of the center point of the camera + a given offset
        if (Physics.SphereCast(cameraTransform.position + raycastOffset, 0.5f, cameraTransform.forward, out hit, maxInteractDistance))
        {
            // Set current interactable to any interactable that we are currently in range of interacting with
            hit.transform.gameObject.TryGetComponent<IInteractable>(out var interactable);
            _currentInteractable = interactable;
        }
        else
        {
            _currentInteractable = null;
        }
        

        // Button press for interact
        UIManager.Instance.showButtonToggle = _currentInteractable != null;

    }

    // If we interact this frame and theres an interactable, call it's Interact() func
    private void HandleInteraction()
    {
        if (interactAction.action.WasPressedThisFrame() && _currentInteractable != null)
        {
            _currentInteractable.Interact();
        }
    }
}
