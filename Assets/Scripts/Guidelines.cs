using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// Scene UI like MainMenu: WinScreen.Restart reloads the scene, so every listener added in Start is removed in OnDestroy
public class Guidelines : MonoBehaviour
{
    private enum Step
    {
        NotStarted,
        BuyChiselKit,
        BuyBait,
        UnlockPond,
        StartCatch,
        Catch,
        Sell,
        Escape,
        Hint,
        FreePlay
    }

    [SerializeField] private CanvasGroup panel;
    [SerializeField] private TextMeshProUGUI stepText;
    [SerializeField] private Button skipButton;
    [SerializeField] private string[] stepTexts =
    {
        "Buy your first Chisel Kit in the Store.",
        "Buy 3 of the cheapest Bait.",
        "Go to the right corner of the room and unlock the first Pond.",
        "Pick a Bait and start fishing.",
        "Catch it! Keep the needle inside the moving area.",
        "Carry the Prey to the Lift and send it up to sell it.",
        "Leave through the main door and run down the corridor."
    };
    [SerializeField] private string hintText = "Didn't make it? Try to lighten that heavy thing. Have fun!";
    [SerializeField] private float hintDuration = 15f;
    [SerializeField] private int baitGoal = 3;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private Store store;
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private EscapeSequence escapeSequence;

    private PlayerStateManager stateManager;
    private Pond[] ponds;
    private Step currentStep;
    private bool hasUnlockedPond;
    private bool isWaitingForRespawn;
    private int lastMoney;
    private Tween hintTween;

    private void Start()
    {
        if (store == null) store = Store.Instance != null ? Store.Instance : FindFirstObjectByType<Store>();
        if (mainMenu == null) mainMenu = FindFirstObjectByType<MainMenu>();
        if (escapeSequence == null) escapeSequence = FindFirstObjectByType<EscapeSequence>();
        stateManager = PlayerStateManager.Instance;
        ponds = FindObjectsByType<Pond>(FindObjectsSortMode.None);
        SetPanelVisible(false, true);
        if (stateManager == null || store == null)
        {
            Debug.LogWarning("Guidelines: no PlayerStateManager or Store in the scene, the Guidelines are off.", this);
            return;
        }
        lastMoney = stateManager.Money;
        stateManager.OnStateChanged.AddListener(OnStateChanged);
        stateManager.OnMoneyChanged.AddListener(OnMoneyChanged);
        stateManager.OnBaitInventoryChanged.AddListener(OnBaitInventoryChanged);
        store.OnChiselKitBought.AddListener(OnChiselKitBought);
        foreach (Pond pond in ponds)
        {
            pond.OnUnlocked.AddListener(OnPondUnlocked);
            pond.OnCatchStarted.AddListener(OnCatchStarted);
            pond.OnPreyCaught.AddListener(OnPreyCaught);
        }
        if (escapeSequence != null) escapeSequence.OnPlayerCaught.AddListener(OnPlayerCaught);
        if (skipButton != null) skipButton.onClick.AddListener(Skip);
        // Locked from the first frame, so nothing is bought out of order before the panel shows
        store.SetTutorialLock(true, false, false);
        if (mainMenu != null) mainMenu.OnGameStarted.AddListener(BeginGuidelines);
        else BeginGuidelines();
    }

    private void OnDestroy()
    {
        if (hintTween.IsActive()) hintTween.Kill();
        if (stateManager != null)
        {
            stateManager.OnStateChanged.RemoveListener(OnStateChanged);
            stateManager.OnMoneyChanged.RemoveListener(OnMoneyChanged);
            stateManager.OnBaitInventoryChanged.RemoveListener(OnBaitInventoryChanged);
        }
        if (store != null) store.OnChiselKitBought.RemoveListener(OnChiselKitBought);
        if (ponds != null)
        {
            foreach (Pond pond in ponds)
            {
                if (pond == null) continue;
                pond.OnUnlocked.RemoveListener(OnPondUnlocked);
                pond.OnCatchStarted.RemoveListener(OnCatchStarted);
                pond.OnPreyCaught.RemoveListener(OnPreyCaught);
            }
        }
        if (escapeSequence != null) escapeSequence.OnPlayerCaught.RemoveListener(OnPlayerCaught);
        if (skipButton != null) skipButton.onClick.RemoveListener(Skip);
        if (mainMenu != null) mainMenu.OnGameStarted.RemoveListener(BeginGuidelines);
    }

    // Free Play at once, then buys whatever the skipped steps would still have bought, paid from Money
    public void Skip()
    {
        if (!CanSkip()) return;
        Step skippedAt = currentStep;
        // Free Play first: it clears the Store lock, and the purchases below must not move the steps through the handlers
        EnterFreePlay();
        if (store.ChiselKitsBought == 0) store.BuyChiselKit();
        if (skippedAt <= Step.BuyBait)
        {
            int missingBait = baitGoal - stateManager.GetBaitCount(0);
            for (int i = 0; i < missingBait; i++) store.BuyBait(0);
        }
        if (!hasUnlockedPond)
        {
            Pond cheapest = FindCheapestLockedPond();
            if (cheapest != null && !cheapest.TryBuyUnlock()) Debug.LogWarning("Guidelines: not enough Money to unlock the first Pond on Skip.", this);
        }
        if (store.ChiselKitsBought == 0 || (skippedAt <= Step.BuyBait && stateManager.GetBaitCount(0) < baitGoal))
        {
            Debug.LogWarning("Guidelines: not enough Money to finish the Guidelines purchases on Skip.", this);
        }
    }

