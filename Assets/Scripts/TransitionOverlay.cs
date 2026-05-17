using UnityEngine;

public class TransitionOverlay : MonoBehaviour
{
    public static TransitionOverlay Instance { get; private set; }

    const float BEAT = 60f / 180f; // 0.333s at 180 BPM

    Texture2D[] _frames = new Texture2D[3];
    int    _frame = -1;
    float  _timer;
    System.Action _onComplete;

    void Awake()
    {
        Instance = this;
#if UNITY_EDITOR
        _frames[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Menu/Transicion/1.png");
        _frames[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Menu/Transicion/2.png");
        _frames[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Menu/Transicion/3.png");
#endif
    }

    public void Play(System.Action onComplete)
    {
        _onComplete = onComplete;
        _frame      = 0;
        _timer      = BEAT;
    }

    void Update()
    {
        if (_frame < 0) return;

        _timer -= Time.unscaledDeltaTime;
        if (_timer > 0f) return;

        _frame++;
        if (_frame >= _frames.Length)
        {
            _frame = -1;
            _onComplete?.Invoke();
        }
        else
        {
            _timer = BEAT;
        }
    }

    void OnGUI()
    {
        if (_frame < 0 || _frame >= _frames.Length || _frames[_frame] == null) return;
        int prev = GUI.depth;
        GUI.depth = -999;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
                        _frames[_frame], ScaleMode.StretchToFill, true);
        GUI.depth = prev;
    }
}
