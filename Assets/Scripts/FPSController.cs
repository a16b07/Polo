using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Crouch")]
    public float crouchSpeed      = 3f;
    public float crouchHeight     = 1.0f;
    public float standHeight      = 2.0f;
    public float crouchCamOffset  = -0.5f;

    CharacterController _cc;
    Vector3 _velocity;
    Vector2 _moveInput;
    bool _jumpPressed;
    bool _sprintHeld;
    bool _crouchHeld;
    bool _canDoubleJump;

    Transform _camTransform;
    float     _camStandY;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        standHeight = _cc.height;
        _camTransform = GetComponentInChildren<Camera>()?.transform;
        if (_camTransform != null) _camStandY = _camTransform.localPosition.y;
    }

    void OnEnable()  => InputSystem.onAfterUpdate += ReadInput;
    void OnDisable() => InputSystem.onAfterUpdate -= ReadInput;

    void ReadInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        _moveInput  = new Vector2(h, v);
        _sprintHeld = kb.leftShiftKey.isPressed;
        _crouchHeld = kb.leftCtrlKey.isPressed;
        if (kb.spaceKey.wasPressedThisFrame) _jumpPressed = true;
    }

    void Update()
    {
        // Crouch — keep CC bottom pinned at same world offset
        float targetHeight = _crouchHeld ? crouchHeight : standHeight;
        _cc.height = Mathf.Lerp(_cc.height, targetHeight, Time.deltaTime * 10f);
        _cc.center = new Vector3(0f, (_cc.height - standHeight) * 0.5f, 0f);

        if (_camTransform != null)
        {
            float targetCamY = _crouchHeld ? _camStandY + crouchCamOffset : _camStandY;
            var lp = _camTransform.localPosition;
            _camTransform.localPosition = new Vector3(lp.x, Mathf.Lerp(lp.y, targetCamY, Time.deltaTime * 10f), lp.z);
        }

        bool grounded = _cc.isGrounded;
        if (grounded && _velocity.y < 0f) _velocity.y = -2f;

        if (PerkManager.IsMenuOpen) return;
        float statMult = GetComponent<PlayerStats>()?.speedMultiplier ?? 1f;
        float speed = (_crouchHeld ? crouchSpeed : (_sprintHeld ? sprintSpeed : walkSpeed)) * statMult;
        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        _cc.Move(move * speed * Time.deltaTime);

        bool hasDoubleJump = PerkManager.Instance?.HasFlag("DOUBLE_JUMP") == true;
        if (grounded) _canDoubleJump = true;

        if (_jumpPressed && !_crouchHeld)
        {
            if (grounded)
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            else if (hasDoubleJump && _canDoubleJump)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _canDoubleJump = false;
            }
        }
        _jumpPressed = false;

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    public void Resize(float multiplier)
    {
        standHeight  *= multiplier;
        crouchHeight *= multiplier;
        _cc.height    = standHeight;
        _cc.center    = Vector3.zero;
        if (_camTransform != null)
        {
            _camStandY *= multiplier;
            var lp = _camTransform.localPosition;
            _camTransform.localPosition = new Vector3(lp.x, _camStandY, lp.z);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var rb = hit.collider.GetComponentInParent<Rigidbody>();
        if (rb == null || rb.isKinematic) return;
        if (hit.moveDirection.y < -0.3f) return; // ignore downward (standing on top)
        Vector3 push = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z) * 22f;
        rb.AddForce(push, ForceMode.Force);
    }
}
