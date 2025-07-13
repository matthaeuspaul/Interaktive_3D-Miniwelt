using UnityEngine;
using DG.Tweening;

public class PickUpItemInteraction : MonoBehaviour, IInteractable
{
    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.5f;
    [SerializeField] private Ease _animationEase = Ease.OutQuad;

    [Header("UI Settings")]
    [SerializeField] private string _pickupPrompt = "(F) to Pick Up";
    [SerializeField] private string _dropPrompt = "(G) to Drop";

    [Header("Drop Settings")]
    [SerializeField] private bool _usePhysicsDrop = true;

    [Header("Item Usage")]
    [SerializeField] private LayerMask _usableObjectsLayer = -1;
    [SerializeField] private float _useRange = 3f;
    [SerializeField] private string _useItemPrompt = "(F) Use Item";

    [Header("Debug")]
    [SerializeField] private bool _debugMode = false;

    private bool _isPickedUp = false;
    private Transform _originalParent;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Tween _currentTween;
    private bool _wasDropped = false;
    private float _dropCooldown = 0f;
    private const float DROP_COOLDOWN_TIME = 0.5f;

    // Physics Components
    private Rigidbody _rigidbody;
    private bool _originalKinematicState;

    // Referenz zum aktuellen Holder (Player)
    private static PickUpItemInteraction _currentHeldItem;
    private static Interactor _currentHolder;

    private void Awake()
    {
        _originalParent = transform.parent;
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;

        _rigidbody = GetComponent<Rigidbody>();

        if (_rigidbody != null)
        {
            _originalKinematicState = _rigidbody.isKinematic;
            _rigidbody.isKinematic = true;
        }
    }

    private void Update()
    {
        if (_dropCooldown > 0f)
        {
            _dropCooldown -= Time.deltaTime;
        }

        // Entferne die HandleItemUsage() aus Update - das macht der Interactor
    }

    public bool CanInteract()
    {
        if (_isPickedUp || _dropCooldown > 0f)
        {
            if (_debugMode && _dropCooldown > 0f)
            {
                Debug.Log($"Drop cooldown active: {_dropCooldown:F2}");
            }
            return false;
        }

        return true;
    }

    public string GetInteractionPrompt()
    {
        return _isPickedUp ? _dropPrompt : _pickupPrompt;
    }

    public bool Interact(Interactor interactor)
    {
        if (!_isPickedUp)
        {
            PickUpItem(interactor);
        }
        return true;
    }

    private void PickUpItem(Interactor interactor)
    {
        // Prüfe ob der Player bereits ein Item hält
        if (_currentHeldItem != null && _currentHeldItem != this)
        {
            Debug.LogWarning("Player is already holding an item!");
            return;
        }

        if (!CanInteract())
        {
            if (_debugMode)
            {
                Debug.LogWarning("Cannot pick up item - CanInteract returned false");
            }
            return;
        }

        _isPickedUp = true;
        _currentHeldItem = this;
        _currentHolder = interactor;

        _currentTween?.Kill();
        DisablePhysics();

        Transform handPosition = GetHandPosition(interactor);
        transform.SetParent(handPosition);

        _currentTween = DOTween.Sequence()
            .Join(transform.DOLocalMove(Vector3.zero, _animationDuration).SetEase(_animationEase))
            .Join(transform.DOLocalRotate(Vector3.zero, _animationDuration).SetEase(_animationEase));

        if (_debugMode)
        {
            Debug.Log($"Picked up item: {gameObject.name}");
        }
    }

    public void DropItem()
    {
        if (!_isPickedUp) return;

        _isPickedUp = false;
        _currentHeldItem = null;
        _currentHolder = null;
        _wasDropped = true;
        _dropCooldown = DROP_COOLDOWN_TIME;

        _currentTween?.Kill();
        transform.SetParent(_originalParent);

        if (_usePhysicsDrop)
        {
            DropWithPhysics();
        }

        if (_debugMode)
        {
            Debug.Log($"Dropped item: {gameObject.name}");
        }
    }

    private void DropWithPhysics()
    {
        if (_rigidbody != null)
        {
            EnablePhysics();

            Vector3 forwardForce = _currentHolder.transform.forward * 2f;
            _rigidbody.AddForce(forwardForce, ForceMode.Impulse);

            StartCoroutine(DisablePhysicsAfterTime(3f));
        }
        else
        {
            Debug.LogWarning("No Rigidbody found for physics drop");
        }
    }

    private void EnablePhysics()
    {
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = false;
        }
    }

    private void DisablePhysics()
    {
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
        }
    }

    private System.Collections.IEnumerator DisablePhysicsAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        if (this != null && !_isPickedUp)
        {
            DisablePhysics();
        }
    }

    private Transform GetHandPosition(Interactor interactor)
    {
        Transform handPosition = interactor.transform.Find("HandPosition");

        if (handPosition == null)
        {
            GameObject handPosObj = new GameObject("HandPosition");
            handPosObj.transform.SetParent(interactor.transform);
            handPosObj.transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
            handPosObj.transform.localRotation = Quaternion.identity;

            handPosition = handPosObj.transform;

            if (_debugMode)
            {
                Debug.Log("Created HandPosition for interactor");
            }
        }

        return handPosition;
    }

    private void OnDestroy()
    {
        if (_currentHeldItem == this)
        {
            _currentHeldItem = null;
            _currentHolder = null;
        }

        _currentTween?.Kill();
    }

    [ContextMenu("Reset to Original Position")]
    public void ResetToOriginalPosition()
    {
        if (_isPickedUp)
        {
            DropItem();
        }

        _currentTween?.Kill();
        StopAllCoroutines();

        transform.SetParent(_originalParent);
        transform.position = _originalPosition;
        transform.rotation = _originalRotation;
        _wasDropped = false;
        _dropCooldown = 0f;

        DisablePhysics();

        if (_debugMode)
        {
            Debug.Log("Reset item to original position");
        }
    }

    public static bool IsPlayerHoldingItem()
    {
        return _currentHeldItem != null;
    }

    public static PickUpItemInteraction GetCurrentHeldItem()
    {
        return _currentHeldItem;
    }

    public static Interactor GetCurrentHolder()
    {
        return _currentHolder;
    }

    public void ForceDropItem()
    {
        if (_isPickedUp)
        {
            DropItem();
        }
    }
}