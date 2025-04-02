using UnityEngine;

public class ListOfPhoto : MonoBehaviour
{


    public GameObject[] cubePrefab; // Assign a cube prefab in the Inspector
    public Vector3 spawnOffset = new Vector3(0, 1, 0); // Offset for spawning the cube
    int randomIndex;
    void OnMouseDown()
    {
        for(int i = 0; i < cubePrefab.Length; i++)
        {
            randomIndex = Random.Range(0, cubePrefab.Length);
            if (cubePrefab[randomIndex] != null)
            {
                Instantiate(cubePrefab[randomIndex], transform.position + spawnOffset, Quaternion.identity);
                cubePrefab[randomIndex] = null;
            }
            else
            {
                i--;
            }
            
        } 
    }
}


