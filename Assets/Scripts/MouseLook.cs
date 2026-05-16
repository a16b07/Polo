using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Look")]
    public float sensitivity = 0.12f;
    public float maxPitch    = 85f;

    float     _pitch;
    Transform _playerBody;
    bool      _tabUnlocked;

    void Start()
    {
        _playerBody          = transform.parent;
        Cursor.lockState     = CursorLockMode.Locked;
        Cursor.visible       = false;
    }

    void Update()
    {
        bool tabHeld = Keyboard.current != null && Keyboard.current.tabKey.isPressed;

        // Wheel click toggles cursor while Tab is held
        if (tabHeld && Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame)
            _tabUnlocked = !_tabUnlocked;
        if (!tabHeld) _tabUnlocked = false; // always re-lock when Tab released

        if (PerkManager.IsMenuOpen || _tabUnlocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 delta = mouse.delta.ReadValue() * sensitivity;
        if (PerkManager.Instance?.HasFlag("PARANOIA") == true)
            delta += Random.insideUnitCircle * 0.35f;

        _pitch -= delta.y;
        _pitch  = Mathf.Clamp(_pitch, -maxPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        _playerBody.Rotate(Vector3.up * delta.x);
    }
}
