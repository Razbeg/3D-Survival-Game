using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public float checkRate = 0.05f;
    private float _lastCheckTime;
    public float maxCheckDistance;
    public LayerMask layerMask;

    private GameObject _curInteractGameObject;
    private IInteractable _curInteractable;

    public TextMeshProUGUI promptText;
    private Camera _cam;

    private void Start ()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        if(Time.time - _lastCheckTime > checkRate)
        {
            _lastCheckTime = Time.time;

            Ray ray = _cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                if(hit.collider.gameObject != _curInteractGameObject)
                {
                    _curInteractGameObject = hit.collider.gameObject;
                    _curInteractable = hit.collider.GetComponent<IInteractable>();
                    SetPromptText();
                }
            }
            else
            {
                _curInteractGameObject = null;
                _curInteractable = null;
                promptText.gameObject.SetActive(false);
            }
        }
    }

    private void SetPromptText ()
    {
        promptText.gameObject.SetActive(true);
        promptText.text = string.Format("<b>[E]</b> {0}", _curInteractable.GetInteractPrompt());
    }

    public void OnInteractInput (InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started && _curInteractable != null)
        {
            _curInteractable.OnInteract();
            _curInteractGameObject = null;
            _curInteractable = null;
            promptText.gameObject.SetActive(false);
        }
    }
}

public interface IInteractable
{
    string GetInteractPrompt();
    void OnInteract();
}