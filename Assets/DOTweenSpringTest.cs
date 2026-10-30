using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class DOTweenSpringTest : MonoBehaviour
{
    [Header("Spring Animation Parameters")]
    [SerializeField] private Vector3 punchScale = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private int vibrato = 10;
    [SerializeField] private float elasticity = 1f;
    [SerializeField] private AudioSource audioSource;

    private void OnMouseDown()
    {
        // Complete current tween to prevent scale distortion on rapid clicks
        transform.DOComplete();
        
        // Execute the spring scaling effect
        transform.DOPunchScale(punchScale, duration, vibrato, elasticity);
        audioSource.Play();
    }
}