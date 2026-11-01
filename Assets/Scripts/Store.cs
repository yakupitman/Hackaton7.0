using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
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
    [SerializeField] private int chieselKitPrice = 50;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private float originalY;

    private void Awake()
    {
        Instance = this;
        if (storeInterface != null)
        {
            rectTransform = storeInterface.GetComponent<RectTransform>();
            if (rectTransform != null) originalY = rectTransform.anchoredPosition.y;
            canvasGroup = storeInterface.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = storeInterface.AddComponent<CanvasGroup>();
        }
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
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, originalY + -50f);
            canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutQuad);
            rectTransform.DOAnchorPosY(originalY, 0.25f).SetEase(Ease.OutQuad);
        }
    }

    public override void EndInteraction()
    {
        base.EndInteraction();
        storeCamera.enabled = false;
        if (storeInterface != null && canvasGroup != null && rectTransform != null)
        {
            rectTransform.DOKill();
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InQuad);
            rectTransform.DOAnchorPosY(originalY + -50f, 0.25f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                storeInterface.SetActive(false);
            });
        }
        else if (storeInterface != null) storeInterface.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        InteractableObject item = other.GetComponent<InteractableObject>();
        if (item == null) return;
        // Processes the sale by adding the item's price to the player's balance and destroying the object
        PlayerStateManager.Instance.AddMoney(item.Price);
        Destroy(other.gameObject);
    }

    public void BuyBait(int baitType)
    {
        int price = baitType switch
        {
            0 => bait0Price,
            1 => bait1Price,
            2 => bait2Price,
            _ => bait0Price
        };
        // Deducts bait cost and adds one unit to player inventory
        if (PlayerStateManager.Instance.SpendMoney(price))
        {
            PlayerStateManager.Instance.AddBait(baitType, 1);
        }
    }

    public void BuyChiselKit()
    {
        // Deducts chisel kit price and notifies subscribed listeners
        if (PlayerStateManager.Instance.SpendMoney(chieselKitPrice)) OnChiselKitBought?.Invoke();
        EndInteraction();
    }
}