    private void BeginGuidelines()
    {
        if (currentStep != Step.NotStarted) return;
        SetPanelVisible(true, false);
        GoToStep(Step.BuyChiselKit);
    }

    private void GoToStep(Step step)
    {
        // A step that is already done (e.g. a Pond unlocked before step 3) is passed straight away
        while (IsStepDone(step)) step++;
        currentStep = step;
        switch (currentStep)
        {
            case Step.BuyChiselKit:
                store.SetTutorialLock(true, false, false);
                break;
            case Step.BuyBait:
                store.SetTutorialLock(false, true, false);
                break;
            case Step.Hint:
                store.ClearTutorialLock();
                break;
            default:
                store.SetTutorialLock(false, false, false);
                break;
        }
        if (stepText != null)
        {
            stepText.text = GetStepText(currentStep);
            stepText.rectTransform.DOKill(true);
            stepText.rectTransform.DOPunchScale(Vector3.one * 0.1f, 0.3f).SetLink(gameObject);
        }
        RefreshSkipButton();
    }

    private bool IsStepDone(Step step)
    {
        return step switch
        {
            Step.BuyChiselKit => store.ChiselKitsBought > 0,
            Step.BuyBait => stateManager.GetBaitCount(0) >= baitGoal,
            Step.UnlockPond => hasUnlockedPond,
            _ => false
        };
    }

    private string GetStepText(Step step)
    {
        if (step == Step.Hint) return hintText;
        int index = (int)step - 1;
        return index >= 0 && index < stepTexts.Length ? stepTexts[index] : "";
    }

    private void ShowHint()
    {
        GoToStep(Step.Hint);
        if (hintTween.IsActive()) hintTween.Kill();
        // Scaled time: the hint must not run out while the game is frozen, and Death and the Store fades are scaled too
        hintTween = DOVirtual.DelayedCall(hintDuration, EnterFreePlay).SetLink(gameObject);
    }

    private void EnterFreePlay()
    {
        if (hintTween.IsActive()) hintTween.Kill();
        hintTween = null;
        isWaitingForRespawn = false;
        currentStep = Step.FreePlay;
        store.ClearTutorialLock();
        SetPanelVisible(false, false);
        RefreshSkipButton();
    }

    private Pond FindCheapestLockedPond()
    {
        Pond cheapest = null;
        foreach (Pond pond in ponds)
        {
            if (pond == null || pond.IsUnlocked) continue;
            if (cheapest == null || pond.UnlockPrice < cheapest.UnlockPrice) cheapest = pond;
        }
        return cheapest;
    }

    private bool CanSkip()
    {
        if (currentStep == Step.NotStarted || currentStep == Step.FreePlay) return false;
        PlayerState state = stateManager.CurrentState;
        if (state == PlayerState.Menu) return false;
        // Buying the Kit forces Regular and ends the Store interaction, so it may only happen outside other Buildings and Death
        return store.ChiselKitsBought > 0 || state == PlayerState.Regular || state == PlayerState.Shopping;
    }

    private void RefreshSkipButton()
    {
        if (skipButton != null) skipButton.interactable = CanSkip();
    }

    private void SetPanelVisible(bool visible, bool instant)
    {
        if (panel == null) return;
        panel.DOKill();
        panel.interactable = visible;
        panel.blocksRaycasts = visible;
        if (instant) panel.alpha = visible ? 1f : 0f;
        else panel.DOFade(visible ? 1f : 0f, fadeDuration).SetLink(gameObject);
    }

    private void OnStateChanged(PlayerState state)
    {
        RefreshSkipButton();
        // OnPlayerCaught comes before Die, so the next Regular is the respawn after the Ghost's Death
        if (isWaitingForRespawn && state == PlayerState.Regular)
        {
            isWaitingForRespawn = false;
            ShowHint();
        }
    }

    private void OnMoneyChanged(int money)
    {
        // Spending also changes Money, so only a rise counts as a Sell
        bool isSale = money > lastMoney;
        lastMoney = money;
        if (currentStep == Step.Sell && isSale) GoToStep(Step.Escape);
    }

    private void OnBaitInventoryChanged(int baitType, int amount)
    {
        if (currentStep == Step.BuyBait && baitType == 0 && amount >= baitGoal) GoToStep(Step.UnlockPond);
    }

    private void OnChiselKitBought()
    {
        RefreshSkipButton();
        if (currentStep == Step.BuyChiselKit) GoToStep(Step.BuyBait);
    }

    private void OnPondUnlocked()
    {
        hasUnlockedPond = true;
        if (currentStep == Step.UnlockPond) GoToStep(Step.StartCatch);
    }

    private void OnCatchStarted()
    {
        if (currentStep == Step.StartCatch) GoToStep(Step.Catch);
    }

    private void OnPreyCaught()
    {
        // StartCatch too, so a Catch that began before step 4 still moves the Guidelines on
        if (currentStep == Step.StartCatch || currentStep == Step.Catch) GoToStep(Step.Sell);
    }

    private void OnPlayerCaught()
    {
        // Only the Ghost at step 7 counts; other Deaths and earlier Ghost catches leave the step as it is
        if (currentStep == Step.Escape) isWaitingForRespawn = true;
    }
}
