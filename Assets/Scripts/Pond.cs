using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

[System.Serializable]
public struct PreyProfile
{
    public GameObject preyPrefab;
    public float minigameDuration;
    public float baseSpeed;
    public int[] availablePersonalities;
}

public class Pond : Building
{
    [SerializeField] private CinemachineCamera pondCamera;
    [SerializeField] private GameObject availableBaits;
    [Header("Catching Minigame")]
    [SerializeField] private GameObject catchingMinigame;
    [SerializeField] private RectTransform bar;
    [SerializeField] private RectTransform area;
    [SerializeField] private RectTransform needle;
    [SerializeField] private float needleSpeed = 300f;
    [Header("Prey Spawn Settings")]
    [SerializeField] private PreyProfile[] preyProfiles;
    [SerializeField] private GameObject[] preyVisualList;
    [SerializeField] private float[] bait0Probabilities = { 60f, 30f, 10f };
    [SerializeField] private float[] bait1Probabilities = { 20f, 50f, 30f };
    [SerializeField] private float[] bait2Probabilities = { 10f, 30f, 60f };
    [SerializeField] private Transform preyVisualsSpawnPoint;
    
    private bool isMinigameActive;
    private bool isWaitingForBite;
    private float successTimer;
    private float minigameTimer;
    private float biteTimer;
    private float biteWaitDuration;
    private float currentMinigameDuration;
    private float targetSuccessDuration;
    private float areaDirection = 1f;
    private float currentAreaSpeed;
    private int currentPersonality;
    private float personalityTimer;
    private float barHalf;
    private float areaHalf;
    private float needleHalf;
    private int currentBaitType;
    private int currentPreyIndex;
    private GameObject currentPreyVisual;

    private CanvasGroup baitsGroup;
    private RectTransform baitsRect;
    private float baitsOrigY;
    private CanvasGroup minigameGroup;
    private RectTransform minigameRect;
    private float minigameOrigY;
    
    private float targetAreaX;
    private float targetNeedleX;

    private void Awake()
    {
        SetupUIElement(availableBaits, out baitsGroup, out baitsRect, out baitsOrigY);
        SetupUIElement(catchingMinigame, out minigameGroup, out minigameRect, out minigameOrigY);
    }

    private void SetupUIElement(GameObject uiObject, out CanvasGroup group, out RectTransform rect, out float origY)
    {
        rect = uiObject.GetComponent<RectTransform>();
        origY = rect != null ? rect.anchoredPosition.y : 0f;
        group = uiObject.GetComponent<CanvasGroup>();
        if (group == null) group = uiObject.AddComponent<CanvasGroup>();
    }

