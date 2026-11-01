using UnityEngine;

public class BallFollower : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerFeetAnchor;
    [SerializeField] private Transform ballPivotAnchor;
    [SerializeField] private float ropeLength = 2f;
    [SerializeField] private float springStiffness = 80f;
    [SerializeField] private float springDamping = 10f;
    [SerializeField] private int ropeSegments = 24;
    [SerializeField] private float ropeWidth = 0.04f;
    [SerializeField] private float sagMultiplier = 0.6f;
    [SerializeField] private ParticleSystem breakParticleSystem;
    
    private Rigidbody ballRigidbody;
    private LineRenderer lineRenderer;
    private float initialScale;
    private float currentScale;
    
    private const int TotalChiselUpgrades = 5;
    private const float MinimumScale = 0.33f;
    private int useCount = 0;

    private void Awake()
    {
        ballRigidbody = GetComponent<Rigidbody>();
        InitLineRenderer();
    }

    private void Start()
    {
        initialScale = transform.localScale.x;
        currentScale = initialScale;
        
        Store store = Store.Instance != null ? Store.Instance : FindFirstObjectByType<Store>();
        if (store != null)
        {
            store.OnChiselKitBought.AddListener(OnChiselKitBought);
        }
    }

    private void OnDestroy()
    {
        Store store = Store.Instance != null ? Store.Instance : FindFirstObjectByType<Store>();
        if (store != null)
        {
            store.OnChiselKitBought.RemoveListener(OnChiselKitBought);
        }
    }

    private void InitLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = ropeSegments;
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;
        lineRenderer.useWorldSpace = true;
    }

    private void FixedUpdate()
    {
        ApplyRopeForce();
    }

    private void LateUpdate()
    {
        UpdateRopeVisual();
    }

    public void OnChiselKitBought()
    {
        if (useCount >= TotalChiselUpgrades) return;
        
        useCount++;
        
        if (breakParticleSystem != null)
        {
            breakParticleSystem.Play();
        }

        if (useCount >= TotalChiselUpgrades)
        {
            Destroy(gameObject);
        }
        else
        {
            // Calculate step size so scale reaches exactly 0.33f on the 4th use (TotalChiselUpgrades - 1)
            float scaleReductionsCount = TotalChiselUpgrades - 1;
            float scaleStep = (initialScale - MinimumScale) / scaleReductionsCount;
            
            currentScale -= scaleStep;
            currentScale = Mathf.Max(MinimumScale, currentScale);
            transform.localScale = Vector3.one * currentScale;
        }
    }

    private void ApplyRopeForce()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        if (distance <= ropeLength) return;
        
        float stretch = distance - ropeLength;
        Vector3 direction = toPlayer.normalized;
        Vector3 springForce = direction * (stretch * springStiffness);
        Vector3 dampingForce = -ballRigidbody.linearVelocity * springDamping;
        
        ballRigidbody.AddForce(springForce + dampingForce, ForceMode.Force);
    }

    private void UpdateRopeVisual()
    {
        Vector3 start = playerFeetAnchor.position;
        Vector3 end = ballPivotAnchor.position;
        float distance = Vector3.Distance(start, end);
        float slack = Mathf.Max(0f, ropeLength - distance);

        Vector3 midPoint = (start + end) * 0.5f;
        Vector3 controlPoint = midPoint + Vector3.down * (slack * sagMultiplier);
        
        for (int i = 0; i < ropeSegments; i++)
        {
            float t = i / (float)(ropeSegments - 1);
            lineRenderer.SetPosition(i, QuadraticBezier(start, controlPoint, end, t));
        }
    }

    private Vector3 QuadraticBezier(Vector3 a, Vector3 control, Vector3 b, float t)
    {
        float u = 1f - t;
        return (u * u * a) + (2f * u * t * control) + (t * t * b);
    }
}