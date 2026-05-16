using UnityEngine;

public class CameraBob : MonoBehaviour
{
    public float frequency    = 12f;
    public float amplitude    = 0.09f;
    public float smoothReturn = 8f;

    Vector3 _origin;
    float   _t;
    float   _currentAmp;
    Vector3 _prevRootPos;

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

        bool grounded = Physics.Raycast(
            transform.root.position + Vector3.up * 0.2f, Vector3.down, 1.5f);
        bool moving = grounded && delta.magnitude / Time.deltaTime > 0.5f;

        _currentAmp = Mathf.Lerp(_currentAmp, moving ? amplitude : 0f, Time.deltaTime * smoothReturn);
        _t += Time.deltaTime * frequency;
        transform.localPosition = _origin + Vector3.up * (Mathf.Sin(_t) * _currentAmp);
    }
}
