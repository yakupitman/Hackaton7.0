using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Global UI")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI[] globalBaitTexts = new TextMeshProUGUI[3];

    [Header("Minigame UI")]
    [SerializeField] private TextMeshProUGUI[] minigameBaitTexts = new TextMeshProUGUI[3];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.OnBaitInventoryChanged.AddListener(UpdateBaitUI);
            PlayerStateManager.Instance.OnMoneyChanged.AddListener(UpdateMoneyUI);
            InitializeUI();
        }
    }

    private void OnDestroy()
    {
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.OnBaitInventoryChanged.RemoveListener(UpdateBaitUI);
            PlayerStateManager.Instance.OnMoneyChanged.RemoveListener(UpdateMoneyUI);
        }
    }

    private void InitializeUI()
    {
        UpdateMoneyUI(PlayerStateManager.Instance.Money);
        for (int i = 0; i < 3; i++)
        {
            UpdateBaitUI(i, PlayerStateManager.Instance.GetBaitCount(i));
        }
    }

    private void UpdateBaitUI(int baitType, int amount)
    {
        if (baitType < 0 || baitType > 2) return;
        // Updates both global and minigame UI text elements synchronously
        if (globalBaitTexts[baitType] != null) globalBaitTexts[baitType].text = amount.ToString();
        if (minigameBaitTexts[baitType] != null) minigameBaitTexts[baitType].text = amount.ToString();
    }

    private void UpdateMoneyUI(int amount)
    {
        if (moneyText != null) moneyText.text = amount.ToString();
    }
}