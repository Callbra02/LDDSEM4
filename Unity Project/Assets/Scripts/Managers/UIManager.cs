using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public bool showButtonToggle = false;
    public GameObject buttonTogglePanel;
    
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonTogglePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        buttonTogglePanel.SetActive(showButtonToggle);
    }
}
