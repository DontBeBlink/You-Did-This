using System.Collections.Generic;
using UnityEngine;

public class CloneManager : MonoBehaviour
{
    [Header("Clone Settings")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private Transform cloneParent;
    [SerializeField] private int maxClones = 10;
    
    private List<Clone> clones = new List<Clone>();
    private int activeCloneIndex = -1;
    
    [Header("Debug Info")]
    [SerializeField] private int totalClones = 0;
    [SerializeField] private int stuckClones = 0;
    
    private void Start()
    {
        // Find the initial player clone if it exists
        Clone initialClone = FindObjectOfType<Clone>();
        if (initialClone != null)
        {
            clones.Add(initialClone);
            activeCloneIndex = 0;
            initialClone.SetState(Clone.CloneState.Active);
        }
        
        UpdateDebugInfo();
    }
    
    public void CreateClone()
    {
        if (clones.Count >= maxClones)
        {
            Debug.LogWarning("Maximum number of clones reached!");
            return;
        }
        
        if (activeCloneIndex >= 0 && activeCloneIndex < clones.Count)
        {
            Clone currentActiveClone = clones[activeCloneIndex];
            
            // Stop recording on current clone and set it to replaying
            currentActiveClone.SetState(Clone.CloneState.Replaying);
            
            // Create new clone at current position
            Vector3 spawnPosition = currentActiveClone.transform.position;
            GameObject newCloneObj = Instantiate(clonePrefab, spawnPosition, Quaternion.identity, cloneParent);
            
            Clone newClone = newCloneObj.GetComponent<Clone>();
            if (newClone == null)
            {
                newClone = newCloneObj.AddComponent<Clone>();
            }
            
            clones.Add(newClone);
            activeCloneIndex = clones.Count - 1;
            newClone.SetState(Clone.CloneState.Active);
            
            Debug.Log($"Created new clone. Total clones: {clones.Count}");
            
            // Play clone creation sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCloneCreateSound();
            }
        }
        
        UpdateDebugInfo();
    }
    
    public void RetractToLastClone()
    {
        if (clones.Count <= 1)
        {
            Debug.Log("Cannot retract - only one clone remaining");
            return;
        }
        
        // Can't retract if previous clone is stuck
        if (activeCloneIndex > 0)
        {
            Clone previousClone = clones[activeCloneIndex - 1];
            if (previousClone.IsStuck)
            {
                Debug.Log("Cannot retract - previous clone is stuck at goal");
                return;
            }
        }
        
        // Destroy current active clone
        Clone currentClone = clones[activeCloneIndex];
        clones.RemoveAt(activeCloneIndex);
        Destroy(currentClone.gameObject);
        
        // Activate previous clone
        activeCloneIndex--;
        if (activeCloneIndex >= 0)
        {
            Clone previousClone = clones[activeCloneIndex];
            previousClone.SetState(Clone.CloneState.Active);
            Debug.Log($"Retracted to previous clone. Total clones: {clones.Count}");
            
            // Play retract sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCloneRetractSound();
            }
        }
        
        UpdateDebugInfo();
    }
    
    public void SetCloneStuck(Clone clone)
    {
        if (clone != null && clones.Contains(clone))
        {
            clone.SetState(Clone.CloneState.Stuck);
            Debug.Log($"Clone reached goal and is now stuck");
        }
        
        UpdateDebugInfo();
    }
    
    private void UpdateDebugInfo()
    {
        totalClones = clones.Count;
        stuckClones = 0;
        
        foreach (Clone clone in clones)
        {
            if (clone.IsStuck)
                stuckClones++;
        }
    }
    
    public Clone GetActiveClone()
    {
        if (activeCloneIndex >= 0 && activeCloneIndex < clones.Count)
            return clones[activeCloneIndex];
        return null;
    }
    
    public List<Clone> GetAllClones()
    {
        return new List<Clone>(clones);
    }
    
    public int ActiveCloneIndex => activeCloneIndex;
    public int TotalClones => clones.Count;
    public int StuckClones => stuckClones;
}