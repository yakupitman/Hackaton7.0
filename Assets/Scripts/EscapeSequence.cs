using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

public class EscapeSequence : MonoBehaviour
{
    public UnityEvent OnEscaped;
    public UnityEvent OnPlayerCaught;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform ghost;
    [SerializeField] private Transform ghostSpawnPoint;
    [SerializeField] private Transform successTrigger;
    [SerializeField] private CinemachineCamera playerCamera;

    [Header("Chase Settings")]
    [SerializeField] private float ghostSpeed = 7.9f; 
    [SerializeField] private float catchDistance = 1.5f;

    [Header("FOV Settings")]
    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float targetFOV = 90f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource chaseAudioSource;
    [SerializeField] private AudioClip ghostSpawnSound;

    private bool isChasing = false;
    private float initialDistanceToExit;

    private void Start()
    {
        if (ghost != null) 
        {
            ghost.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isChasing && other.CompareTag("Player"))
        {
            isChasing = true;
            
            if (successTrigger != null && player != null)
            {
                initialDistanceToExit = Vector3.Distance(player.position, successTrigger.position);
            }
            
            if (ghost != null)
            {
                if (ghostSpawnPoint != null) 
                {
                    ghost.position = ghostSpawnPoint.position;
                }
                ghost.gameObject.SetActive(true);
            }

            if (chaseAudioSource != null && ghostSpawnSound != null)
            {
                chaseAudioSource.clip = ghostSpawnSound;
                chaseAudioSource.Play();
            }
        }
    }

    private void Update()
    {
        if (!isChasing || ghost == null || player == null) return;

        MoveGhost();
        UpdateFOV();
        CheckCatch();
    }

    private void MoveGhost()
    {
        Vector3 direction = (player.position - ghost.position).normalized;
        direction.y = 0; 

        if (direction != Vector3.zero)
        {
            ghost.rotation = Quaternion.LookRotation(direction);
        }

        ghost.position += direction * ghostSpeed * Time.deltaTime;
    }

    private void UpdateFOV()
    {
        if (playerCamera == null || successTrigger == null || initialDistanceToExit <= 0f) return;

        float currentDistance = Vector3.Distance(player.position, successTrigger.position);
        float progress = 1f - Mathf.Clamp01(currentDistance / initialDistanceToExit);
        
        playerCamera.Lens.FieldOfView = Mathf.Lerp(defaultFOV, targetFOV, progress);
    }

    private void CheckCatch()
    {
        Vector3 ghostPos2D = new Vector3(ghost.position.x, 0, ghost.position.z);
        Vector3 playerPos2D = new Vector3(player.position.x, 0, player.position.z);
        
        float distance = Vector3.Distance(ghostPos2D, playerPos2D);
        
        if (distance <= catchDistance)
        {
            isChasing = false;
            ResetState();
            OnPlayerCaught?.Invoke();
            PlayerStateManager.Instance.Die();
        }
    }

    public void SuccessfulEscape()
    {
        if (!isChasing) return;
        
        isChasing = false;
        ResetState();
        
        if (ghost != null) 
        {
            ghost.gameObject.SetActive(false);
        }
        OnEscaped?.Invoke();
    }

    private void ResetState()
    {
        ghost.gameObject.SetActive(false);
        if (playerCamera != null)
        {
            playerCamera.Lens.FieldOfView = defaultFOV;
        }
        
        if (chaseAudioSource != null)
        {
            chaseAudioSource.Stop();
        }
    }
}