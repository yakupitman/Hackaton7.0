using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class HoldManager : MonoBehaviour
{
    public static HoldManager Instance { get; private set; }

    public UnityEvent<GameObject> OnItemGrabbed;
    public UnityEvent<GameObject> OnItemDropped;

    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float liftHeight = 1f;
    [SerializeField] private float baseDragSpeed = 15f;

    private Camera mainCamera;
    private Rigidbody heldItem;
    private InteractableObject heldInteractable;
    private bool wasItemKinematic;

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
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (heldItem != null)
        {
            DragItem();
        }
    }

    private void HandleInput()
    {
        if (Mouse.current == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame && PlayerStateManager.Instance.CurrentState == PlayerState.Regular)
        {
            TryGrabItem();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame && heldItem != null)
        {
            DropItem();
        }
    }

    private void TryGrabItem()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            heldItem = hit.collider.GetComponent<Rigidbody>();
            if (heldItem != null)
            {
                heldInteractable = hit.collider.GetComponent<InteractableObject>();
                heldInteractable.SetHoldState(true);
                wasItemKinematic = heldItem.isKinematic;
                heldItem.isKinematic = true;
                OnItemGrabbed?.Invoke(heldItem.gameObject);
            }
        }
    }

    private void DragItem()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector3 targetPosition = new Vector3(hit.point.x, hit.point.y + liftHeight, hit.point.z);
            // Calculates dynamic speed simulating drag based on the item weight, preventing division by zero
            float itemWeight = heldInteractable != null ? Mathf.Max(heldInteractable.Weight, 0.1f) : 1f;
            float currentSpeed = baseDragSpeed / itemWeight;
            heldItem.MovePosition(Vector3.Lerp(heldItem.position, targetPosition, Time.fixedDeltaTime * currentSpeed));
        }
    }

    private void DropItem()
    {
        heldItem.isKinematic = wasItemKinematic;
        OnItemDropped?.Invoke(heldItem.gameObject);
        heldItem = null;
        heldInteractable.SetHoldState(false);
        heldInteractable = null;
    }
}