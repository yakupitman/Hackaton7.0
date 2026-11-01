using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;

public enum PlayerState
{
    Regular,
    Interacting,
    Shopping,
    Dead,
    Menu
}

public class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager Instance { get; private set; }

    public UnityEvent<PlayerState> OnStateChanged;
    public UnityEvent<int> OnMoneyChanged;
    public UnityEvent<int, int> OnBaitInventoryChanged;
    public UnityEvent OnPlayerDeath;

    public PlayerState CurrentState { get; private set; }
    public int Money { get; private set; }

    [SerializeField] private int startingMoney = 500;
    [SerializeField] private int[] baitCounts = new int[3];
    
    [Header("UI Elements")]
    [SerializeField] private RectTransform moneyUIElement;
    [SerializeField] private CanvasGroup moneyUIGroup;
    [SerializeField] private CanvasGroup globalBaitsUIGroup;

    [Header("Death Sequence")]
    [SerializeField] private CanvasGroup fadeScreen;
    [SerializeField] private Transform playerTransform;

    [Header("Teleport Consumable")]
    [SerializeField] private int teleportConsumableCount = 5;
    [SerializeField] private TextMeshProUGUI teleportConsumableText;
    [SerializeField] private Vector3 teleportPosition = new Vector3(11f, 0f, 7f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Store store = Store.Instance != null ? Store.Instance : FindFirstObjectByType<Store>();
        if (store != null) store.OnChiselKitBought.AddListener(OnChiselKitBought);
    }

    private void Start()
    {
        // SetState (not a direct assignment) so the HUD hides and listeners like PlayerVisibility hear about Menu
        SetState(PlayerState.Menu);
        // Snap the HUD hidden instead of letting it fade out over the menu
        if (moneyUIGroup != null) moneyUIGroup.DOKill(complete: true);
        if (globalBaitsUIGroup != null) globalBaitsUIGroup.DOKill(complete: true);
        Money = startingMoney;
        OnMoneyChanged?.Invoke(Money);
        UpdateTeleportText();
        
        if (fadeScreen != null)
        {
            fadeScreen.alpha = 0f;
            fadeScreen.blocksRaycasts = false;
        }
        if (FindFirstObjectByType<MainMenu>() == null)
        {
            Debug.LogWarning("PlayerStateManager: no active MainMenu in the scene, starting the game without it.");
            SetState(PlayerState.Regular);
        }
    }

    public void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        
        bool showHUD = (CurrentState == PlayerState.Regular || CurrentState == PlayerState.Shopping);
        float targetAlpha = showHUD ? 1f : 0f;
        
        if (moneyUIGroup != null)
        {
            moneyUIGroup.DOFade(targetAlpha, 0.25f);
            moneyUIGroup.blocksRaycasts = showHUD;
        }
        
        if (globalBaitsUIGroup != null)
        {
            globalBaitsUIGroup.DOFade(targetAlpha, 0.25f);
            globalBaitsUIGroup.blocksRaycasts = showHUD;
        }

        OnStateChanged?.Invoke(CurrentState);
    }

    public void UseTeleport()
    {
        if (CurrentState != PlayerState.Regular || teleportConsumableCount <= 0) return;

        teleportConsumableCount--;
        UpdateTeleportText();

        TeleportPlayerAndBall(teleportPosition);
    }

    public void AddTeleport(int amount)
    {
        if (amount <= 0) return;
        teleportConsumableCount += amount;
        UpdateTeleportText();
    }

    private void UpdateTeleportText()
    {
        if (teleportConsumableText != null)
        {
            teleportConsumableText.text = teleportConsumableCount.ToString();
        }
    }

    public void Die()
    {
        if (CurrentState == PlayerState.Dead) return;
        SetState(PlayerState.Dead);
        OnPlayerDeath?.Invoke();
        if (fadeScreen == null || playerTransform == null)
        {
            // Fallback: never leave the Player stuck in Dead when the fade is not wired up
            Debug.LogWarning("PlayerStateManager: fadeScreen or playerTransform is not assigned, respawning without a fade.");
            TeleportPlayerAndBall(Vector3.zero);
            DOVirtual.DelayedCall(1f, () => SetState(PlayerState.Regular));
            return;
        }
        fadeScreen.blocksRaycasts = true;
        Sequence deathSequence = DOTween.Sequence();
        deathSequence.Append(fadeScreen.DOFade(1f, 0.5f));
        deathSequence.AppendInterval(1f);
        // The screen is black here, so the teleport is not visible
        deathSequence.AppendCallback(() => TeleportPlayerAndBall(Vector3.zero));
        deathSequence.Append(fadeScreen.DOFade(0f, 0.5f));
        deathSequence.AppendCallback(() =>
        {
            fadeScreen.blocksRaycasts = false;
            SetState(PlayerState.Regular);
        });
    }

    private void TeleportPlayerAndBall(Vector3 position)
    {
        if (playerTransform != null)
        {
            // Move the Rigidbody too, otherwise its velocity and interpolation fight the teleport
            Rigidbody playerBody = playerTransform.GetComponent<Rigidbody>();
            if (playerBody != null)
            {
                playerBody.position = position;
                playerBody.linearVelocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
            }
            playerTransform.position = position;
        }
        // The Ball is gone after the 5th Chisel Kit
        BallFollower ball = FindFirstObjectByType<BallFollower>();
        if (ball != null) ball.TeleportTo(position + Vector3.back * 1f);
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
        ShakeMoneyUI();
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0 || Money < amount) return false;
        Money -= amount;
        OnMoneyChanged?.Invoke(Money);
        ShakeMoneyUI();
        return true;
    }

    private void ShakeMoneyUI()
    {
        if (moneyUIElement != null)
        {
            moneyUIElement.DOKill(complete: true);
            moneyUIElement.DOShakeAnchorPos(0.3f, new Vector2(15f, 0f), 15, 90, false, true);
        }
    }

    public void AddBait(int baitType, int amount)
    {
        if (baitType < 0 || baitType >= baitCounts.Length || amount <= 0) return;
        baitCounts[baitType] += amount;
        OnBaitInventoryChanged?.Invoke(baitType, baitCounts[baitType]);
    }

    public bool SpendBait(int baitType)
    {
        if (baitType < 0 || baitType >= baitCounts.Length || baitCounts[baitType] <= 0) return false;
        baitCounts[baitType]--;
        OnBaitInventoryChanged?.Invoke(baitType, baitCounts[baitType]);
        return true;
    }

    public int GetBaitCount(int baitType)
    {
        if (baitType < 0 || baitType >= baitCounts.Length) return 0;
        return baitCounts[baitType];
    }

    private void OnChiselKitBought()
    {
        SetState(PlayerState.Regular);
    }
}