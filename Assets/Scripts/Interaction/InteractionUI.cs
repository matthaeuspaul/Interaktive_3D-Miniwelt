using UnityEngine;
using TMPro;
using DG.Tweening;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _promptPanel;
    [SerializeField] private TextMeshProUGUI _promptText;

    [Header("Animation Settings")]
    [SerializeField] private float _fadeInDuration = 0.2f;
    [SerializeField] private float _fadeOutDuration = 0.15f;
    [SerializeField] private float _textChangeDuration = 0.1f;
    [SerializeField] private Ease _fadeInEase = Ease.OutQuad;
    [SerializeField] private Ease _fadeOutEase = Ease.InQuad;
    [SerializeField] private Ease _textChangeEase = Ease.InOutQuad;

    private CanvasGroup _canvasGroup;
    private Tween _currentTween;
    private bool _isVisible = false;
    private string _currentText = "";
    private bool _isChangingText = false;

    private void Awake()
    {
        // Setup Canvas Group für Fade-Effekte
        if (_promptPanel != null)
        {
            _canvasGroup = _promptPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = _promptPanel.AddComponent<CanvasGroup>();
            }
        }

        // Initial verstecken
        HidePrompt(true);
    }

    public void ShowPrompt(string promptText)
    {
        if (_promptText != null)
        {
            _currentText = promptText;
            _promptText.text = promptText;
        }

        if (_promptPanel != null && !_isVisible)
        {
            _isVisible = true;
            _promptPanel.SetActive(true);

            // Stoppe vorherige Animation
            _currentTween?.Kill();

            // Fade In Animation
            _currentTween = _canvasGroup.DOFade(1f, _fadeInDuration)
                .SetEase(_fadeInEase)
                .OnComplete(() => _currentTween = null);
        }
    }

    public void HidePrompt(bool immediate = false)
    {
        if (_promptPanel != null && _isVisible)
        {
            _isVisible = false;
            _currentText = "";

            // Stoppe vorherige Animation
            _currentTween?.Kill();

            if (immediate)
            {
                _canvasGroup.alpha = 0f;
                _promptPanel.SetActive(false);
            }
            else
            {
                // Fade Out Animation
                _currentTween = _canvasGroup.DOFade(0f, _fadeOutDuration)
                    .SetEase(_fadeOutEase)
                    .OnComplete(() => {
                        _promptPanel.SetActive(false);
                        _currentTween = null;
                    });
            }
        }
    }

    public void UpdatePrompt(string promptText)
    {
        if (_promptText != null && _isVisible && promptText != _currentText && !_isChangingText)
        {
            _isChangingText = true;
            _currentText = promptText;

            // Stoppe vorherige Animation
            _currentTween?.Kill();

            // Fade out aktueller Text
            _currentTween = _canvasGroup.DOFade(0f, _textChangeDuration)
                .SetEase(_textChangeEase)
                .OnComplete(() => {
                    // Ändere Text
                    _promptText.text = promptText;

                    // Fade in neuer Text
                    _currentTween = _canvasGroup.DOFade(1f, _textChangeDuration)
                        .SetEase(_textChangeEase)
                        .OnComplete(() => {
                            _currentTween = null;
                            _isChangingText = false;
                        });
                });
        }
    }
}