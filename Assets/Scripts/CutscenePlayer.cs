using System.Collections;
using UnityEngine;

public class CutscenePlayer : MonoBehaviour
{
    public static CutscenePlayer Instance { get; private set; }
    public static bool IsPlaying { get; private set; }

    static int _sceneIndex = 0;

    GifDecoder.GifFrame[] _frames;
    int   _frameIdx;
    float _frameTimer;

    System.Action _onComplete;
    bool          _goodEnding;

    FPSController _fps;
    MouseLook     _mouseLook;

    GUIStyle _hintStyle;

    void Awake() => Instance = this;

    string ScenePath()
    {
        string root = Application.dataPath + "/cutscenes/";
        return _sceneIndex switch
        {
            0 => root + "escenauno.gif",
            1 => root + "escenados.gif",
            2 => root + "escenatres.gif",
            _ => root + "endings/" + (_goodEnding ? "goodending.gif" : "badending.gif"),
        };
    }

    public void Play(bool goodEnding, System.Action onComplete)
    {
        _onComplete = onComplete;
        _goodEnding = goodEnding;
    
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _fps       = player.GetComponent<FPSController>();
            _mouseLook = player.GetComponentInChildren<MouseLook>();
            if (_fps      != null) _fps.enabled      = false;
            if (_mouseLook != null) _mouseLook.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        AudioManager.Instance.PlayMusic(AudioManager.MusicTrack.Cinematic);
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        IsPlaying = true;

        // Decode GIF on first frame of coroutine (avoids blocking before yield)
        _frames   = GifDecoder.Load(ScenePath());
        _frameIdx = 0;

        if (_frames.Length == 0)
        {
            // Nothing to show — skip after a moment
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // Play through every frame at its native delay
            while (_frameIdx < _frames.Length)
            {
                float elapsed = 0f;
                float dur     = _frames[_frameIdx].delay;
                while (elapsed < dur) { elapsed += Time.unscaledDeltaTime; yield return null; }
                _frameIdx++;
            }
        }

        IsPlaying = false;
        _sceneIndex++;
        Finish();
    }

    void Finish()
    {
        // Destroy decoded textures
        if (_frames != null)
            foreach (var f in _frames)
                if (f.tex != null) Destroy(f.tex);
        _frames = null;

        if (_fps      != null) _fps.enabled      = true;
        if (_mouseLook != null) _mouseLook.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        AudioManager.Instance.StopMusic(0.05f);
        

        if (TransitionOverlay.Instance != null)
            TransitionOverlay.Instance.Play(_onComplete);
        else
            _onComplete?.Invoke();

        
    }

    void OnGUI()
    {
        if (!IsPlaying || _frames == null || _frameIdx >= _frames.Length) return;

        var tex = _frames[_frameIdx].tex;
        if (tex == null) return;

        int prev  = GUI.depth;
        GUI.depth = -998;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
                        tex, ScaleMode.StretchToFill, true);

        if (_hintStyle == null)
        {
            _hintStyle = new GUIStyle(GUI.skin.label)
            {
                font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                fontSize  = 12,
                alignment = TextAnchor.MiddleCenter,
            };
            _hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.4f);
        }
        GUI.Label(new Rect(0, Screen.height - 28f, Screen.width, 22f), "...", _hintStyle);
        GUI.depth = prev;
    }
}
