using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private float _castDistance = 5f;
    [SerializeField] private Vector3 _raycastOffset = new Vector3(0, 1f, 0);
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private InteractionUI _interactionUI;

    private LayerMask layerMask;
    private IInteractable _currentInteractable;

    private void Awake()
    {
        if (_inputHandler == null)
        {
            _inputHandler = GetComponent<PlayerInputHandler>();
        }

        if (_interactionUI == null)
        {
            _interactionUI = FindObjectOfType<InteractionUI>();
        }
    }

    private void Update()
    {
        // Prüfe kontinuierlich auf Interactables
        CheckForInteractables();

        // Handle Interaction Input
        if (_inputHandler != null && _inputHandler.InteractPressed)
        {
            Debug.Log("Interact button pressed");
            if (_currentInteractable != null && _currentInteractable.CanInteract())
            {
                _currentInteractable.Interact(this);
            }
        }
    }

    private void CheckForInteractables()
    {
        IInteractable newInteractable = null;

        if (DoInteractionTest(out newInteractable))
        {
            if (newInteractable != _currentInteractable)
            {
                // Wechsel zu neuem Interactable
                _currentInteractable = newInteractable;
                ShowInteractionPrompt();
            }
            else if (_currentInteractable != null)
            {
                // Gleiches Interactable - aktualisiere den Text falls sich der Status geändert hat
                UpdateInteractionPrompt();
            }
        }
        else
        {
            // Kein Interactable mehr im Blick
            if (_currentInteractable != null)
            {
                _currentInteractable = null;
                HideInteractionPrompt();
            }
        }
    }

    private bool DoInteractionTest(out IInteractable interactable)
    {
        interactable = null;

        Ray ray = new Ray(transform.position + _raycastOffset, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, _castDistance))
        {
            interactable = hitInfo.collider.GetComponent<IInteractable>();
            return interactable != null;
        }

        return false;
    }

    private void ShowInteractionPrompt()
    {
        if (_interactionUI != null && _currentInteractable != null)
        {
            string promptText = _currentInteractable.GetInteractionPrompt();
            _interactionUI.ShowPrompt(promptText);
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (_interactionUI != null && _currentInteractable != null)
        {
            string promptText = _currentInteractable.GetInteractionPrompt();
            _interactionUI.UpdatePrompt(promptText);
        }
    }

    private void HideInteractionPrompt()
    {
        if (_interactionUI != null)
        {
            _interactionUI.HidePrompt();
        }
    }

    private void OnDrawGizmos()
    {
        // Visualisiere den Raycast im Scene View
        Gizmos.color = Color.red;
        Vector3 rayStart = transform.position + _raycastOffset;
        Vector3 rayEnd = rayStart + transform.forward * _castDistance;
        Gizmos.DrawLine(rayStart, rayEnd);

        // Zeige Raycast-Treffer
        if (_currentInteractable != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(rayEnd, 0.2f);
        }
    }
}
