using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    [Range(0.0f, 100.0f)]
    [SerializeField] private float flickerChance = 50.0f;

    [SerializeField] private float flickerIntensity = 0.5f;

    private Light _light;

    private bool _isFlickering = false;
    private bool _canFlicker = true;
    private float _flickerTimer = 0.0f;
    [SerializeField]  private float flickerTimerMax = 0.2f;

    private float defaultIntensity;
    void Start()
    {
        _light = GetComponent<Light>();
        defaultIntensity = _light.intensity;
    }
    
    void Update()
    {
        float randomFloat = Random.Range(0.0f, 100.0f);

        if (Time.time % 2 == 0)
        {
            return;
        }

        if (randomFloat < flickerChance && _canFlicker)
        {
            Flicker();
        }

        HandleFlicker();

    }

    void Flicker()
    {
        _isFlickering = true;
    }

    void HandleFlicker()
    {
        if (_isFlickering)
        {
            _canFlicker = false;
            TurnOffLight();
            
            _flickerTimer += Time.deltaTime;

            if (_flickerTimer > flickerTimerMax)
            {
                TurnOnLight();
                _flickerTimer = 0.0f;
                _canFlicker = true;
                _isFlickering = false;
            }
        }
    }

    void TurnOffLight()
    {
        _light.intensity = flickerIntensity;
    }

    void TurnOnLight()
    {
        _light.intensity = defaultIntensity;
    }
}
