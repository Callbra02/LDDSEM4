using UnityEngine;
using UnityEngine.InputSystem;

public class InteractSystem : MonoBehaviour
{
    public InputActionReference interactAction;
    public Vector3 raycastOffset = Vector3.zero;
    private IInteractable _currentInteractable;
    public Transform cameraTransform;
    public float maxInteractDistance = 2.0f;

    private void Start()
    {
    }

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

        if (Physics.SphereCast(cameraTransform.position + raycastOffset, 0.5f, cameraTransform.forward, out hit, maxInteractDistance))
        {
            hit.transform.gameObject.TryGetComponent<IInteractable>(out var interactable);
            _currentInteractable = interactable;
        }
        else
        {
            _currentInteractable = null;
        }
        

        UIManager.Instance.showButtonToggle = _currentInteractable != null;

    }

    private void HandleInteraction()
    {
        if (interactAction.action.WasPressedThisFrame() && _currentInteractable != null)
        {
            _currentInteractable.Interact();
        }
    }
}
