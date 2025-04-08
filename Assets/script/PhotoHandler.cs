using UnityEngine;

public class PhotoHandler : MonoBehaviour
{
    public int ID;

    public bool IsSameID(int IDToCheck)
    {
        return ID == IDToCheck;
    }
}
