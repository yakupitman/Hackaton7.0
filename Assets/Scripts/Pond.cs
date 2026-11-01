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

public enum FlashlightAxis
{
    Y,
    Z
}

public class Pond : Building
{
    [SerializeField] private CinemachineCamera pondCamera;
    [SerializeField] private CinemachineCamera pondCamera2;
    [SerializeField] private Animator pondVisualAnimator;
    [SerializeField] private GameObject availableBaits;

    [Header("Catching Minigame")]
    [SerializeField] private GameObject catchingMinigame;
    [SerializeField] private RectTransform bar;
    [SerializeField] private RectTransform area;
    [SerializeField] private RectTransform needle;
    [SerializeField] private float needleSpeed = 300f;
    [SerializeField] private float cameraShakingIntensityMultiplier;

    [Header("Prey Spawn Settings")]
    [SerializeField] private PreyProfile[] preyProfiles;
    [SerializeField] private GameObject[] preyVisualList;
    [SerializeField] private float[] bait0Probabilities = { 60f, 30f, 10f };
    [SerializeField] private float[] bait1Probabilities = { 20f, 50f, 30f };
    [SerializeField] private float[] bait2Probabilities = { 10f, 30f, 60f };
    [SerializeField] private Transform preyVisualsSpawnPoint;

    [Header("Flashlight Settings")]
    [SerializeField] private GameObject flashlight;
    [SerializeField] private FlashlightAxis flashlightRotationAxis = FlashlightAxis.Y;

    [Header("Effects")]
    [SerializeField] private ParticleSystem incenseParticles;
    [SerializeField] private float incenseDuration = 2f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource mainAudioSource;
    [SerializeField] private AudioSource ambientAudioSource;
    [SerializeField] private AudioClip doorSqueakClip;
    [SerializeField] private AudioClip incenseAromaClip;
    [SerializeField] private AudioClip intenseAmbientClip;

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
    private int preyAnimType;
    private CinemachineBasicMultiChannelPerlin cameraNoise;
    private CanvasGroup baitsGroup;
    private RectTransform baitsRect;
    private float baitsOrigY;
    private CanvasGroup minigameGroup;
    private RectTransform minigameRect;
    private float minigameOrigY;
    private float targetAreaX;
    private float targetNeedleX;
    private float areaLimit;
    private Quaternion initialFlashlightRotation;
    private static readonly int OpenedHash = Animator.StringToHash("opened");
    private Tween particleWaitTween;

