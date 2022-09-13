using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private Vector2 _curMovementInput;
    public float jumpForce;
    public LayerMask groundLayerMask;

    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;
    public float maxXLook;
    private float _camCurXRot;
    public float lookSensitivity;

    private Vector2 _mouseDelta;

    [HideInInspector]
    public bool canLook = true;

    private Rigidbody _rig;

    public static PlayerController instance;

    private void Awake ()
    {
        _rig = GetComponent<Rigidbody>();

        instance = this;
    }

    private void Start ()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate ()
    {
        Move();
    }

    private void LateUpdate ()
    {
        if(canLook == true)
            CameraLook();
    }

    private void Move ()
    {
        Vector3 dir = transform.forward * _curMovementInput.y + transform.right * _curMovementInput.x;
        dir *= moveSpeed;
        dir.y = _rig.velocity.y;

        _rig.velocity = dir;
    }

    private void CameraLook ()
    {
        _camCurXRot += _mouseDelta.y * lookSensitivity;
        _camCurXRot = Mathf.Clamp(_camCurXRot, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-_camCurXRot, 0, 0);

        transform.eulerAngles += new Vector3(0, _mouseDelta.x * lookSensitivity, 0);
    }

    public void OnLookInput (InputAction.CallbackContext context)
    {
        _mouseDelta = context.ReadValue<Vector2>();
    }

    public void OnMoveInput (InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            _curMovementInput = context.ReadValue<Vector2>();
        }
        else if(context.phase == InputActionPhase.Canceled)
        {
            _curMovementInput = Vector2.zero;
        }
    }

    public void OnJumpInput (InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started)
        {
            if(IsGrounded())
            {
                _rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }    

    private bool IsGrounded ()
    {
        Ray[] rays = new Ray[4]
        {
            new Ray(transform.position + (transform.forward * 0.2f) + (Vector3.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.forward * 0.2f) + (Vector3.up * 0.01f), Vector3.down),
            new Ray(transform.position + (transform.right * 0.2f) + (Vector3.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.right * 0.2f) + (Vector3.up * 0.01f), Vector3.down)
        };

        for(int i = 0; i < rays.Length; i++)
        {
            if(Physics.Raycast(rays[i], 0.1f, groundLayerMask))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos ()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay(transform.position + (transform.forward * 0.2f), Vector3.down);
        Gizmos.DrawRay(transform.position + (-transform.forward * 0.2f), Vector3.down);
        Gizmos.DrawRay(transform.position + (transform.right * 0.2f), Vector3.down);
        Gizmos.DrawRay(transform.position + (-transform.right * 0.2f), Vector3.down);
    }

    public void ToggleCursor (bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }
}