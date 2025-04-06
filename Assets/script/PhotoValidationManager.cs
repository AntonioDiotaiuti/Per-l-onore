using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CorrectPhotoAssignment
{
    public GameObject photo;
    public Transform correctSlot;
}

public class PhotoValidationManager : MonoBehaviour
{
    public List<CorrectPhotoAssignment> correctAssignments;
    private Dictionary<GameObject, Transform> correctSlotAssignment = new();

    void Awake()
    {
        foreach (var assignment in correctAssignments)
        {
            if (assignment.photo != null && assignment.correctSlot != null)
            {
                correctSlotAssignment[assignment.photo] = assignment.correctSlot;
            }
        }
    }

    public bool IsPhotoInCorrectSlot(GameObject photo, Transform slot)
    {
        return correctSlotAssignment.TryGetValue(photo, out Transform correctSlot) && correctSlot == slot;
    }

    public bool HasAssignment(GameObject photo)
    {
        return correctSlotAssignment.ContainsKey(photo);
    }

    public Transform GetCorrectSlot(GameObject photo)
    {
        return correctSlotAssignment.TryGetValue(photo, out Transform correctSlot) ? correctSlot : null;
    }
}
