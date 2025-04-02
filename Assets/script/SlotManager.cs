using System.Collections.Generic;
using UnityEngine;

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance; // Singleton per un facile accesso
    public List<GameObject> pages; // Lista delle pagine dell'album
    private int currentPageIndex = 0;
    private Transform selectedSlot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        UpdatePageVisibility();
    }

    public void SelectSlot(Transform slot)
    {
        selectedSlot = slot;
        Debug.Log("Slot selezionato: " + slot.name);
    }

    public Transform GetSelectedSlot()
    {
        return selectedSlot;
    }

    public void ChangePage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < pages.Count)
        {
            currentPageIndex = pageIndex;
            UpdatePageVisibility();
            Debug.Log("Pagina cambiata: " + pageIndex);
        }
    }

    private void UpdatePageVisibility()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
        }
    }
}
