using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private float _castDistance = 5f;
    [SerializeField] private Vector3 _raycastOffset = new Vector3(0, 1f, 0);
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private InteractionUI _interactionUI;
    [SerializeField] private Camera _playerCamera;

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
            _interactionUI = FindFirstObjectByType<InteractionUI>();
        }

        if (_playerCamera == null)
        {
            _playerCamera = Camera.main;
            if (_playerCamera == null)
            {
                _playerCamera = FindFirstObjectByType<Camera>();
                Debug.LogWarning("Camera.main not found, using first Camera found");
            }
        }

        if (_playerCamera == null)
        {
            Debug.LogError("No camera found! Please assign a camera to the Interactor.");
        }

        // LayerMask für alle Layer setzen
        layerMask = ~0;
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
                Debug.Log($"Interacting with: {_currentInteractable}");
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
                Debug.Log($"Found NEW interactable: {newInteractable}");
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
                Debug.Log("Lost interactable");
                _currentInteractable = null;
                HideInteractionPrompt();
            }
        }
    }

    private bool DoInteractionTest(out IInteractable interactable)
    {
        interactable = null;

        // Prüfe ob Kamera vorhanden ist
        if (_playerCamera == null)
        {
            Debug.LogError("No camera assigned to Interactor!");
            return false;
        }

        // Raycast von der Kamera statt vom Player
        Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        Debug.DrawRay(ray.origin, ray.direction * _castDistance, Color.red, 0.1f);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, _castDistance, layerMask))
        {
            Debug.Log($"Raycast hit: {hitInfo.collider.name} at distance {hitInfo.distance}");
            interactable = hitInfo.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                Debug.Log($"Found IInteractable component: {interactable}");
                return true;
            }
            else
            {
                Debug.Log($"No IInteractable component on {hitInfo.collider.name}");
            }
        }
        else
        {
            Debug.Log("Raycast hit nothing");
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
        if (_playerCamera != null)
        {
            Gizmos.color = Color.red;
            Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Vector3 rayStart = ray.origin;
            Vector3 rayEnd = rayStart + ray.direction * _castDistance;
            Gizmos.DrawLine(rayStart, rayEnd);

            // Zeige Raycast-Treffer
            if (_currentInteractable != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(rayEnd, 0.2f);
            }
        }
    }
}