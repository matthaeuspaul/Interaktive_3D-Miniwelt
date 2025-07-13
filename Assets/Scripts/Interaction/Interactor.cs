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

        // Handle Interaction Input (F-Taste)
        if (_inputHandler != null && _inputHandler.InteractPressed)
        {
            Debug.Log("Interact button pressed");
            HandleInteraction();
        }

        // Handle Drop Input (G-Taste)
        if (_inputHandler != null && _inputHandler.DropPressed)
        {
            Debug.Log("Drop button pressed");
            HandleDropAction();
        }
    }

    private void HandleInteraction()
    {
        if (_currentInteractable == null) return;

        // Prüfe ob das Objekt IItemUsable implementiert
        IItemUsable itemUsable = _currentInteractable as IItemUsable;

        if (itemUsable != null)
        {
            // Spezielle Behandlung für IItemUsable-Objekte
            HandleItemUsableInteraction(itemUsable);
        }
        else
        {
            // Standard-Interaktion
            HandleStandardInteraction();
        }
    }

    private void HandleItemUsableInteraction(IItemUsable itemUsable)
    {
        Debug.Log("Handling IItemUsable interaction");

        // Prüfe ob Player ein Item hält
        if (PickUpItemInteraction.IsPlayerHoldingItem())
        {
            PickUpItemInteraction heldItem = PickUpItemInteraction.GetCurrentHeldItem();

            if (heldItem != null && itemUsable.CanUseItem(heldItem))
            {
                Debug.Log($"Using item {heldItem.gameObject.name} on {itemUsable}");

                // Verwende das Item
                if (itemUsable.UseItem(heldItem))
                {
                    // Item wurde erfolgreich verwendet, vernichte es
                    Debug.Log($"Item {heldItem.gameObject.name} consumed");
                    Destroy(heldItem.gameObject);
                }
            }
            else
            {
                Debug.Log("Cannot use held item with this object");
            }
        }
        else
        {
            // Fallback auf normale Interaktion wenn kein Item gehalten wird
            if (_currentInteractable.CanInteract())
            {
                Debug.Log("Using fallback interaction for IItemUsable");
                _currentInteractable.Interact(this);
            }
        }
    }

    private void HandleStandardInteraction()
    {
        Debug.Log("Handling standard interaction");

        // Normale Interaktion nur wenn kein Item gehalten wird ODER wenn es ein PickUpItemInteraction ist
        PickUpItemInteraction pickupItem = _currentInteractable as PickUpItemInteraction;

        if (pickupItem != null)
        {
            // PickUpItemInteraction hat ihre eigene Logik für Drop/Pickup
            if (_currentInteractable.CanInteract())
            {
                _currentInteractable.Interact(this);
            }
        }
        else if (!PickUpItemInteraction.IsPlayerHoldingItem())
        {
            // Normale Objekte nur ohne gehaltenes Item
            if (_currentInteractable.CanInteract())
            {
                _currentInteractable.Interact(this);
            }
        }
        else
        {
            Debug.Log("Cannot interact with standard object while holding item");
        }
    }

    private void HandleDropAction()
    {
        if (PickUpItemInteraction.IsPlayerHoldingItem())
        {
            PickUpItemInteraction heldItem = PickUpItemInteraction.GetCurrentHeldItem();
            if (heldItem != null && PickUpItemInteraction.GetCurrentHolder() == this)
            {
                heldItem.DropItem();
            }
        }
    }

    private void CheckForInteractables()
    {
        IInteractable newInteractable = null;
        bool isHoldingItem = PickUpItemInteraction.IsPlayerHoldingItem();

        // Prüfe zuerst ob der Player ein Item hält
        if (isHoldingItem)
        {
            PickUpItemInteraction heldItem = PickUpItemInteraction.GetCurrentHeldItem();

            // Prüfe ob dieses Item vom aktuellen Interactor gehalten wird
            if (heldItem != null && PickUpItemInteraction.GetCurrentHolder() == this)
            {
                // Mache einen Raycast um zu sehen, ob wir auf ein IItemUsable-Objekt schauen
                if (DoInteractionTest(out IInteractable raycastTarget))
                {
                    IItemUsable itemUsable = raycastTarget as IItemUsable;

                    if (itemUsable != null)
                    {
                        // Wir schauen auf ein IItemUsable-Objekt - zeige dessen Prompt
                        newInteractable = raycastTarget;
                    }
                    else
                    {
                        // Wir schauen auf ein normales Objekt - zeige Drop-Prompt
                        newInteractable = heldItem;
                    }
                }
                else
                {
                    // Wir schauen auf nichts - zeige Drop-Prompt
                    newInteractable = heldItem;
                }
            }
        }
        else
        {
            // Falls kein Item gehalten wird, mache normalen Raycast
            DoInteractionTest(out newInteractable);
        }

        // Update UI basierend auf dem gefundenen Interactable
        if (newInteractable != _currentInteractable)
        {
            if (newInteractable != null)
            {
                Debug.Log($"Found NEW interactable: {newInteractable}");
                _currentInteractable = newInteractable;
                ShowInteractionPrompt();
            }
            else
            {
                Debug.Log("Lost interactable");
                _currentInteractable = null;
                HideInteractionPrompt();
            }
        }
        else if (_currentInteractable != null)
        {
            // Gleiches Interactable - aktualisiere den Text falls sich der Status geändert hat
            UpdateInteractionPrompt();
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
            string promptText = GetCurrentInteractionPrompt();
            _interactionUI.ShowPrompt(promptText);
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (_interactionUI != null && _currentInteractable != null)
        {
            string promptText = GetCurrentInteractionPrompt();
            _interactionUI.UpdatePrompt(promptText);
        }
    }

    private string GetCurrentInteractionPrompt()
    {
        if (_currentInteractable == null) return "";

        // Prüfe ob das Objekt IItemUsable implementiert
        IItemUsable itemUsable = _currentInteractable as IItemUsable;

        if (itemUsable != null)
        {
            // Spezielle Prompt-Logik für IItemUsable
            if (PickUpItemInteraction.IsPlayerHoldingItem())
            {
                PickUpItemInteraction heldItem = PickUpItemInteraction.GetCurrentHeldItem();
                return itemUsable.GetUsePrompt(heldItem);
            }
            else
            {
                return itemUsable.GetUsePrompt(null);
            }
        }
        else
        {
            // Standard-Prompt
            return _currentInteractable.GetInteractionPrompt();
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

        // Visualisiere gehaltenes Item
        if (PickUpItemInteraction.IsPlayerHoldingItem())
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
        }
    }
}