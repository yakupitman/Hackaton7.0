using System.Collections;
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
        if (fade != null)
        {
            fade.alpha = 0f;
            fade.blocksRaycasts = false;
        }
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
        if (menuPanel != null) menuPanel.interactable = false;
        StartCoroutine(StartGameRoutine());
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator StartGameRoutine()
    {
        if (fade != null)
        {
            fade.DOKill();
            fade.blocksRaycasts = true;
            yield return fade.DOFade(1f, fadeDuration).SetLink(gameObject).WaitForCompletion();
        }
        else
        {
            // Leave the click frame first, so the same key press can't also act in Regular
            yield return null;
        }
        SetMenuVisible(false);
        if (menuCamera != null) menuCamera.enabled = false;
        // Stay black until the CinemachineBrain finishes blending from the MenuCamera, so the camera move is never seen
        yield return null;
        CinemachineBrain brain = Camera.main != null ? Camera.main.GetComponent<CinemachineBrain>() : null;
        while (brain != null && brain.IsBlending) yield return null;
        PlayerStateManager.Instance.SetState(PlayerState.Regular);
        if (fade != null)
        {
            yield return fade.DOFade(0f, fadeDuration).SetLink(gameObject).WaitForCompletion();
            fade.blocksRaycasts = false;
        }
        OnGameStarted?.Invoke();
    }

    private void SetMenuVisible(bool visible)
    {
        if (menuPanel == null) return;
        menuPanel.alpha = visible ? 1f : 0f;
        menuPanel.interactable = visible;
        menuPanel.blocksRaycasts = visible;
    }
}
