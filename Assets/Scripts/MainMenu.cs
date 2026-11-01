using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Unity.Cinemachine;
using DG.Tweening;

// Scene UI, not a DontDestroyOnLoad manager: WinScreen.Restart reloads the scene and brings a fresh Main Menu
public class MainMenu : MonoBehaviour
{
    public UnityEvent OnGameStarted;

    [SerializeField] private CanvasGroup menuPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private CinemachineCamera menuCamera;
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private float fadeDuration = 0.5f;

    private bool isStarting;

    private void Start()
    {
        SetMenuVisible(true);
        if (menuCamera != null) menuCamera.enabled = true;
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void OnDestroy()
    {
        if (startButton != null) startButton.onClick.RemoveListener(StartGame);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
    }

    public void StartGame()
    {
        if (isStarting) return;
        isStarting = true;
        SetButtonsInteractable(false);
        if (fade == null)
        {
            BeginGame();
            OnGameStarted?.Invoke();
            return;
        }
        fade.DOKill();
        fade.blocksRaycasts = true;
        Sequence startSequence = DOTween.Sequence().SetLink(gameObject);
        startSequence.Append(fade.DOFade(1f, fadeDuration));
        // The screen is black here, so the camera switch and the Player appearing are not visible
        startSequence.AppendCallback(BeginGame);
        startSequence.Append(fade.DOFade(0f, fadeDuration));
        startSequence.AppendCallback(() =>
        {
            fade.blocksRaycasts = false;
            OnGameStarted?.Invoke();
        });
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void BeginGame()
    {
        SetMenuVisible(false);
        if (menuCamera != null) menuCamera.enabled = false;
        PlayerStateManager.Instance.SetState(PlayerState.Regular);
    }

    private void SetMenuVisible(bool visible)
    {
        if (menuPanel == null) return;
        menuPanel.DOKill();
        menuPanel.alpha = visible ? 1f : 0f;
        menuPanel.interactable = visible;
        menuPanel.blocksRaycasts = visible;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (startButton != null) startButton.interactable = interactable;
        if (quitButton != null) quitButton.interactable = interactable;
    }
}
