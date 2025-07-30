using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 2f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);
    [SerializeField] private bool smoothFollow = true;
    
    private Transform target;
    private CloneManager cloneManager;
    private PlayerController player;
    
    private void Start()
    {
        cloneManager = FindFirstObjectByType<CloneManager>();
        player = FindFirstObjectByType<PlayerController>();
        
        // Initially follow the player
        if (player != null)
        {
            target = player.transform;
        }
    }
    
    private void LateUpdate()
    {
        UpdateTarget();
        FollowTarget();
    }
    
    private void UpdateTarget()
    {
        // Always follow the player (not clones) since player is the active character
        if (player != null)
        {
            target = player.transform;
        }
        
        // Alternative: Follow the most recently created active clone
        /*
        if (cloneManager != null)
        {
            Clone activeClone = cloneManager.GetActiveClone();
            if (activeClone != null)
            {
                target = activeClone.transform;
            }
        }
        */
    }
    
    private void FollowTarget()
    {
        if (target == null) return;
        
        Vector3 targetPosition = target.position + offset;
        
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }
    }
}