using UnityEngine;

public class CampfireInteraction : MonoBehaviour, IInteractable, IItemUsable
{
    [Header("Campfire Settings")]
    [SerializeField] private GameObject[] _logObjects;

    [Header("Item Requirements")]
    [SerializeField] private string _requiredItemName = "Holz";

    [Header("UI Settings")]
    [SerializeField] private string _addLogPrompt = "(F) Place Log";
    [SerializeField] private string _noItemPrompt = "Need wood to add logs";
    [SerializeField] private string _maxLogsPrompt = "Campfire is full";

    [Header("Debug")]
    [SerializeField] private bool _showDebug = true;
    [SerializeField] private bool _skipItemRequirement = false;

    private int _currentLogIndex = 0;

    private void Start()
    {
        Debug.Log("=== CAMPFIRE START CALLED ===");

        // Auto-finde Log-Objekte falls Array leer ist
        if (_logObjects == null || _logObjects.Length == 0)
        {
            AutoFindLogObjects();
        }

        // Verstecke alle Logs am Anfang
        for (int i = 0; i < _logObjects.Length; i++)
        {
            if (_logObjects[i] != null)
            {
                _logObjects[i].SetActive(false);
            }
        }

        if (_showDebug)
        {
            Debug.Log($"Campfire '{gameObject.name}' - Total logs: {_logObjects.Length}");
        }
    }

    private void AutoFindLogObjects()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        System.Collections.Generic.List<GameObject> foundLogs = new System.Collections.Generic.List<GameObject>();

        foreach (Transform child in children)
        {
            if (child != transform && child.name.ToLower().Contains("log"))
            {
                foundLogs.Add(child.gameObject);
            }
        }

        foundLogs.Sort((a, b) => a.name.CompareTo(b.name));
        _logObjects = foundLogs.ToArray();

