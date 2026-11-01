using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;
using DG.Tweening;

public class Store : Building
{
    public static Store Instance { get; private set; }

    public UnityEvent OnChiselKitBought;

    [SerializeField] private CinemachineCamera storeCamera;
    [SerializeField] private GameObject storeInterface;
    [SerializeField] private int bait0Price = 10;
    [SerializeField] private int bait1Price = 20;
    [SerializeField] private int bait2Price = 30;
    [SerializeField] private int[] chiselKitPrices = { 50, 120, 250, 450, 800 };
    [SerializeField] private TextMeshProUGUI chiselKitPriceText;
    [SerializeField] private Button chiselKitButton;
    [SerializeField] private int teleportPrice = 40;
    [SerializeField] private Button[] baitButtons = new Button[3];
    [SerializeField] private Button teleportButton;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private float originalY;
    private int chiselKitsBought;
    private bool kitAllowed = true;
    private bool bait0Allowed = true;
    private bool othersAllowed = true;

    public int ChiselKitsBought => chiselKitsBought;

    // Price of the next Chisel Kit, or -1 when all are bought
    public int NextChiselKitPrice => chiselKitsBought < chiselKitPrices.Length ? chiselKitPrices[chiselKitsBought] : -1;

    private void Awake()
    {
        Instance = this;
        // BallFollower and PlayerMovement both cap at 5 Kits
        if (chiselKitPrices.Length != 5) Debug.LogWarning("Store: Chisel Kit Prices should have exactly 5 entries.");
        if (storeInterface != null)
        {
            rectTransform = storeInterface.GetComponent<RectTransform>();
            if (rectTransform != null) originalY = rectTransform.anchoredPosition.y;
            canvasGroup = storeInterface.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = storeInterface.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        RefreshChiselKitUI();
    }

    public override void BeginInteraction()
    {
        PlayerStateManager.Instance.SetState(PlayerState.Shopping);
        base.BeginInteraction();
        storeCamera.enabled = true;
        
        if (storeInterface != null && canvasGroup != null && rectTransform != null)
        {
            rectTransform.DOKill();
            canvasGroup.DOKill();
            storeInterface.SetActive(true);
            canvasGroup.alpha = 0f;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, originalY - 50f);
            canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutQuad);
            rectTransform.DOAnchorPosY(originalY, 0.25f).SetEase(Ease.OutQuad);
        }
    }

    public override void EndInteraction()
    {
        PlayerStateManager.Instance.SetState(PlayerState.Regular);
        base.EndInteraction();
        storeCamera.enabled = false;
        
        if (storeInterface != null && canvasGroup != null && rectTransform != null)
        {
            rectTransform.DOKill();
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InQuad);
            rectTransform.DOAnchorPosY(originalY - 50f, 0.25f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                storeInterface.SetActive(false);
            });
        }
        else if (storeInterface != null) 
        {
            storeInterface.SetActive(false);
        }
    }

    public void BuyBait(int baitType)
    {
        // Guidelines lock: tier 0 has its own flag, tiers 1 and 2 count as others
        if (baitType == 0 ? !bait0Allowed : !othersAllowed) return;
        int price = baitType switch
        {
            0 => bait0Price,
            1 => bait1Price,
            2 => bait2Price,
            _ => bait0Price
        };
        
        if (PlayerStateManager.Instance.SpendMoney(price))
        {
            PlayerStateManager.Instance.AddBait(baitType, 1);
        }
    }

    public void BuyChiselKit()
    {
        int price = NextChiselKitPrice;
        // Sold out or locked by the Guidelines: do nothing
        if (price < 0 || !kitAllowed) return;
        if (PlayerStateManager.Instance.SpendMoney(price))
        {
            chiselKitsBought++;
            RefreshChiselKitUI();
            OnChiselKitBought?.Invoke();
        }
        EndInteraction();
    }

    public void BuyTeleportation()
    {
        if (!othersAllowed) return;
        if (PlayerStateManager.Instance.SpendMoney(teleportPrice))
        {
            PlayerStateManager.Instance.AddTeleport(1);
        }
    }

    // Each flag says whether that item can be bought; the Buy methods check them too, not only the buttons
    public void SetTutorialLock(bool kit, bool bait0, bool others)
    {
        kitAllowed = kit;
        bait0Allowed = bait0;
        othersAllowed = others;
        RefreshChiselKitUI();
        for (int i = 0; i < baitButtons.Length; i++)
        {
            if (baitButtons[i] != null) baitButtons[i].interactable = i == 0 ? bait0Allowed : othersAllowed;
        }
        if (teleportButton != null) teleportButton.interactable = othersAllowed;
    }

    public void ClearTutorialLock()
    {
        SetTutorialLock(true, true, true);
    }

    private void RefreshChiselKitUI()
    {
        int price = NextChiselKitPrice;
        if (chiselKitPriceText != null) chiselKitPriceText.text = price < 0 ? "Sold out" : price.ToString();
        // Sold out wins over the Guidelines lock: clearing the lock must not re-enable a sold-out Kit
        if (chiselKitButton != null) chiselKitButton.interactable = price >= 0 && kitAllowed;
    }
}
