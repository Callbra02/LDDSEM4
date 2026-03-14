using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bCharacter playerCharacter;
    
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
    
}
