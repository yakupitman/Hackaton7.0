using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class Lift : Building
{
    [SerializeField] private CinemachineCamera liftCamera;
    [SerializeField] private Transform liftLever;
    [SerializeField] private Transform liftPlatform;
    [SerializeField] private float liftHeight = 10f;
    [SerializeField] private float liftDuration = 2f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private GameObject raycastBlocker;

    private Camera mainCamera;
    private bool isInteracting;
    private bool isLifting;
    private float startY;
    private Sequence liftSequence;

    private void Start()
    {
        mainCamera = Camera.main;
        startY = liftPlatform.position.y;
        raycastBlocker.SetActive(false);
    }

    private void Update()
    {
        if (isInteracting && !isLifting) HandleLeverClick();
    }

    public override void BeginInteraction()
    {
        base.BeginInteraction();
        liftCamera.enabled = true;
        isInteracting = true;
    }

    public override void EndInteraction()
    {
        base.EndInteraction();
        liftCamera.enabled = false;
        isInteracting = false;
    }

    private void HandleLeverClick()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            if (hit.collider.transform == liftLever) ActivateLift();
        }
    }

    private void ActivateLift()
    {
        isLifting = true;
        raycastBlocker.SetActive(true);
        liftLever.DOKill();
        liftSequence?.Kill();
        // The lever shake stays on the normal update so it looks smooth
        liftLever.DOShakeRotation(0.3f, new Vector3(0f, 0f, 5f), 30, 90f);
        // The platform moves in physics steps so LiftPlatform can carry Prey with it
        liftSequence = DOTween.Sequence().SetUpdate(UpdateType.Fixed);
        liftSequence.AppendInterval(0.3f);
        liftSequence.Append(liftPlatform.DOMoveY(startY + liftHeight, liftDuration).SetEase(Ease.InOutQuad));
        liftSequence.Append(liftPlatform.DOMoveY(startY, liftDuration).SetEase(Ease.InOutQuad));
        liftSequence.OnComplete(() =>
        {
            isLifting = false;
            raycastBlocker.SetActive(false);
        }); 
    }
}