    private void Awake()
    {
        SetupUIElement(availableBaits, out baitsGroup, out baitsRect, out baitsOrigY);
        SetupUIElement(catchingMinigame, out minigameGroup, out minigameRect, out minigameOrigY);
        
        if (pondCamera2 != null)
        {
            cameraNoise = pondCamera2.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
        if (flashlight == null)
        {
            Transform flashlightChild = transform.Find("Flashlight");
            if (flashlightChild != null) flashlight = flashlightChild.gameObject;
        }
        if (flashlight != null)
        {
            initialFlashlightRotation = flashlight.transform.localRotation;
            flashlight.SetActive(false);
        }
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
        if (pondCamera2 != null) pondCamera2.enabled = false;
        ShowUI(availableBaits, baitsGroup, baitsRect, baitsOrigY);
    }

    public override void EndInteraction()
    {
        base.EndInteraction();
        StopMinigame();
        pondCamera.enabled = false;
        if (pondCamera2 != null) pondCamera2.enabled = false;
        particleWaitTween?.Kill();
        if (incenseParticles != null)
        {
            incenseParticles.Stop();
            incenseParticles.Clear();
        }
        if (mainAudioSource != null) mainAudioSource.Stop();
        if (ambientAudioSource != null) ambientAudioSource.Stop();
        HideUI(availableBaits, baitsGroup, baitsRect, baitsOrigY);
        isWaitingForBite = false;
    }

    public void SelectBait(int baitType)
    {
        if (!PlayerStateManager.Instance.SpendBait(baitType)) return;
        HideUI(availableBaits, baitsGroup, baitsRect, baitsOrigY);
        pondCamera.enabled = false;
        if (pondCamera2 != null) pondCamera2.enabled = true;
        
        if (pondVisualAnimator != null)
        {
            pondVisualAnimator.SetTrigger(OpenedHash);
        }
        if (mainAudioSource != null && doorSqueakClip != null)
        {
            mainAudioSource.PlayOneShot(doorSqueakClip);
        }
        
        currentBaitType = baitType;
        Color baitColor = Color.white;
        
        switch (baitType)
        {
            case 0:
                biteWaitDuration = Random.Range(4f, 6f);
                baitColor = Color.blue;
                break;
            case 1:
                biteWaitDuration = Random.Range(3f, 5f);
                baitColor = Color.green;
                break;
            case 2:
                biteWaitDuration = Random.Range(2f, 4f);
                baitColor = new Color(0.5f, 0f, 0.5f);
                break;
            default:
                biteWaitDuration = Random.Range(4f, 6f);
                baitColor = Color.blue;
                break;
        }
        
        particleWaitTween?.Kill();
        particleWaitTween = DOTween.Sequence()
            .AppendInterval(2f)
            .AppendCallback(() => 
            {
                if (incenseParticles != null) 
                {
                    var mainModule = incenseParticles.main;
                    mainModule.startColor = new ParticleSystem.MinMaxGradient(baitColor);
                    incenseParticles.Play();
                }
                if (mainAudioSource != null && incenseAromaClip != null)
                {
                    mainAudioSource.PlayOneShot(incenseAromaClip);
                }
            })
            .AppendInterval(incenseDuration)
            .AppendCallback(() => 
            {
                DeterminePrey();
                biteTimer = 0f;
                isWaitingForBite = true;
            });
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
        areaLimit = barHalf - areaHalf;
        
        targetAreaX = Random.Range(-areaLimit, areaLimit);
        area.anchoredPosition = new Vector2(targetAreaX, 0f);
        
        float needleLimit = barHalf - needleHalf;
        targetNeedleX = Random.Range(-needleLimit, needleLimit);
        needle.anchoredPosition = new Vector2(targetNeedleX, 0f);
        
        preyAnimType = Random.Range(0, 3);
        if (currentPreyIndex < preyVisualList.Length && preyVisualList[currentPreyIndex] != null)
        {
            GameObject visualPrefab = preyVisualList[currentPreyIndex];
            currentPreyVisual = Instantiate(visualPrefab, preyVisualsSpawnPoint.position, visualPrefab.transform.rotation);
        }
        if (ambientAudioSource != null && intenseAmbientClip != null)
        {
            ambientAudioSource.clip = intenseAmbientClip;
            ambientAudioSource.loop = true;
            ambientAudioSource.volume = 0f;
            ambientAudioSource.Play();
        }
        if (flashlight != null)
        {
            flashlight.transform.localRotation = initialFlashlightRotation;
            flashlight.SetActive(true);
        }
    }

    private void StopMinigame()
    {
        isMinigameActive = false;
        HideUI(catchingMinigame, minigameGroup, minigameRect, minigameOrigY);
        
        if (cameraNoise != null) cameraNoise.AmplitudeGain = 0f;
        if (ambientAudioSource != null) ambientAudioSource.Stop();
        
        if (currentPreyVisual != null)
        {
            Destroy(currentPreyVisual);
        }
        if (flashlight != null)
        {
            flashlight.SetActive(false);
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
            UpdateFlashlightRotation();
            UpdatePreyVisualAndCamera();
            CheckMinigameState();
        }
    }

    private void UpdateFlashlightRotation()
    {
        if (flashlight == null) return;
        
        float maxNeedleX = barHalf - needleHalf;
        float normalizedNeedleX = maxNeedleX > 0f ? (targetNeedleX / maxNeedleX) : 0f;
        
        // Інвертоване обертання та жорстко заданий кут (30f)
        float targetAngle = normalizedNeedleX * 30f;
        
        Vector3 targetEuler = flashlightRotationAxis == FlashlightAxis.Z
            ? new Vector3(0f, 0f, targetAngle)
            : new Vector3(0f, targetAngle, 0f);
            
        Quaternion targetRotation = initialFlashlightRotation * Quaternion.Euler(targetEuler);
        
        // Жорстко задана швидкість обертання (3f)
        flashlight.transform.localRotation = Quaternion.Lerp(
            flashlight.transform.localRotation,
            targetRotation,
            3f * Time.deltaTime
        );
    }

    private void WaitForBite()
    {
        biteTimer += Time.deltaTime;
        if (biteTimer >= biteWaitDuration)
        {
            isWaitingForBite = false;
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
        targetAreaX += areaDirection * currentAreaSpeed * Time.deltaTime;
        if (Mathf.Abs(targetAreaX) >= areaLimit)
        {
            targetAreaX = Mathf.Sign(targetAreaX) * areaLimit;
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

    private void UpdatePreyVisualAndCamera()
    {
        if (currentPreyVisual == null || pondCamera2 == null) return;
        float progress = Mathf.Clamp01(minigameTimer / currentMinigameDuration);
        
        if (ambientAudioSource != null)
        {
            ambientAudioSource.volume = progress;
        }
        if (cameraNoise != null) cameraNoise.AmplitudeGain = Mathf.Lerp(0f, cameraShakingIntensityMultiplier, progress);
        
        Vector3 targetPos = pondCamera2.transform.position + pondCamera2.transform.forward * 1f;
        Vector3 basePos = Vector3.Lerp(preyVisualsSpawnPoint.position, targetPos, progress);
        float normalizedAreaX = areaLimit > 0 ? area.anchoredPosition.x / areaLimit : 0f;
        Vector3 mirrorOffset = pondCamera2.transform.right * (normalizedAreaX * 0.75f);
        Vector3 animOffset = Vector3.zero;
        
        switch (preyAnimType)
        {
            case 0:
                animOffset = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0f);
                break;
            case 1:
                animOffset = pondCamera2.transform.up * (Mathf.Sin(Time.time * 12f) * 0.1f);
                break;
            case 2:
                animOffset = pondCamera2.transform.right * (Mathf.Sin(Time.time * 4f) * 0.4f) + 
                             pondCamera2.transform.up * (Mathf.Sin(Time.time * 8f) * 0.2f);
                break;
        }
        currentPreyVisual.transform.position = basePos + mirrorOffset + animOffset;
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
        if (isSuccess)
        {
            pondCamera.enabled = false;
            if (pondCamera2 != null) pondCamera2.enabled = false;
            PlayerStateManager.Instance.SetState(PlayerState.Regular);
            SpawnPrey();
        }
        else
        {
            PlayerStateManager.Instance.Die();
            DOVirtual.DelayedCall(1.5f, () => 
            {
                pondCamera.enabled = false;
                if (pondCamera2 != null) pondCamera2.enabled = false;
            });
        }
    }

    private void SpawnPrey()
    {
        if (currentPreyIndex < preyProfiles.Length)
        {
            GameObject prefab = preyProfiles[currentPreyIndex].preyPrefab;
            if (prefab != null)
            {
                GameObject prey = Instantiate(prefab, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 2f, gameObject.transform.position.z), prefab.transform.rotation);
                Rigidbody rb = prey.GetComponent<Rigidbody>();
                float throwForce = Random.Range(3f, 7f);
                rb.AddForce(-Vector3.forward * throwForce, ForceMode.Impulse);
            }
        }
    }
}