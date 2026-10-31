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
    [SerializeField] private ParticleSystem particleSystem;
    
    private Rigidbody ballRigidbody;
    private LineRenderer lineRenderer;
    private float currentScale = 1f;

    private void Awake()
    {
        ballRigidbody = GetComponent<Rigidbody>();
        InitLineRenderer();
    }

    private void Start()
    {
        currentScale = transform.localScale.x;
        Store store = Store.Instance != null ? Store.Instance : FindFirstObjectByType<Store>();
        if (store != null)
        {
            store.OnChiselKitBought.AddListener(OnChieselKitBought);
        }
    }

    private void OnDestroy()
    {
        Store store = Store.Instance != null ? Store.Instance : FindFirstObjectByType<Store>();
        if (store != null)
        {
            store.OnChiselKitBought.RemoveListener(OnChieselKitBought);
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

    public void OnChieselKitBought()
    {
        // Logs message, reduces ball scale by 0.1f, and breaks ball when reaching threshold
        currentScale = Mathf.Round((currentScale - 0.1f) * 10f) / 10f;
        particleSystem.Play();
        if (currentScale < 0.45f) Destroy(gameObject);
        else transform.localScale = Vector3.one * currentScale;
    }

    private void ApplyRopeForce()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        if (distance <= ropeLength) return;
        float stretch = distance - ropeLength;
        Vector3 direction = toPlayer.normalized;
        // Apply spring force: stiffness pulls proportionally to stretch, damping reduces oscillation
        Vector3 springForce = direction * (stretch * springStiffness);
        Vector3 dampingForce = -ballRigidbody.linearVelocity * springDamping;
        ballRigidbody.AddForce(springForce + dampingForce, ForceMode.Force);
    }

    private void UpdateRopeVisual()
    {
        Vector3 start = playerFeetAnchor.position;
        Vector3 end = ballPivotAnchor.position;
        float distance = Vector3.Distance(start, end);
        // Slack is how much rope is left hanging loose
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
        // Standard formula: (1-t)^2 * A + 2(1-t)t * C + t^2 * B
        float u = 1f - t;
        return (u * u * a) + (2f * u * t * control) + (t * t * b);
    }
}
