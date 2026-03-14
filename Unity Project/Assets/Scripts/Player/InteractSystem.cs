using UnityEngine;
using UnityEngine.InputSystem;

public class InteractSystem : MonoBehaviour
{
    public InputActionReference interactAction;
    public Vector3 raycastOffset = Vector3.zero;
    private IInteractable _currentInteractable;
    private Transform _cameraTransform;
    public float maxInteractDistance = 2.0f;

    private void Start()
    {
        _cameraTransform = this.transform.GetChild(0).transform.GetChild(0);
    }
    
    private void Update()
    {
        HandleItemDetection();
        HandleInteraction();
    }

    private void HandleItemDetection()
    {
        RaycastHit hit;

        if (Physics.SphereCast(_cameraTransform.position + raycastOffset, 0.5f, _cameraTransform.forward, out hit, maxInteractDistance))
        {
            _currentInteractable = hit.transform.gameObject.TryGetComponent<IInteractable>(out var interactable)
                ? interactable
                : null;
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
