using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class WinScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private Button restartButton;
    [SerializeField] private EscapeSequence escapeSequence;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Start()
    {
        if (escapeSequence == null) escapeSequence = FindFirstObjectByType<EscapeSequence>();
        if (panel != null)
        {
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }
        if (escapeSequence != null) escapeSequence.OnEscaped.AddListener(Show);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
    }

    private void OnDestroy()
    {
        if (escapeSequence != null) escapeSequence.OnEscaped.RemoveListener(Show);
        if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
    }

    public void Show()
    {
        Time.timeScale = 0f;
        if (panel == null) return;
        panel.DOKill();
        panel.blocksRaycasts = true;
        // Unscaled update so the fade runs while the game is frozen
        panel.DOFade(1f, fadeDuration).SetUpdate(true).OnComplete(() => panel.interactable = true);
    }

    public void Restart()
    {
        if (panel != null) panel.interactable = false;
        Time.timeScale = 1f;
        DOTween.KillAll();
        // DontDestroyOnLoad managers would survive the reload, keep pointing at the old scene and destroy the fresh copies in Awake
        if (UIManager.Instance != null) Destroy(UIManager.Instance.gameObject);
        if (HoldManager.Instance != null) Destroy(HoldManager.Instance.gameObject);
        if (InputManager.Instance != null) Destroy(InputManager.Instance.gameObject);
        if (PlayerStateManager.Instance != null) Destroy(PlayerStateManager.Instance.gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
