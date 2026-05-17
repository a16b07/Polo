using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    public static bool IsOpen { get; private set; } = true;

    bool  _configOpen       = false;
    bool  _inGameConfigOpen = false;
    float _gamma;
    float _rotationY;

    Texture2D _texTitle, _texBox, _texIniciar, _texConfig, _texGameOver;
    bool      _gameOverActive;

    Camera     _cam;
    Transform  _menuPOV;
    Transform  _camOrigParent;
    Vector3    _camOrigLocalPos;
    Quaternion _camOrigLocalRot;

    FPSController _fps;
    MouseLook     _mouseLook;
    Shooter       _shooter;
    GunPickup     _gunPickup;

    GUIStyle             _labelStyle;
    BuckshotFilterFeature _buckshotFeature;

    void Start()
    {
        _gamma   = PlayerPrefs.GetFloat("Gamma", 1f);
        _cam     = Camera.main;
        _menuPOV = GameObject.Find("Menu POV")?.transform;

        LoadTextures();
        CachePlayerComponents();
        CacheCamera();
        var features = Resources.FindObjectsOfTypeAll<BuckshotFilterFeature>();
        if (features.Length > 0) _buckshotFeature = features[0];
        ApplyGamma(_gamma);
        OpenMenu();
    }

    void LoadTextures()
    {
#if UNITY_EDITOR
        _texTitle    = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Menu/titulo.png");
        _texBox      = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Menu/poloBox.png");
        _texIniciar  = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Menu/boton_iniciar.png");
        _texConfig   = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Menu/boton_config.png");
        _texGameOver = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/text/gameOver.png");
#endif
    }

    void CachePlayerComponents()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        _fps       = player.GetComponent<FPSController>();
        _shooter   = player.GetComponent<Shooter>();
        _gunPickup = player.GetComponent<GunPickup>();
        _mouseLook = player.GetComponentInChildren<MouseLook>();
    }

    void CacheCamera()
    {
        if (_cam == null) return;
        _camOrigParent   = _cam.transform.parent;
        _camOrigLocalPos = _cam.transform.localPosition;
        _camOrigLocalRot = _cam.transform.localRotation;
    }

    void OpenMenu()
    {
        IsOpen = true;

        if (_fps       != null) _fps.enabled       = false;
        if (_mouseLook != null) _mouseLook.enabled  = false;
        if (_shooter   != null) _shooter.enabled   = false;
        if (_gunPickup != null) _gunPickup.enabled  = false;

        if (_cam != null && _menuPOV != null)
        {
            _cam.transform.SetParent(null, true);
            _cam.transform.position = _menuPOV.position;
            _cam.transform.rotation = _menuPOV.rotation;
            _rotationY = _menuPOV.eulerAngles.y;
        }
    }

    void StartGame()
    {
        IsOpen      = false;
        _configOpen = false;

        // Restore camera: reparent then set local transform explicitly
        if (_cam != null && _camOrigParent != null)
        {
            _cam.transform.SetParent(_camOrigParent, true);
            _cam.transform.localPosition = _camOrigLocalPos;
            _cam.transform.localRotation = _camOrigLocalRot;
        }

        if (_fps       != null) _fps.enabled       = true;
        if (_mouseLook != null) _mouseLook.enabled  = true;
        if (_shooter   != null) _shooter.enabled   = true;
        if (_gunPickup != null) _gunPickup.enabled  = true;

        ApplyGamma(_gamma); // ensure saved gamma is active in-game

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        if (!IsOpen) return;

        // Enforce free cursor every frame — beats any script execution order race
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Slow panoramic rotation
        if (_cam != null && _menuPOV != null)
        {
            _rotationY += 4f * Time.deltaTime;
            _cam.transform.rotation = Quaternion.Euler(
                _menuPOV.eulerAngles.x, _rotationY, 0f);
        }

        // Menu Esc toggles config panel
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            _configOpen = !_configOpen;
    }

    void ToggleInGameConfig()
    {
        _inGameConfigOpen = !_inGameConfigOpen;
        if (_inGameConfigOpen)
        {
            if (_mouseLook != null) _mouseLook.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
        else
        {
            if (_mouseLook != null) _mouseLook.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    void LateUpdate()
    {
        if (_gameOverActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                ReturnToMenu();
            return;
        }
        if (IsOpen) return; // main menu handles it in Update
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            ToggleInGameConfig();
        if (_inGameConfigOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }

    void OnGUI()
    {
        if (_gameOverActive)
        {
            if (_labelStyle == null) BuildStyles();
            int prev = GUI.depth; GUI.depth = -997;
            if (_texGameOver != null)
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
                                _texGameOver, ScaleMode.StretchToFill, true);
            var hint = new GUIStyle(_labelStyle) { fontSize = 13 };
            hint.normal.textColor = new Color(1f, 1f, 1f, 0.55f);
            GUI.Label(new Rect(0, Screen.height - 32f, Screen.width, 24f),
                      "Press any key to return", hint);
            GUI.depth = prev;
            return;
        }

        if (!IsOpen && _inGameConfigOpen)
        {
            if (_labelStyle == null) BuildStyles();
            DrawConfig(Screen.width, Screen.height);
            return;
        }
        if (!IsOpen) return;
        if (_labelStyle == null) BuildStyles();

        float sw = Screen.width, sh = Screen.height;

        if (_configOpen) DrawConfig(sw, sh);
        else             DrawMainMenu(sw, sh);
    }

    void DrawMainMenu(float sw, float sh)
    {
        // Title
        if (_texTitle != null)
        {
            float tw = Mathf.Min(500f, sw * 0.52f);
            float th = tw * ((float)_texTitle.height / _texTitle.width);
            GUI.DrawTexture(new Rect((sw - tw) / 2f, sh * 0.08f, tw, th),
                            _texTitle, ScaleMode.ScaleToFit, true);
        }

        // boton_iniciar — no background, standalone centered button
        if (_texIniciar != null)
        {
            float iw = 280f;
            float ih = iw * ((float)_texIniciar.height / _texIniciar.width);
            float ix = (sw - iw) / 2f;
            float iy = sh * 0.50f;
            GUI.DrawTexture(new Rect(ix, iy, iw, ih), _texIniciar, ScaleMode.ScaleToFit, true);
            if (GUI.Button(new Rect(ix, iy, iw, ih), GUIContent.none, GUIStyle.none))
                StartGame();
        }

        // Config button — bottom left
        if (_texConfig != null)
        {
            float cw = 70f;
            float ch = cw * ((float)_texConfig.height / _texConfig.width);
            var   cr = new Rect(18f, sh - ch - 18f, cw, ch);
            GUI.DrawTexture(cr, _texConfig, ScaleMode.ScaleToFit, true);
            if (GUI.Button(cr, GUIContent.none, GUIStyle.none))
                _configOpen = true;
        }
    }

    void DrawConfig(float sw, float sh)
    {
        float pw = 520f, ph = 210f;
        float px = (sw - pw) / 2f;
        float py = (sh - ph) / 2f;

        if (_texBox != null)
            GUI.DrawTexture(new Rect(px, py, pw, ph), _texBox, ScaleMode.StretchToFill, true);

        GUI.Label(new Rect(px, py + 14f, pw, 28f), "CONFIG", _labelStyle);

        // Gamma row — wide slider
        float ry  = py + 88f;
        float lw  = 80f;
        float val = 44f;
        float sliderW = pw - lw - val - 48f;    // ~348px
        float lx  = px + 16f;
        float sx  = lx + lw + 8f;

        var left = new GUIStyle(_labelStyle) { alignment = TextAnchor.MiddleLeft };
        GUI.Label(new Rect(lx, ry, lw, 26f), "Gamma", left);
        float ng = GUI.HorizontalSlider(new Rect(sx, ry + 7f, sliderW, 14f), _gamma, 0.5f, 3.0f);
        GUI.Label(new Rect(sx + sliderW + 6f, ry, val, 26f), $"{ng:F2}", left);

        if (Mathf.Abs(ng - _gamma) > 0.001f)
        {
            _gamma = ng;
            PlayerPrefs.SetFloat("Gamma", _gamma);
            PlayerPrefs.Save();
            ApplyGamma(_gamma);
        }

        var hint = new GUIStyle(_labelStyle) { fontSize = 11 };
        hint.normal.textColor = new Color(0.75f, 0.75f, 0.75f);
        GUI.Label(new Rect(px, py + ph - 26f, pw, 20f), "[ESC] close", hint);
    }

    void SetPlayerEnabled(bool on)
    {
        if (_fps       != null) _fps.enabled       = on;
        if (_mouseLook != null) _mouseLook.enabled  = on;
        if (_shooter   != null) _shooter.enabled   = on;
        if (_gunPickup != null) _gunPickup.enabled  = on;
    }

    // ── Game Over ──────────────────────────────────────────────────────────────
    public void TriggerGameOver()
    {
        _gameOverActive = true;
        SetPlayerEnabled(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void ReturnToMenu()
    {
        _gameOverActive = false;
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.ResetHP();
        OpenMenu();
    }

    void ApplyGamma(float g)
    {
        if (_buckshotFeature == null) return;
        _buckshotFeature.settings.gamma = g;
    }

    void BuildStyles()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            font      = font,
            fontSize  = 17,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _labelStyle.normal.textColor = Color.white;
    }
}
