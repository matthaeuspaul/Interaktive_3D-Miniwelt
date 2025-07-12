using DG.Tweening;
using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private float _openAngle = -90f;
    [SerializeField] private float _animationDuration = 1f;
    [SerializeField] private Ease _animationEase = Ease.InOutQuad;

    [Header("UI Settings")]
    [SerializeField] private string _openPrompt = "(F) to Open";
    [SerializeField] private string _closePrompt = "(F) to Close";

    [Header("Debug")]
    [SerializeField] private bool _showDebug = true;

    private bool _isOpen = false;
    private Vector3 _closedRotation;
    private Vector3 _openRotation;
    private Tween _currentTween;

    private void Start()
    {
        _closedRotation = transform.localEulerAngles;
        _openRotation = _closedRotation + new Vector3(0, _openAngle, 0);

        if (_showDebug)
        {
            Debug.Log($"Door '{gameObject.name}' - Closed: {_closedRotation}, Open: {_openRotation}");
        }
    }

    public bool CanInteract()
    {
        return _currentTween == null || !_currentTween.IsActive();
    }

    public string GetInteractionPrompt()
    {
        return _isOpen ? _closePrompt : _openPrompt;
    }

    public bool Interact(Interactor interactor)
    {
        if (!CanInteract()) return false;

        _currentTween?.Kill();

        Vector3 targetRotation = _isOpen ? _closedRotation : _openRotation;

        if (_showDebug)
        {
            Debug.Log($"Door '{gameObject.name}' - Moving to: {targetRotation} (Open: {!_isOpen})");
        }

        _currentTween = transform.DOLocalRotate(targetRotation, _animationDuration)
            .SetEase(_animationEase)
            .OnComplete(() => {
                if (_showDebug)
                {
                    Debug.Log($"Door '{gameObject.name}' - Animation complete");
                }
                _currentTween = null;
            });

        _isOpen = !_isOpen;
        return true;
    }
}

