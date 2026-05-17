using UnityEngine;

public class CameraBob : MonoBehaviour
{
    public float frequency    = 12f;
    public float amplitude    = 0.06f;
    public float smoothReturn = 8f;

    Vector3 _origin;
    float   _t;
    float   _currentAmp;
    Vector3 _prevRootPos;

    string[] _stepSounds = { "Steps1", "Steps2", "Steps3" };
    bool _wasPositive = false; // para detectar el cruce por cero

    void Start()
    {
        _origin      = transform.localPosition;
        _prevRootPos = transform.root.position;
    }

    void Update()
    {
        Vector3 rootPos = transform.root.position;
        Vector3 delta   = rootPos - _prevRootPos;
        delta.y         = 0f;
        _prevRootPos    = rootPos;

        var cc         = transform.root.GetComponent<CharacterController>();
        var fps        = transform.root.GetComponent<FPSController>();
        bool grounded  = cc != null && cc.isGrounded;
        bool crouching = fps != null && fps.IsCrouching;
        bool moving    = grounded && !crouching && delta.magnitude / Time.deltaTime > 0.5f;

        _currentAmp = Mathf.Lerp(_currentAmp, moving ? amplitude : 0f, Time.deltaTime * smoothReturn);
        _t += Time.deltaTime * frequency;

        float sineValue = Mathf.Sin(_t);
        transform.localPosition = _origin + Vector3.up * (sineValue * _currentAmp);

        // Dispara el SFX cada vez que el seno cruza de negativo a positivo (= un paso)
        bool isPositive = sineValue > 0f;
        if (isPositive && !_wasPositive && moving && _currentAmp > amplitude * 0.5f)
        {
            string randomStep = _stepSounds[Random.Range(0, _stepSounds.Length)];
            AudioManager.Instance.PlaySFX(randomStep);
        }
        _wasPositive = isPositive;
    }
}