    private void ShowUI(GameObject uiObject, CanvasGroup group, RectTransform rect, float origY)
    {
        if (uiObject == null || group == null || rect == null) return;
        rect.DOKill();
        group.DOKill();
        uiObject.SetActive(true);
        group.alpha = 0f;
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, origY - 50f);
        group.DOFade(1f, 0.25f).SetEase(Ease.OutQuad);
        rect.DOAnchorPosY(origY, 0.25f).SetEase(Ease.OutQuad);
    }

    private void HideUI(GameObject uiObject, CanvasGroup group, RectTransform rect, float origY)
    {
        if (uiObject == null || group == null || rect == null) return;
        rect.DOKill();
        group.DOKill();
        group.DOFade(0f, 0.25f).SetEase(Ease.InQuad);
        rect.DOAnchorPosY(origY - 50f, 0.25f).SetEase(Ease.InQuad).OnComplete(() => uiObject.SetActive(false));
    }
    
    public override void BeginInteraction()
    {
        base.BeginInteraction();
        pondCamera.enabled = true;
        ShowUI(availableBaits, baitsGroup, baitsRect, baitsOrigY);
    }

    public override void EndInteraction()
    {
        base.EndInteraction();
        StopMinigame();
        pondCamera.enabled = false;
        HideUI(availableBaits, baitsGroup, baitsRect, baitsOrigY);
        isWaitingForBite = false;
    }

    public void SelectBait(int baitType)
    {
        if (!PlayerStateManager.Instance.SpendBait(baitType)) return;
        HideUI(availableBaits, baitsGroup, baitsRect, baitsOrigY);
        currentBaitType = baitType;
        switch (baitType)
        {
            case 0:
                biteWaitDuration = Random.Range(4f, 6f);
                break;
            case 1:
                biteWaitDuration = Random.Range(3f, 5f);
                break;
            case 2:
                biteWaitDuration = Random.Range(2f, 4f);
                break;
            default:
                biteWaitDuration = Random.Range(4f, 6f);
                break;
        }
        biteTimer = 0f;
        isWaitingForBite = true;
    }

    private void DeterminePrey()
    {
        float[] currentProbabilities = currentBaitType switch
        {
            0 => bait0Probabilities,
            1 => bait1Probabilities,
            2 => bait2Probabilities,
            _ => bait0Probabilities
        };
        float randomValue = Random.Range(0f, 100f);
        float cumulativeProbability = 0f;
        currentPreyIndex = 0;
        for (int i = 0; i < currentProbabilities.Length; i++)
        {
            cumulativeProbability += currentProbabilities[i];
            if (randomValue <= cumulativeProbability)
            {
                currentPreyIndex = i;
                break;
            }
        }
        if (currentPreyIndex < preyProfiles.Length)
        {
            PreyProfile profile = preyProfiles[currentPreyIndex];
            if (profile.availablePersonalities != null && profile.availablePersonalities.Length > 0)
            {
                int randomPersonalityIndex = Random.Range(0, profile.availablePersonalities.Length);
                currentPersonality = profile.availablePersonalities[randomPersonalityIndex];
            }
            else currentPersonality = 0;
            currentAreaSpeed = profile.baseSpeed;
        }
    }

    private void StartMinigame()
    {
        ShowUI(catchingMinigame, minigameGroup, minigameRect, minigameOrigY);
        isMinigameActive = true;
        successTimer = 0f;
        minigameTimer = 0f;
        personalityTimer = 0f;
        currentMinigameDuration = currentPreyIndex < preyProfiles.Length ? preyProfiles[currentPreyIndex].minigameDuration : 5f;
        targetSuccessDuration = Mathf.Max(0f, currentMinigameDuration - 2.5f);
        barHalf = bar.sizeDelta.x * 0.5f;
        areaHalf = area.sizeDelta.x * 0.5f;
        needleHalf = needle.sizeDelta.x * 0.5f;
        areaDirection = Random.value > 0.5f ? 1f : -1f;
        float areaLimit = barHalf - areaHalf;
        targetAreaX = Random.Range(-areaLimit, areaLimit);
        area.anchoredPosition = new Vector2(targetAreaX, 0f);
        float needleLimit = barHalf - needleHalf;
        targetNeedleX = Random.Range(-needleLimit, needleLimit);
        needle.anchoredPosition = new Vector2(targetNeedleX, 0f);
        if (currentPreyIndex < preyVisualList.Length && preyVisualList[currentPreyIndex] != null)
        {
            currentPreyVisual = Instantiate(preyVisualList[currentPreyIndex], preyVisualsSpawnPoint.position, Quaternion.identity);
            currentPreyVisual.transform.DOMove(pondCamera.transform.position, currentMinigameDuration).SetEase(Ease.Linear);
        }
    }

    private void StopMinigame()
    {
        isMinigameActive = false;
        HideUI(catchingMinigame, minigameGroup, minigameRect, minigameOrigY);
        if (currentPreyVisual != null)
        {
            currentPreyVisual.transform.DOKill();
            Destroy(currentPreyVisual);
        }
    }
    
    private void Update()
    {
        if (isWaitingForBite) WaitForBite();
        else if (isMinigameActive)
        {
            UpdatePersonality();
            MoveArea();
            MoveNeedle();
            CheckMinigameState();
        }
    }

    private void WaitForBite()
    {
        biteTimer += Time.deltaTime;
        if (biteTimer >= biteWaitDuration)
        {
            isWaitingForBite = false;
            DeterminePrey();
            StartMinigame();
        }
    }

    private void UpdatePersonality()
    {
        personalityTimer -= Time.deltaTime;
        if (personalityTimer > 0f) return;
        float baseSpeed = currentPreyIndex < preyProfiles.Length ? preyProfiles[currentPreyIndex].baseSpeed : 80f;
        switch (currentPersonality)
        {
            case 0:
                currentAreaSpeed = baseSpeed;
                personalityTimer = 999f;
                break;
            case 1:
                areaDirection = Random.value > 0.5f ? 1f : -1f;
                currentAreaSpeed = baseSpeed * Random.Range(0.8f, 1.2f);
                personalityTimer = Random.Range(1f, 2f);
                break;
            case 2:
                if (currentAreaSpeed > 0f)
                {
                    currentAreaSpeed = 0f;
                    personalityTimer = Random.Range(0.5f, 1f);
                }
                else
                {
                    areaDirection = Random.value > 0.5f ? 1f : -1f;
                    currentAreaSpeed = baseSpeed;
                    personalityTimer = Random.Range(1f, 2f);
                }
                break;
            case 3:
                areaDirection = Random.value > 0.5f ? 1f : -1f;
                currentAreaSpeed = baseSpeed * 1.3f;
                personalityTimer = Random.Range(0.3f, 0.7f);
                break;
            case 4:
                areaDirection = Random.value > 0.5f ? 1f : -1f;
                currentAreaSpeed = baseSpeed * 2f;
                personalityTimer = Random.Range(0.2f, 0.4f);
                break;
            case 5:
                areaDirection = Random.value > 0.5f ? 1f : -1f;
                currentAreaSpeed = baseSpeed * Random.Range(0.5f, 1.8f);
                personalityTimer = Random.Range(0.4f, 1f);
                break;
        }
    }
    
    private void MoveArea()
    {
        float limit = barHalf - areaHalf;
        targetAreaX += areaDirection * currentAreaSpeed * Time.deltaTime;
        if (Mathf.Abs(targetAreaX) >= limit)
        {
            targetAreaX = Mathf.Sign(targetAreaX) * limit;
            areaDirection *= -1f;
        }
        Vector2 pos = area.anchoredPosition;
        pos.x = Mathf.Lerp(pos.x, targetAreaX, Time.deltaTime * 5f);
        area.anchoredPosition = pos;
    }

    private void MoveNeedle()
    {
        float input = InputManager.Instance.MoveDirection.x;
        targetNeedleX += input * needleSpeed * Time.deltaTime;
        targetNeedleX = Mathf.Clamp(targetNeedleX, -(barHalf - needleHalf), barHalf - needleHalf);
        Vector2 pos = needle.anchoredPosition;
        pos.x = Mathf.Lerp(pos.x, targetNeedleX, Time.deltaTime * 10f);
        needle.anchoredPosition = pos;
    }

    private void CheckMinigameState()
    {
        minigameTimer += Time.deltaTime;
        if (IsNeedleInsideArea()) successTimer += Time.deltaTime;
        if (successTimer >= targetSuccessDuration) CompleteMinigame(true);
        else if (minigameTimer >= currentMinigameDuration) CompleteMinigame(false);
    }

    private bool IsNeedleInsideArea()
    {
        float delta = Mathf.Abs(needle.anchoredPosition.x - area.anchoredPosition.x);
        return delta <= (areaHalf - needleHalf);
    }

    private void CompleteMinigame(bool isSuccess)
    {
        StopMinigame();
        pondCamera.enabled = false;
        PlayerStateManager.Instance.SetState(PlayerState.Regular);
        if (isSuccess) SpawnPrey();
    }

    private void SpawnPrey()
    {
        if (currentPreyIndex < preyProfiles.Length)
        {
            GameObject prefab = preyProfiles[currentPreyIndex].preyPrefab;
            if (prefab != null)
            {
                GameObject prey = Instantiate(prefab, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 2f, gameObject.transform.position.z), Quaternion.identity);
                Rigidbody rb = prey.GetComponent<Rigidbody>();
                float throwForce = Random.Range(3f, 7f);
                rb.AddForce(-Vector3.forward * throwForce, ForceMode.Impulse);
            }
        }
    }
}