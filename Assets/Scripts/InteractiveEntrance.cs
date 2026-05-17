using UnityEngine;
using UnityEngine.InputSystem;

public class InteractiveEntrance : MonoBehaviour
{
    [Header("Interaction")]
    public float interactRange = 3f;
    public string promptText = "Press [E] to enter";

    [Header("Teleport")]
    public Transform tpTarget;

    public static bool WasActivated { get; private set; }

    bool _showPrompt;
    Camera _cam;
    GUIStyle _style;

    void Start()
    {
        _cam = Camera.main;
        var renderer = GetComponent<Renderer>();
        if (renderer) renderer.enabled = false;
    }

    void Update()
    {
        _showPrompt = false;
        if (_cam == null) return;

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange) && hit.collider.gameObject == gameObject)
        {
            _showPrompt = true;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                OnEnter();
        }
    }

    bool _transitioning;

    void OnEnter()
    {
        AudioManager.Instance.StopMusic(0.05f);
        AudioManager.Instance.PlayMusic(AudioManager.MusicTrack.Level1);
        if (_transitioning) return;
        _transitioning = true;

        if (TransitionOverlay.Instance != null)
            TransitionOverlay.Instance.Play(DoEnter);
        else
            DoEnter();
    }

    void DoEnter()
    {
        _showPrompt = false;
        if (tpTarget == null) return;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = tpTarget.position;
        if (cc != null) cc.enabled = true;

        AudioManager.Instance.StopDiscoMusic(1.5f);
        AudioManager.Instance.PlayMusic(AudioManager.MusicTrack.Level1, 1.5f);

        // 80% taller, areas and weapons 30% bigger
        var fps = player.GetComponent<FPSController>();
        if (fps != null) fps.Resize(1.8f);

        WasActivated = true;
        foreach (var enemy in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
            enemy.Resize(1.8f, 1.3f);

        // Scale weapons 30% bigger (skip AI-held ones, already handled in enemy.Resize)
        foreach (var ws in Object.FindObjectsByType<WeaponStats>(FindObjectsSortMode.None))
            if (ws.GetComponentInParent<EnemyAI>() == null)
                ws.transform.localScale *= 1.3f;

        // Kill environmental lighting after entering
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
            if (l.type == LightType.Directional)
                l.enabled = false;
    }

    void OnGUI()
    {
        if (!_showPrompt) return;

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label);
            _style.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _style.fontSize  = 28;
            _style.fontStyle = FontStyle.Bold;
            _style.normal.textColor = new Color(1f, 1f, 1f, 0.55f);
            _style.alignment = TextAnchor.MiddleCenter;
        }

        float w = 300, h = 40;
        GUI.Label(new Rect((Screen.width - w) / 2f, Screen.height / 2f + 30, w, h), promptText, _style);
    }
}
