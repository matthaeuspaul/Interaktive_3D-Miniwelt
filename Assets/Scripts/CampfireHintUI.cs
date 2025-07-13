using UnityEngine;
using TMPro;

public class CampfireHintUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _hintPanel;
    [SerializeField] private TextMeshProUGUI _hintText;
    [SerializeField] private TextMeshProUGUI _progressText;

    [Header("Hint Settings")]
    [SerializeField] private string _hintMessage = "Sammle Holz und bringe es zum Campfire!";
    [SerializeField] private float _fadeOutDuration = 1f;
    [SerializeField] private bool _showProgress = true;

    [Header("Campfire Reference")]
    [SerializeField] private CampfireInteraction _campfire;

    private bool _isHintVisible = true;
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        // Auto-finde Campfire falls nicht zugewiesen
        if (_campfire == null)
        {
            _campfire = FindObjectOfType<CampfireInteraction>();
        }

        if (_campfire == null)
        {
            Debug.LogError("CampfireHintUI: No CampfireInteraction found!");
            return;
        }

        // Setup UI
        SetupUI();

        // Zeige Hint initial an
        ShowHint();
    }

    private void SetupUI()
    {
        // Erstelle Canvas Group für Fade-Effekt
        _canvasGroup = _hintPanel.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = _hintPanel.AddComponent<CanvasGroup>();
        }

        // Setze Hint Text
        if (_hintText != null)
        {
            _hintText.text = _hintMessage;
        }

        // Initial Panel aktivieren
        _hintPanel.SetActive(true);
        _canvasGroup.alpha = 1f;
    }

    private void Update()
    {
        if (!_isHintVisible || _campfire == null) return;

        // Prüfe ob Campfire voll ist
        if (_campfire.IsMaxLogs())
        {
            HideHint();
            return;
        }

        // Update Progress Text
        if (_showProgress && _progressText != null)
        {
            int current = _campfire.GetCurrentLogCount();
            int max = _campfire.GetMaxLogCount();
            _progressText.text = $"Logs: {current}/{max}";
        }
    }

    private void ShowHint()
    {
        if (_hintPanel == null) return;

        _hintPanel.SetActive(true);
        _canvasGroup.alpha = 1f;
        _isHintVisible = true;

        Debug.Log("Campfire hint shown");
    }

    private void HideHint()
    {
        if (!_isHintVisible) return;

        _isHintVisible = false;

        // Fade out effect
        StartCoroutine(FadeOutHint());

        Debug.Log("Campfire hint hidden - campfire is full!");
    }

    private System.Collections.IEnumerator FadeOutHint()
    {
        float startAlpha = _canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < _fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _fadeOutDuration;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _hintPanel.SetActive(false);
    }

    // Public Methods für externe Kontrolle
    public void ForceShowHint()
    {
        if (_campfire != null && !_campfire.IsMaxLogs())
        {
            ShowHint();
        }
    }

    public void ForceHideHint()
    {
        HideHint();
    }

    public void SetCampfire(CampfireInteraction campfire)
    {
        _campfire = campfire;
    }

    public void SetHintMessage(string message)
    {
        _hintMessage = message;
        if (_hintText != null)
        {
            _hintText.text = message;
        }
    }

    // Debug Methods
    [ContextMenu("Test Show Hint")]
    private void TestShowHint()
    {
        ForceShowHint();
    }

    [ContextMenu("Test Hide Hint")]
    private void TestHideHint()
    {
        ForceHideHint();
    }
}