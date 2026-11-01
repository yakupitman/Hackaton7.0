using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float currentMoveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float interactionRange = 4f;
    
    [Header("Upgrades")]
    [SerializeField] private float[] speedIncreases = { 1.4f, 1.2f, 0.9f, 0.9f, 1.6f };

    private Rigidbody rigidBody;
    private Vector3 lastMoveDirection = Vector3.forward;
    private Building currentBuilding;
    private Building highlightedBuilding;
    private int speedUpgradeIndex = 0;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
    }

    private void Start()
    {
        Store store = Store.Instance != null ? Store.Instance : FindFirstObjectByType<Store>();
        if (store != null) store.OnChiselKitBought.AddListener(OnChiselKitBought);
    }

    private void OnDestroy()
    {
        Store store = Store.Instance != null ? Store.Instance : FindFirstObjectByType<Store>();
        if (store != null) store.OnChiselKitBought.RemoveListener(OnChiselKitBought);
    }

    private void Update()
    {
        TrackMoveDirection();
        UpdateUseIndicator();
        HandleInteractionInput();
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
    }

    public void OnChiselKitBought()
    {
        if (speedUpgradeIndex < speedIncreases.Length)
        {
            currentMoveSpeed += speedIncreases[speedUpgradeIndex];
            speedUpgradeIndex++;
        }
    }

    private void TrackMoveDirection()
    {
        Vector2 input = InputManager.Instance.MoveDirection;
        if (input.sqrMagnitude > 0.01f) lastMoveDirection = new Vector3(input.x, 0f, input.y).normalized;
    }

    private void HandleInteractionInput()
    {
        if (!InputManager.Instance.Interact) return;
        
        PlayerState state = PlayerStateManager.Instance.CurrentState;
        
        if (state == PlayerState.Regular) 
        {
            TryBeginInteraction();
        }
        // Додано перевірку на стан Shopping, щоб магазин можна було закрити кнопкою взаємодії
        else if (state == PlayerState.Interacting || state == PlayerState.Shopping) 
        {
            EndCurrentInteraction();
        }
    }

    private Building FindTargetBuilding()
    {
        Ray ray = new Ray(transform.position, lastMoveDirection);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange)) return null;
        if (!hit.collider.CompareTag("Building")) return null;
        return hit.collider.GetComponent<Building>();
    }

    private void UpdateUseIndicator()
    {
        // Only Regular state can start an interaction, so only then is a Building a target
        Building target = PlayerStateManager.Instance.CurrentState == PlayerState.Regular ? FindTargetBuilding() : null;
        if (target == highlightedBuilding) return;
        if (highlightedBuilding != null) highlightedBuilding.SetIndicatorVisible(false);
        highlightedBuilding = target;
        if (highlightedBuilding != null) highlightedBuilding.SetIndicatorVisible(true);
    }

    private void TryBeginInteraction()
    {
        Building building = FindTargetBuilding();
        if (building == null) return;
        currentBuilding = building;
        // 1. Спочатку встановлюємо базовий стан
        PlayerStateManager.Instance.SetState(PlayerState.Interacting);
        // 2. Потім викликаємо логіку будівлі (Store автоматично змінить стан на Shopping)
        currentBuilding.BeginInteraction();
    }

    private void EndCurrentInteraction()
    {
        currentBuilding?.EndInteraction();
        currentBuilding = null;
        PlayerStateManager.Instance.SetState(PlayerState.Regular);
    }

    private void Move()
    {
        if (PlayerStateManager.Instance.CurrentState == PlayerState.Interacting)
        {
            rigidBody.linearVelocity = new Vector3(0f, rigidBody.linearVelocity.y, 0f);
            return;
        }
        Vector2 input = InputManager.Instance.MoveDirection;
        Vector3 movement = new Vector3(input.x, 0f, input.y);
        rigidBody.linearVelocity = new Vector3(movement.x * currentMoveSpeed, rigidBody.linearVelocity.y, movement.z * currentMoveSpeed);
    }

    private void Rotate()
    {
        Vector2 input = InputManager.Instance.MoveDirection;
        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 lookDirection = new Vector3(input.x, 0f, input.y);
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }
}