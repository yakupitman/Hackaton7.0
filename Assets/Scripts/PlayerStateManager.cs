using UnityEngine;
using UnityEngine.Events;

public enum PlayerState
{
    Regular,
    Catching,
    Interacting
}

public class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager Instance { get; private set; }

    public UnityEvent<PlayerState> OnStateChanged;
    public UnityEvent<int> OnMoneyChanged;
    public UnityEvent<int, int> OnBaitInventoryChanged;

    public PlayerState CurrentState { get; private set; }
    public int Money { get; private set; }

    [SerializeField] private int[] baitCounts = new int[3];

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
        if (store != null) store.OnChiselKitBought.AddListener(OnChiselKitBought);    }

    private void Start()
    {
        CurrentState = PlayerState.Regular;
        Money = 500;
        OnMoneyChanged?.Invoke(Money);
        
    }

    private void Update()
    {
        CheckCatchingExit();
    }

    private void CheckCatchingExit()
    {
        if (CurrentState == PlayerState.Catching && InputManager.Instance.Interact)
        {
            SetState(PlayerState.Regular);
        }
    }

    public void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0 || Money < amount) return false;
        Money -= amount;
        OnMoneyChanged?.Invoke(Money);
        return true;
    }

    // Manages bait inventory and notifies UI elements to synchronize their display
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
        CurrentState =  PlayerState.Regular;
    }
}