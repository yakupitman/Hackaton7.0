using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// Scene singleton without DontDestroyOnLoad: it references the scene Canvas, and WinScreen.Restart
// only destroys the DontDestroyOnLoad managers it lists, so a surviving popup would point at a destroyed panel
public class PondUnlockPopup : MonoBehaviour
{
    public static PondUnlockPopup Instance { get; private set; }

    public Pond CurrentPond { get; private set; }
    public bool IsOpen => CurrentPond != null;

    [SerializeField] private CanvasGroup panel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private string messageFormat = "Unlock this Pond for {0}?";
    [SerializeField] private float fadeDuration = 0.25f;

    private int currentPrice;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (panel != null)
        {
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }
        if (buyButton != null) buyButton.onClick.AddListener(OnBuyClicked);
        if (cancelButton != null) cancelButton.onClick.AddListener(Close);
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.OnMoneyChanged.AddListener(OnMoneyChanged);
            PlayerStateManager.Instance.OnStateChanged.AddListener(OnStateChanged);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (buyButton != null) buyButton.onClick.RemoveListener(OnBuyClicked);
        if (cancelButton != null) cancelButton.onClick.RemoveListener(Close);
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.OnMoneyChanged.RemoveListener(OnMoneyChanged);
            PlayerStateManager.Instance.OnStateChanged.RemoveListener(OnStateChanged);
        }
    }

    public void Show(Pond pond, int price)
    {
        CurrentPond = pond;
        currentPrice = price;
        if (messageText != null) messageText.text = string.Format(messageFormat, price);
        RefreshBuyButton();
        if (panel == null) return;
        panel.DOKill();
        panel.interactable = true;
        panel.blocksRaycasts = true;
        panel.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);
    }

    // Visual only; Pond.EndInteraction calls it, so it must never end the interaction itself
    public void Hide()
    {
        CurrentPond = null;
        if (panel == null) return;
        panel.DOKill();
        panel.interactable = false;
        panel.blocksRaycasts = false;
        panel.DOFade(0f, fadeDuration).SetEase(Ease.InQuad);
    }

    // The one clean exit, same as pressing E: hide, end the Pond interaction, back to Regular
    public void Close()
    {
        Pond pond = CurrentPond;
        Hide();
        if (pond != null) pond.EndInteraction();
        PlayerStateManager.Instance.SetState(PlayerState.Regular);
    }

    private void OnBuyClicked()
    {
        if (CurrentPond != null) CurrentPond.TryBuyUnlock();
        Close();
    }

    private void OnMoneyChanged(int money)
    {
        if (IsOpen) RefreshBuyButton();
    }

    private void OnStateChanged(PlayerState state)
    {
        // A Death while open (e.g. the Ghost) must not leave the popup up after the respawn
        if (!IsOpen || state != PlayerState.Dead) return;
        Pond pond = CurrentPond;
        Hide();
        pond.EndInteraction();
    }

    private void RefreshBuyButton()
    {
        if (buyButton == null) return;
        buyButton.interactable = PlayerStateManager.Instance != null && PlayerStateManager.Instance.Money >= currentPrice;
    }
}
