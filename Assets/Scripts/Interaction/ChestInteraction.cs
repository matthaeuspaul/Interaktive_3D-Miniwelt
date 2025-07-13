using DG.Tweening;
using UnityEngine;

public class ChestInteraction : MonoBehaviour, IInteractable
{
    [Header("Chest Settings")]
    [SerializeField] private float _openAngle = 40f;
    [SerializeField] private float _animationDuration = 0.8f;
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
        Debug.Log($"ChestInteraction Script gestartet auf: {gameObject.name}");

        // Definiere die Rotationen
        _closedRotation = new Vector3(270f, 270f, 90f);
        _openRotation = new Vector3(_openAngle, 270f, 90f);

        if (_showDebug)
        {
            Debug.Log($"BEFORE - Transform rotation: {transform.localEulerAngles}");
            Debug.Log($"TARGET - Closed: {_closedRotation}, Open: {_openRotation}");
        }

        // Setze die Startposition mit Quaternion.Euler (funktioniert zuverlässig)
        transform.localRotation = Quaternion.Euler(_closedRotation);

        if (_showDebug)
        {
            Debug.Log($"AFTER - Transform rotation: {transform.localEulerAngles}");
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
            Debug.Log($"INTERACTION - Current: {transform.localEulerAngles}");
            Debug.Log($"INTERACTION - Target: {targetRotation} (Open: {!_isOpen})");
        }

        // LÖSUNG: Verwende DOLocalRotateQuaternion für glatte Rotation ohne zusätzliche Drehungen
        Quaternion targetQuaternion = _isOpen ? Quaternion.Euler(_closedRotation) : Quaternion.Euler(_openRotation);

        _currentTween = transform.DOLocalRotateQuaternion(targetQuaternion, _animationDuration)
            .SetEase(_animationEase)
            .OnComplete(() => {
                if (_showDebug)
                {
                    Debug.Log($"COMPLETE - Final rotation: {transform.localEulerAngles}");
                }
                _currentTween = null;
            });

        _isOpen = !_isOpen;
        return true;
    }
}