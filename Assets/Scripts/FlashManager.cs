using UnityEngine;
using UnityEngine.InputSystem;

public class FlashManager : MonoBehaviour
{
    public static FlashManager Instance { get; private set; }

    public int flashCount = 3;

    Camera     _cam;
    Mesh       _flashMesh;
    Material[] _flashMats;
    Vector3    _flashScale = Vector3.one;

    void Awake() => Instance = this;

    void Start()
    {
        _cam = Camera.main;

        // Cache Flash model — search active and inactive objects
        var pickups = Resources.FindObjectsOfTypeAll<FlashPickup>();
        if (pickups.Length > 0)
        {
            var t  = pickups[0].gameObject;
            var mf = t.GetComponent<MeshFilter>();
            var mr = t.GetComponent<MeshRenderer>();
            if (mf != null) _flashMesh  = mf.sharedMesh;
            if (mr != null) _flashMats  = mr.sharedMaterials;
            _flashScale = t.transform.localScale;
        }
    }

    public void AddFlash() => flashCount++;

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.gKey.wasPressedThisFrame && flashCount > 0)
            ThrowFlash();
    }

    void ThrowFlash()
    {
        flashCount--;
        if (_cam == null) _cam = Camera.main;

        var go = new GameObject("ThrownFlash");
        go.transform.position = _cam.transform.position + _cam.transform.forward * 0.6f;
        go.transform.rotation = _cam.transform.rotation;

        // Use cached Flash mesh — always, no yellow-ball fallback
        if (_flashMesh != null)
        {
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = _flashMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterials = _flashMats;
            go.transform.localScale = _flashScale;
        }

        // Physics
        var cap       = go.AddComponent<CapsuleCollider>();
        cap.radius    = 0.12f;
        cap.height    = 0.35f;
        cap.direction = 2;

        var rb            = go.AddComponent<Rigidbody>();
        rb.mass           = 0.4f;
        rb.linearDamping  = 0.05f;
        rb.angularDamping = 0.3f;
        rb.linearVelocity  = _cam.transform.forward * 18f + Vector3.up * 4f;
        rb.angularVelocity = Random.insideUnitSphere * 6f;

        go.AddComponent<FlashGrenade>();
    }
}
