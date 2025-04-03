using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoManager : MonoBehaviour
{
    public List<GameObject> photoPrefabs;
    public Transform pilePosition;
    public Transform handPosition;
    public Transform reservePosition;
    public Transform album;

    public Button pickPhotoButton;
    public Button reservePhotoButton;
    public Button rotatePhotoButton;
    public Button lowerPhotoButton;
    public Button nextPageButton;
    public Button previousPageButton;

    public PageSlotManager pageSlotManager;

    private Stack<GameObject> photoStack = new Stack<GameObject>();
    private GameObject currentPhoto;
    private GameObject reservedPhoto;
    private bool isPhotoRotated = false;
    private bool isPhotoLowered = false;

    private Vector3 originalPhotoPosition;
    private Quaternion originalPhotoRotation;

    void Start()
    {
        InitializePhotos();
        pickPhotoButton.onClick.AddListener(PickPhoto);
        reservePhotoButton.onClick.AddListener(ToggleReserveState);
        rotatePhotoButton.onClick.AddListener(RotatePhoto);
        lowerPhotoButton.onClick.AddListener(TogglePhotoPosition);
        nextPageButton.onClick.AddListener(pageSlotManager.NextPage);
        previousPageButton.onClick.AddListener(pageSlotManager.PreviousPage);
        UpdateUIButtons();
    }

    void InitializePhotos()
    {
        List<GameObject> shuffledPhotos = new List<GameObject>(photoPrefabs);
        ShuffleList(shuffledPhotos);

        foreach (var photoPrefab in shuffledPhotos)
        {
            GameObject photo = Instantiate(photoPrefab, pilePosition.position, Quaternion.identity);
            photo.SetActive(false);
            photoStack.Push(photo);
        }

        Debug.Log("Pila inizializzata con " + photoStack.Count + " foto.");
    }

    void ShuffleList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void PickPhoto()
    {
        if (currentPhoto != null)
        {
            Debug.Log("Hai già una foto in mano.");
            return;
        }

        if (photoStack.Count > 0)
        {
            currentPhoto = photoStack.Pop();
            currentPhoto.SetActive(true);
            currentPhoto.transform.position = handPosition.position;
            originalPhotoPosition = currentPhoto.transform.position;
            originalPhotoRotation = currentPhoto.transform.rotation;
            isPhotoRotated = false;
            isPhotoLowered = false;
        }
        else
        {
            Debug.Log("Nessuna foto disponibile nella pila.");
        }

        UpdateUIButtons();
    }

    public void ToggleReserveState()
    {
        if (currentPhoto != null && reservedPhoto == null)
        {
            currentPhoto.transform.position = reservePosition.position;
            reservedPhoto = currentPhoto;
            currentPhoto = null;
        }
        else if (currentPhoto == null && reservedPhoto != null)
        {
            currentPhoto = reservedPhoto;
            currentPhoto.transform.position = handPosition.position;
            originalPhotoPosition = currentPhoto.transform.position;
            originalPhotoRotation = currentPhoto.transform.rotation;
            reservedPhoto = null;
            isPhotoRotated = false;
            isPhotoLowered = false;
        }

        UpdateUIButtons();
    }

    public void RotatePhoto()
    {
        if (currentPhoto != null && !isPhotoLowered)
        {
            if (isPhotoRotated)
                currentPhoto.transform.rotation = originalPhotoRotation;
            else
                currentPhoto.transform.rotation = originalPhotoRotation * Quaternion.Euler(0, 0, 180);

            isPhotoRotated = !isPhotoRotated;
        }
    }

    public void TogglePhotoPosition()
    {
        if (currentPhoto != null)
        {
            float offsetZ = -1.0f;
            if (isPhotoLowered)
                currentPhoto.transform.position = originalPhotoPosition;
            else
                currentPhoto.transform.position += new Vector3(0, 0, offsetZ);

            isPhotoLowered = !isPhotoLowered;
        }
    }

    private void UpdateUIButtons()
    {
        bool hasPhoto = currentPhoto != null;
        rotatePhotoButton.gameObject.SetActive(hasPhoto);
        lowerPhotoButton.gameObject.SetActive(hasPhoto);
        reservePhotoButton.gameObject.SetActive(hasPhoto || (!hasPhoto && reservedPhoto != null));
    }

    public GameObject GetCurrentPhotoInHand()
    {
        return currentPhoto;
    }

    public void ClearCurrentPhoto()
    {
        currentPhoto = null;
        UpdateUIButtons();
    }

    public void SetCurrentPhoto(GameObject photo)
    {
        currentPhoto = photo;
        originalPhotoPosition = photo.transform.position;
        originalPhotoRotation = photo.transform.rotation;
        isPhotoRotated = false;
        isPhotoLowered = false;
        UpdateUIButtons();
    }
}
