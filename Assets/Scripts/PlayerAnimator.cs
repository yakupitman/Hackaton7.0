using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rigidBody;

    private readonly int blendParameter = Animator.StringToHash("Blend");
    private const float animationDampTime = 0.05f;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rigidBody == null) rigidBody = GetComponentInParent<Rigidbody>();
    }

    private void Update()
    {
        UpdateAnimationTree();
    }

    private void UpdateAnimationTree()
    {
        // Idle in every non-Regular state, since only Regular can move
        if (PlayerStateManager.Instance != null && PlayerStateManager.Instance.CurrentState != PlayerState.Regular)
        {
            animator.SetFloat(blendParameter, 0f);
            animator.speed = 1f;
            return;
        }
        Vector3 planarVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);
        float actualSpeed = planarVelocity.magnitude;
        if (actualSpeed > 0.1f)
        {
            float targetSpeed = Mathf.Clamp(actualSpeed, 1f, 6f);
            animator.SetFloat(blendParameter, targetSpeed, animationDampTime, Time.deltaTime);
            // Interpolates animation playback speed between walking and running
            float normalizedSpeed = Mathf.InverseLerp(1f, 6f, actualSpeed);
            animator.speed = Mathf.Lerp(0.5f, 1f, normalizedSpeed);
        }
        else
        {
            animator.SetFloat(blendParameter, 0f);
            animator.speed = 1f;
        }
    }
}
