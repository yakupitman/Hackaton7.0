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

    private void Start()
    {
        mainCamera = Camera.main;
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
            // Verifies if the raycast specifically hit the assigned lever collider before executing the lift sequence
            if (hit.collider.transform == liftLever) ActivateLift();
        }
    }

    private void ActivateLift()
    {
        isLifting = true;
        raycastBlocker.SetActive(true);
        Sequence liftSequence = DOTween.Sequence();
        // Applies a short shake animation to the lever for tactile visual feedback, then smoothly moves the platform up
        liftSequence.Append(liftLever.DOShakeRotation(0.3f, new Vector3(0f, 0f, 5f), 30, 90f));
        liftSequence.Append(liftPlatform.DOMoveY(liftPlatform.position.y + liftHeight, liftDuration).SetEase(Ease.InOutQuad));
        liftSequence.Append(liftPlatform.DOMoveY(0.4125f, liftDuration).SetEase(Ease.InOutQuad));
        liftSequence.OnComplete(() =>
        {
            isLifting = false;
            raycastBlocker.SetActive(false);
        }); 
    }
}