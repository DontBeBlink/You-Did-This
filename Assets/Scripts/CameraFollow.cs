using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 2f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);
    [SerializeField] private bool smoothFollow = true;
    
    private Transform target;
    private CloneManager cloneManager;
    
    private void Start()
    {
        cloneManager = FindObjectOfType<CloneManager>();
    }
    
    private void LateUpdate()
    {
        UpdateTarget();
        FollowTarget();
    }
    
    private void UpdateTarget()
    {
        if (cloneManager != null)
        {
            Clone activeClone = cloneManager.GetActiveClone();
            if (activeClone != null)
            {
                target = activeClone.transform;
            }
        }
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