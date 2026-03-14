using UnityEngine;

public class Switch : MonoBehaviour, IInteractable
{
    private bool _toggleState = false;
    private Animator _anim;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _anim.SetBool("SwitchOn", _toggleState);
    }

    public void Interact()
    {
        _toggleState = !_toggleState;
    }
}
