using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteractive : MonoBehaviour
{
    public float interactRange = 5f;

    Camera     _cam;
    Material   _outlineMat;
    GameObject _outlineGO;
    GUIStyle   _style;
    bool       _showPrompt;
    float      _pulseT;

    void Start()
    {
        _cam = Camera.main;
        BuildOutline();
    }

    void BuildOutline()
    {
        var col = GetComponent<BoxCollider>();
        if (col == null) return;

        _outlineGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _outlineGO.name = "DoorOutline";
        _outlineGO.transform.SetParent(transform, false);
        _outlineGO.transform.localPosition = col.center;
        _outlineGO.transform.localScale    = col.size;

        // Remove collider from visual — physics is handled by parent's BoxCollider
        Destroy(_outlineGO.GetComponent<Collider>());

        // Main renderer fully transparent — we only want the outline pass
        var mr     = _outlineGO.GetComponent<MeshRenderer>();
        var shader = Shader.Find("Custom/DoorOutline");
        if (shader == null) { Destroy(_outlineGO); return; }
        var mat = new Material(shader);
        mat.SetColor("_OutlineColor", new Color(0.25f, 0.92f, 1f, 0.85f));
        mat.SetFloat("_OutlineWidth", 0.018f);
        _outlineMat = mat;
        mr.material = mat;
        mr.shadowCastingMode  = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows     = false;

        _outlineGO.SetActive(false);
    }

    void Update()
    {
        if (_cam == null) return;

        bool waveClear = WaveManager.Instance != null && WaveManager.Instance.WaveClear;
        _outlineGO?.SetActive(waveClear);

        if (waveClear && _outlineMat != null)
        {
            _pulseT += Time.deltaTime * 3f;
            _outlineMat.SetFloat("_Pulse", Mathf.Sin(_pulseT) * 0.5f + 0.5f);
        }

        _showPrompt = false;
        if (!waveClear) return;

        Vector3 toCenter = transform.position - _cam.transform.position;
        float   along    = Vector3.Dot(toCenter, _cam.transform.forward);
        float   perp     = Vector3.Cross(toCenter, _cam.transform.forward).magnitude;

        if (along >= 0f && along <= interactRange && perp <= 3f)
        {
            _showPrompt = true;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                StartCutsceneThenReset();
        }
    }

    void StartCutsceneThenReset()
    {
        _showPrompt = false;
        bool goodEnding = ConsejoPickup.AllCollected;
        if (CutscenePlayer.Instance != null)
            CutscenePlayer.Instance.Play(goodEnding, () => WaveManager.Instance.StartNextWave());
        else
            WaveManager.Instance.StartNextWave();
    }

    void OnGUI()
    {
        if (!_showPrompt) return;
        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                fontSize  = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            _style.normal.textColor = Color.white;
        }
        GUI.Label(new Rect((Screen.width - 220f) / 2f, Screen.height / 2f + 28f, 220f, 20f),
                  "[E] Enter next wave", _style);
    }
}
