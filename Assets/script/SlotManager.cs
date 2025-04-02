using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PageSlotManager : MonoBehaviour
{
    [System.Serializable]
    public class AlbumPage
    {
        public string pageName;
        public GameObject pageObject;
        public Transform[] slots;
        public Animator animator;
    }

    public List<AlbumPage> pages;
    public GameObject slotHighlightPrefab; // UI evidenziatore semplice

    private Dictionary<string, Dictionary<Transform, GameObject>> pagePhotoMap = new();
    private Dictionary<Transform, GameObject> slotHighlights = new();
    private int currentPageIndex = 0;
    private PhotoManager photoManager;

    void Start()
    {
        photoManager = FindFirstObjectByType<PhotoManager>();

        foreach (var page in pages)
        {
            pagePhotoMap[page.pageName] = new Dictionary<Transform, GameObject>();
            page.pageObject.SetActive(false);

            foreach (var slot in page.slots)
            {
                // Crea evidenziatore UI (opzionale ma elegante)
                if (slotHighlightPrefab)
                {
                    GameObject highlight = Instantiate(slotHighlightPrefab, slot.position, Quaternion.identity, slot);
                    highlight.SetActive(false);
                    slotHighlights[slot] = highlight;
                }

                // Aggiungi componente di interazione
                SlotClickHandler clickHandler = slot.gameObject.AddComponent<SlotClickHandler>();
                clickHandler.Initialize(this, slot);
            }
        }

        ShowPage(currentPageIndex);
    }

    public void ShowPage(int index)
    {
        if (index < 0 || index >= pages.Count) return;

        foreach (var page in pages)
        {
            page.pageObject.SetActive(false);
        }

        AlbumPage currentPage = pages[index];
        currentPage.pageObject.SetActive(true);

        if (currentPage.animator != null)
        {
            currentPage.animator.SetTrigger("OpenPage");
        }

        if (pagePhotoMap.TryGetValue(currentPage.pageName, out var slotMap))
        {
            foreach (var kvp in slotMap)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.transform.position = kvp.Key.position;
                    kvp.Value.transform.SetParent(kvp.Key);
                    kvp.Value.SetActive(true);
                }
            }
        }

        currentPageIndex = index;

        // Attiva gli highlight solo per gli slot di questa pagina
        foreach (var slot in slotHighlights.Keys)
        {
            slotHighlights[slot].SetActive(false);
        }
        foreach (var slot in currentPage.slots)
        {
            if (slotHighlights.ContainsKey(slot))
            {
                slotHighlights[slot].SetActive(true);
            }
        }
    }

    public void OnSlotClicked(Transform slot)
    {
        var page = pages[currentPageIndex];
        var pageName = page.pageName;

        GameObject photoInHand = photoManager.GetCurrentPhotoInHand();

        if (pagePhotoMap[pageName].ContainsKey(slot) && pagePhotoMap[pageName][slot] != null)
        {
            GameObject existingPhoto = pagePhotoMap[pageName][slot];
            pagePhotoMap[pageName][slot] = null;
            existingPhoto.transform.SetParent(null);
            photoManager.ClearCurrentPhoto();

            existingPhoto.transform.position = photoManager.transform.position;
            photoInHand = existingPhoto;
        }

        if (photoInHand != null)
        {
            pagePhotoMap[pageName][slot] = photoInHand;
            photoInHand.transform.position = slot.position;
            photoInHand.transform.SetParent(slot);
            photoManager.ClearCurrentPhoto();
        }
    }

    public void AssignPhotoToSlot(GameObject photo, Transform slot)
    {
        var page = pages[currentPageIndex];
        if (!pagePhotoMap[page.pageName].ContainsKey(slot))
        {
            pagePhotoMap[page.pageName].Add(slot, photo);
        }
        else
        {
            pagePhotoMap[page.pageName][slot] = photo;
        }

        photo.transform.position = slot.position;
        photo.transform.SetParent(slot);
        photo.SetActive(true);
    }

    public void NextPage()
    {
        ShowPage((currentPageIndex + 1) % pages.Count);
    }

    public void PreviousPage()
    {
        ShowPage((currentPageIndex - 1 + pages.Count) % pages.Count);
    }

    public string GetCurrentPageName()
    {
        return pages[currentPageIndex].pageName;
    }
}

public class SlotClickHandler : MonoBehaviour, IPointerClickHandler
{
    private PageSlotManager manager;
    private Transform slot;

    public void Initialize(PageSlotManager mgr, Transform s)
    {
        manager = mgr;
        slot = s;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        manager.OnSlotClicked(slot);
    }
}
