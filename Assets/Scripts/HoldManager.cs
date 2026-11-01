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
    
    // Збереження фізичних властивостей замість isKinematic
    private bool wasGravityEnabled;
    private float originalDrag;
    private float originalAngularDrag;

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
                
                // Кешуємо початковий стан гравітації та опору
                wasGravityEnabled = heldItem.useGravity;
                originalDrag = heldItem.linearDamping;
                originalAngularDrag = heldItem.angularDamping;

                // Залишаємо об'єкт фізичним, але вимикаємо гравітацію, щоб він "висів" у повітрі
                heldItem.isKinematic = false;
                heldItem.useGravity = false;
                
                // Додаємо високий опір, щоб об'єкт не крутився і не відлітав при зіткненні зі стінами
                heldItem.linearDamping = 10f;
                heldItem.angularDamping = 10f;
                
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
            float itemWeight = heldInteractable != null ? Mathf.Max(heldInteractable.Weight, 0.1f) : 1f;
            float currentSpeed = baseDragSpeed / itemWeight;

            // Задаємо вектор напрямку і застосовуємо швидкість безпосередньо.
            // Це гарантує, що рушій Unity прораховуватиме колізії, і об'єкт зупиниться перед стіною.
            Vector3 moveDirection = (targetPosition - heldItem.position);
            heldItem.linearVelocity = moveDirection * currentSpeed;
        }
    }

    private void DropItem()
    {
        // Відновлюємо оригінальні фізичні параметри після відпускання ЛКМ
        heldItem.useGravity = wasGravityEnabled;
        heldItem.linearDamping = originalDrag;
        heldItem.angularDamping = originalAngularDrag;
        
        OnItemDropped?.Invoke(heldItem.gameObject);
        heldItem = null;
        heldInteractable.SetHoldState(false);
        heldInteractable = null;
    }
}