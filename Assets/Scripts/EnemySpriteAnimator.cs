using UnityEngine;

public class EnemySpriteAnimator : MonoBehaviour
{
    public enum State { Idle, Walking, Dead }

    Sprite[] _walkFrames;
    Sprite[] _deathFrames;
    SpriteRenderer _sr;
    Camera _cam;

    State _state = State.Idle;
    int   _frameIndex;
    float _timer;
    float _walkFPS  = 8f;
    float _deathFPS = 6f;

    public void Init(Sprite[] walkFrames, Sprite[] deathFrames)
    {
        _walkFrames  = walkFrames;
        _deathFrames = deathFrames;

        _sr = gameObject.AddComponent<SpriteRenderer>();
        _sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _cam = Camera.main;

        if (_walkFrames.Length > 0)
            _sr.sprite = _walkFrames[0];
    }

    void LateUpdate()
    {
        if (_cam != null)
            transform.rotation = _cam.transform.rotation;

        if (_walkFrames == null || _walkFrames.Length == 0) return;
        if (_state == State.Idle) return;

        float fps = _state == State.Dead ? _deathFPS : _walkFPS;
        _timer += Time.deltaTime;
        if (_timer < 1f / fps) return;
        _timer -= 1f / fps;

        if (_state == State.Walking)
        {
            _frameIndex = (_frameIndex + 1) % _walkFrames.Length;
            _sr.sprite  = _walkFrames[_frameIndex];
        }
        else if (_state == State.Dead)
        {
            _frameIndex++;
            if (_frameIndex >= _deathFrames.Length)
            {
                Destroy(transform.parent.gameObject);
                return;
            }
            _sr.sprite = _deathFrames[_frameIndex];
        }
    }

    public void SetState(State state)
    {
        if (_state == State.Dead || _state == state) return;
        _state      = state;
        _timer      = 0f;
        _frameIndex = 0;
        if (_walkFrames != null && _walkFrames.Length > 0)
            _sr.sprite = _walkFrames[0];
    }

    public void PlayDeath()
    {
        if (_state == State.Dead) return;
        _state      = State.Dead;
        _timer      = 0f;
        _frameIndex = 0;
        if (_deathFrames != null && _deathFrames.Length > 0)
            _sr.sprite = _deathFrames[0];
    }
}