        if (_showDebug)
        {
            Debug.Log($"Auto-found {_logObjects.Length} log objects");
        }
    }

    public bool CanInteract()
    {
        // Prüfe ob noch Logs hinzugefügt werden können
        if (_currentLogIndex >= _logObjects.Length)
        {
            if (_showDebug) Debug.Log("Cannot interact: Max logs reached");
            return false;
        }

        // Prüfe ob Item-Requirement erfüllt ist
        bool hasItem = _skipItemRequirement || HasRequiredItem();

        if (_showDebug)
        {
            Debug.Log($"CanInteract: LogIndex={_currentLogIndex}/{_logObjects.Length}, HasItem={hasItem}, SkipReq={_skipItemRequirement}");
        }

        return hasItem;
    }

    public string GetInteractionPrompt()
    {
        // Prüfe ob noch Logs hinzugefügt werden können
        if (_currentLogIndex >= _logObjects.Length)
        {
            return _maxLogsPrompt;
        }

        // Prüfe ob Player ein Item hält
        if (!PickUpItemInteraction.IsPlayerHoldingItem())
        {
            return _noItemPrompt;
        }

        // Prüfe ob es das richtige Item ist
        if (!_skipItemRequirement && !HasRequiredItem())
        {
            return _noItemPrompt;
        }

        return _addLogPrompt;
    }

    public bool Interact(Interactor interactor)
    {
        Debug.Log("=== CAMPFIRE INTERACT CALLED ===");

        if (!CanInteract())
        {
            Debug.Log("=== INTERACT FAILED - CanInteract() returned false ===");
            return false;
        }

        // Doppelte Sicherheitsprüfung
        if (_currentLogIndex >= _logObjects.Length)
        {
            Debug.LogError($"Log index {_currentLogIndex} out of range! Array length: {_logObjects.Length}");
            return false;
        }

        GameObject currentLog = _logObjects[_currentLogIndex];
        if (currentLog == null)
        {
            Debug.LogError($"Log object at index {_currentLogIndex} is null!");
            return false;
        }

        if (_showDebug)
        {
            Debug.Log($"Activating log: {currentLog.name} (Index: {_currentLogIndex})");
        }

        // Aktiviere das Log
        currentLog.SetActive(true);

        // Konsumiere das Item
        if (!_skipItemRequirement)
        {
            ConsumeHeldItem();
        }

        _currentLogIndex++;

        if (_showDebug)
        {
            Debug.Log($"Log activated successfully. New index: {_currentLogIndex}");
        }

        return true;
    }

    private bool HasRequiredItem()
    {
        // Prüfe ob Player ein Item hält
        if (!PickUpItemInteraction.IsPlayerHoldingItem())
        {
            if (_showDebug) Debug.Log("HasRequiredItem: No item held");
            return false;
        }

        // Hole das gehaltene Item
        PickUpItemInteraction heldItem = PickUpItemInteraction.GetCurrentHeldItem();
        if (heldItem == null)
        {
            if (_showDebug) Debug.Log("HasRequiredItem: Held item is null");
            return false;
        }

        // Prüfe den Namen des Items
        string itemName = heldItem.gameObject.name;
        bool hasCorrectItem = itemName.ToLower().Contains(_requiredItemName.ToLower());

        if (_showDebug)
        {
            Debug.Log($"HasRequiredItem: Item='{itemName}' | Required='{_requiredItemName}' | Match={hasCorrectItem}");
        }

        return hasCorrectItem;
    }

    private void ConsumeHeldItem()
    {
        if (PickUpItemInteraction.IsPlayerHoldingItem())
        {
            PickUpItemInteraction heldItem = PickUpItemInteraction.GetCurrentHeldItem();
            if (heldItem != null)
            {
                if (_showDebug)
                {
                    Debug.Log($"Consuming item: {heldItem.gameObject.name}");
                }
                Destroy(heldItem.gameObject);
            }
        }
    }

    // === IItemUsable Interface Implementation ===
    public bool CanUseItem(PickUpItemInteraction item)
    {
        if (item == null) return false;

        // Prüfe ob noch Logs hinzugefügt werden können
        if (_currentLogIndex >= _logObjects.Length) return false;

        // Prüfe Item-Name
        if (_skipItemRequirement) return true;

        string itemName = item.gameObject.name;
        bool hasCorrectItem = itemName.ToLower().Contains(_requiredItemName.ToLower());

        if (_showDebug)
        {
            Debug.Log($"CanUseItem: Item='{itemName}' | Required='{_requiredItemName}' | Match={hasCorrectItem}");
        }

        return hasCorrectItem;
    }

    public bool UseItem(PickUpItemInteraction item)
    {
        if (!CanUseItem(item)) return false;

        if (_showDebug)
        {
            Debug.Log($"UseItem: Adding log {_currentLogIndex + 1} using item '{item.gameObject.name}'");
        }

        // Aktiviere nächstes Log
        if (_currentLogIndex < _logObjects.Length)
        {
            GameObject currentLog = _logObjects[_currentLogIndex];
            currentLog.SetActive(true);
            _currentLogIndex++;
        }

        return true;
    }

    public string GetUsePrompt(PickUpItemInteraction item)
    {
        if (!CanUseItem(item))
        {
            if (_currentLogIndex >= _logObjects.Length)
                return _maxLogsPrompt;
            else
                return _noItemPrompt;
        }

        return _addLogPrompt;
    }

    // === Utility Methods ===
    public int GetCurrentLogCount()
    {
        return _currentLogIndex;
    }

    public int GetMaxLogCount()
    {
        return _logObjects.Length;
    }

    public bool IsMaxLogs()
    {
        return _currentLogIndex >= _logObjects.Length;
    }

    // === Debug Methods ===
    [ContextMenu("Test Item Detection")]
    private void TestItemDetection()
    {
        Debug.Log("=== ITEM DETECTION TEST ===");

        bool isHolding = PickUpItemInteraction.IsPlayerHoldingItem();
        Debug.Log($"1. IsPlayerHoldingItem(): {isHolding}");

        if (isHolding)
        {
            PickUpItemInteraction heldItem = PickUpItemInteraction.GetCurrentHeldItem();
            Debug.Log($"2. GetCurrentHeldItem(): {(heldItem != null ? "NOT NULL" : "NULL")}");

            if (heldItem != null)
            {
                string itemName = heldItem.gameObject.name;
                Debug.Log($"3. Item name: '{itemName}'");
                Debug.Log($"4. Required name: '{_requiredItemName}'");

                bool nameMatch = itemName.ToLower().Contains(_requiredItemName.ToLower());
                Debug.Log($"5. Name match: {nameMatch}");

                bool hasRequired = HasRequiredItem();
                Debug.Log($"6. HasRequiredItem(): {hasRequired}");
            }
        }

        bool canInteract = CanInteract();
        Debug.Log($"7. CanInteract(): {canInteract}");

        string prompt = GetInteractionPrompt();
        Debug.Log($"8. Interaction prompt: '{prompt}'");

        Debug.Log("=== TEST COMPLETE ===");
    }

    [ContextMenu("Debug Log States")]
    private void DebugLogStates()
    {
        Debug.Log($"=== Campfire Log Debug ===");
        Debug.Log($"Current Log Index: {_currentLogIndex}");
        Debug.Log($"Total Logs: {_logObjects.Length}");

        for (int i = 0; i < _logObjects.Length; i++)
        {
            if (_logObjects[i] != null)
            {
                GameObject log = _logObjects[i];
                Debug.Log($"Log {i}: {log.name} - Active: {log.activeSelf}");
            }
            else
            {
                Debug.Log($"Log {i}: NULL!");
            }
        }
    }

    [ContextMenu("Force Show First Log")]
    private void ForceShowFirstLog()
    {
        if (_logObjects.Length > 0 && _logObjects[0] != null)
        {
            _logObjects[0].SetActive(true);
            Debug.Log($"Force activated first log: {_logObjects[0].name}");
        }
    }

    [ContextMenu("Find Log Objects")]
    private void FindLogObjectsMenu()
    {
        AutoFindLogObjects();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestItemDetection();
        }
    }

    private void OnDrawGizmos()
    {
        // Visualisiere den Campfire Status
        if (_logObjects != null && _logObjects.Length > 0)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
            float radius = 0.5f + (_currentLogIndex * 0.1f);
            Gizmos.DrawWireSphere(transform.position, radius);

            // Zeige Anzahl der Logs
            Gizmos.color = Color.red;
            for (int i = 0; i < _currentLogIndex; i++)
            {
                Vector3 logPos = transform.position + Vector3.up * (i * 0.2f + 0.1f);
                Gizmos.DrawWireCube(logPos, Vector3.one * 0.1f);
            }
        }
    }
}