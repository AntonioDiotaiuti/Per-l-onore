using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PageSlotManager : MonoBehaviour
{
    public Transform leftPageAnchor;
    public Transform rightPageAnchor;
    public Button nextButton;
    public Button previousButton;
    [System.Serializable]
    public class AlbumPage
    {
        public string pageName;
        public GameObject pageObject;
        public Transform[] slots;
        public Animator animator;
    }

    public List<AlbumPage> pages;
    public GameObject slotHighlightPrefab;

    private Dictionary<string, Dictionary<Transform, GameObject>> pagePhotoMap = new();
    private Dictionary<Transform, GameObject> slotHighlights = new();
    private int currentPageIndex = 0;
    private Dictionary<GameObject, Vector3> originalScales = new();
    private PhotoManager photoManager;

    void Start()
    {
        photoManager = FindFirstObjectByType<PhotoManager>();

        foreach (var page in pages)
        {
            pagePhotoMap[page.pageName] = new Dictionary<Transform, GameObject>();
            originalScales[page.pageObject] = page.pageObject.transform.localScale;
            page.pageObject.SetActive(false);

            foreach (var slot in page.slots)
            {
                if (slotHighlightPrefab)
                {
                    GameObject highlight = Instantiate(slotHighlightPrefab, slot.position + new Vector3(0, 0.2f, 0), Quaternion.Euler(90, 0, 0), slot);
                    highlight.SetActive(false);
                    slotHighlights[slot] = highlight;
                }

                SlotClickHandler clickHandler = slot.gameObject.AddComponent<SlotClickHandler>();
                clickHandler.Initialize(this, slot);
            }
        }

        nextButton.onClick.AddListener(NextPage);
        previousButton.onClick.AddListener(PreviousPage);

        ShowPage(currentPageIndex);
    }

    public void ShowPage(int index)
    {
        if (index < 0 || index >= pages.Count) return;

        foreach (var page in pages)
        {
            page.pageObject.SetActive(false);
        }

        int rightPageIndex = index + 1;

        AlbumPage leftPage = pages[index];
        leftPage.pageObject.SetActive(true);
        leftPage.pageObject.transform.position = leftPageAnchor.position;
        leftPage.pageObject.transform.rotation = leftPageAnchor.rotation;
        leftPage.pageObject.transform.localScale = leftPageAnchor.lossyScale;
        if (leftPage.animator != null)
        {
            leftPage.animator.SetTrigger("OpenPage");
        }

        if (rightPageIndex < pages.Count)
        {
            AlbumPage rightPage = pages[rightPageIndex];
            rightPage.pageObject.SetActive(true);
            rightPage.pageObject.transform.position = rightPageAnchor.position;
            rightPage.pageObject.transform.rotation = rightPageAnchor.rotation;
            rightPage.pageObject.transform.localScale = rightPageAnchor.lossyScale;
            if (rightPage.animator != null)
            {
                rightPage.animator.SetTrigger("OpenPage");
            }
        }

        currentPageIndex = index;

        foreach (var slot in slotHighlights.Keys)
        {
            slotHighlights[slot].SetActive(false);
        }

        foreach (var slot in leftPage.slots)
        {
            if (slotHighlights.ContainsKey(slot))
            {
                slotHighlights[slot].SetActive(true);
            }
        }

        if (rightPageIndex < pages.Count)
        {
            foreach (var slot in pages[rightPageIndex].slots)
            {
                if (slotHighlights.ContainsKey(slot))
                {
                    slotHighlights[slot].SetActive(true);
                }
            }
        }

        UpdateNavigationButtons();
    }

    void OnApplicationQuit()
    {
        foreach (var page in pages)
        {
            if (originalScales.TryGetValue(page.pageObject, out Vector3 originalScale))
            {
                page.pageObject.transform.localScale = originalScale;
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
            existingPhoto.transform.position = photoManager.handPosition.position;
            photoManager.SetCurrentPhoto(existingPhoto);
            return;
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
        int nextIndex = currentPageIndex + 2;
        if (nextIndex < pages.Count)
        {
            ShowPage(nextIndex);
        }
    }

    public void PreviousPage()
    {
        int previousIndex = currentPageIndex - 2;
        if (previousIndex >= 0)
        {
            ShowPage(previousIndex);
        }
    }

    public string GetCurrentPageName()
    {
        return pages[currentPageIndex].pageName;
    }

    private void UpdateNavigationButtons()
    {
        nextButton.interactable = (currentPageIndex + 2 < pages.Count);
        previousButton.interactable = (currentPageIndex - 2 >= 0);
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
