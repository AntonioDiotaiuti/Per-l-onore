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
                    GameObject highlight = Instantiate(slotHighlightPrefab, slot.position + new Vector3(0, 0, 0), Quaternion.Euler(90, 0, 0), slot);
                    highlight.SetActive(false);
                    slotHighlights[slot] = highlight;
                }

                SlotClickHandler clickHandler = slot.GetComponent<SlotClickHandler>();
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
                bool shouldHighlight = true;
                if (pagePhotoMap.TryGetValue(leftPage.pageName, out var slotMap) &&
                    slotMap.TryGetValue(slot, out GameObject placedPhoto) &&
                    placedPhoto != null)
                {
                    var photoHandler = placedPhoto.GetComponent<PhotoHandler>();
                    var slotHandler = slot.GetComponent<SlotClickHandler>();
                    if (photoHandler != null && slotHandler != null &&
                        photoHandler.ID == slotHandler.AssociatedIDPhoto)
                    {
                        shouldHighlight = false;
                    }
                }
                slotHighlights[slot].SetActive(shouldHighlight);
            }
        }

        if (rightPageIndex < pages.Count)
        {
            foreach (var slot in pages[rightPageIndex].slots)
            {
                if (slotHighlights.ContainsKey(slot))
                {
                    bool shouldHighlight = true;
                    if (pagePhotoMap.TryGetValue(pages[rightPageIndex].pageName, out var slotMap) &&
                        slotMap.TryGetValue(slot, out GameObject placedPhoto) &&
                        placedPhoto != null)
                    {
                        var photoHandler = placedPhoto.GetComponent<PhotoHandler>();
                        var slotHandler = slot.GetComponent<SlotClickHandler>();
                        if (photoHandler != null && slotHandler != null &&
                            photoHandler.ID == slotHandler.AssociatedIDPhoto)
                        {
                            shouldHighlight = false;
                        }
                    }
                    slotHighlights[slot].SetActive(shouldHighlight);
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

    public void OnSlotClicked(Transform slot, SlotClickHandler slotHandler)
    {
        var page = pages[currentPageIndex];
        var pageName = page.pageName;

        GameObject photoInHand = photoManager.GetCurrentPhotoInHand();

        if (photoInHand == null && pagePhotoMap[pageName].ContainsKey(slot) && pagePhotoMap[pageName][slot] != null)
        {
            GameObject existingPhoto = pagePhotoMap[pageName][slot];
            if (existingPhoto != null)
            {
                var photoHandler = existingPhoto.GetComponent<PhotoHandler>();
                if (photoHandler != null && !photoHandler.IsSameID(slotHandler.AssociatedIDPhoto))
                {
                    pagePhotoMap[pageName][slot] = null;
                    existingPhoto.transform.SetParent(null);
                    existingPhoto.transform.position = photoManager.handPosition.position;
                    photoManager.SetCurrentPhoto(existingPhoto);
                }
                else
                {
                    // TODO: handle pick up when photo is in position
                }
            }

            return;
        }

        if (photoInHand != null && (!pagePhotoMap.ContainsKey(pageName) || !pagePhotoMap[pageName].ContainsKey(slot) || pagePhotoMap[pageName][slot] == null))
        {
            Debug.Log("Placing photo!");
            if (!pagePhotoMap.ContainsKey(pageName))
                pagePhotoMap[pageName] = new Dictionary<Transform, GameObject>();

            pagePhotoMap[pageName][slot] = photoInHand;
            photoInHand.transform.position = slot.position;
            photoInHand.transform.SetParent(slot);
            photoManager.ClearCurrentPhoto();

            // Disattiva highlight se la foto corretta per questo slot
            var photoHandler = photoInHand.GetComponent<PhotoHandler>();
            if (photoHandler != null && slotHighlights.ContainsKey(slot))
            {
                if (photoHandler.ID == slotHandler.AssociatedIDPhoto)
                {
                    slotHighlights[slot].SetActive(false);
                    Debug.Log("Foto corretta: highlight disattivato.");
                }
            }
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
        int nextIndex = currentPageIndex + 1;
        if (nextIndex < pages.Count)
        {
            ShowPage(nextIndex);
        }
    }

    public void PreviousPage()
    {
        int previousIndex = currentPageIndex - 1;
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
        nextButton.interactable = (currentPageIndex + 1 < pages.Count);
        previousButton.interactable = (currentPageIndex - 1 >= 0);
    }
}
