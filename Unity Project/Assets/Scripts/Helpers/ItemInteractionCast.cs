using UnityEngine;
using UnityEngine.InputSystem;

public class ItemInteractionCast : MonoBehaviour
{

    public InputActionReference interactAction;

    private IInteractable _interactableInRange;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        
        Debug.DrawLine(this.transform.position, this.transform.position + this.transform.forward * 5.0f);
        if (Physics.SphereCast(this.transform.position, 0.5f, this.transform.forward, out hit, 5.0f))
        {
            _interactableInRange = hit.transform.gameObject.TryGetComponent<IInteractable>(out var interactable)
                ? interactable
                : null;

        }

        if (_interactableInRange != null)
        {
            UIManager.Instance.showButtonToggle = true;
        }
        else
        {
            UIManager.Instance.showButtonToggle = false;
        }
    }
}
