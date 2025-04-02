using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoManager : MonoBehaviour
{
    public List<GameObject> photoPrefabs; // Prefabs delle foto
    public Transform pilePosition; // Posizione della pila
    public Transform handPosition; // Dove va la foto in mano
    public Transform reservePosition; // Dove va la riserva
    public Transform album; // Oggetto album

    public Button pickPhotoButton;
    public Button reservePhotoButton;
    public Button placePhotoButton;
    public Button rotatePhotoButton;
    public Button lowerPhotoButton;
    public Button nextPageButton;
    public Button previousPageButton;

    public PageSlotManager pageSlotManager; // Riferimento al gestore delle pagine

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
        // RIMOSSO: placePhotoButton.onClick.AddListener(PlaceInAlbum);
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
        Debug.Log("Tentativo di prendere una foto...");

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

            Debug.Log("Foto presa in mano: " + currentPhoto.name);
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
            Debug.Log("Foto messa in riserva: " + currentPhoto.name);
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

            Debug.Log("Foto presa dalla riserva.");
        }
        else if (currentPhoto != null && reservedPhoto != null)
        {
            Debug.Log("Hai già una foto in mano e una in riserva. Azione non consentita.");
        }
        else
        {
            Debug.Log("Nessuna azione disponibile con lo stato attuale.");
        }

        UpdateUIButtons();
    }

    public void PlaceInAlbum(Transform slot)
    {
        if (currentPhoto != null && slot != null)
        {
            pageSlotManager.AssignPhotoToSlot(currentPhoto, slot);
            Debug.Log("Foto piazzata nell'album nella pagina: " + pageSlotManager.GetCurrentPageName());
            currentPhoto = null;
        }
        else
        {
            Debug.Log("Errore: nessuna foto o slot non valido.");
        }

        UpdateUIButtons();
    }

    public void RotatePhoto()
    {
        if (currentPhoto != null)
        {
            if (isPhotoLowered)
            {
                Debug.Log("Non puoi ruotare la foto mentre è abbassata.");
                return;
            }

            if (isPhotoRotated)
            {
                currentPhoto.transform.rotation = originalPhotoRotation;
            }
            else
            {
                currentPhoto.transform.rotation = originalPhotoRotation * Quaternion.Euler(0, 0, 180);
            }

            isPhotoRotated = !isPhotoRotated;
            Debug.Log("Foto ruotata: " + (isPhotoRotated ? "Retro" : "Fronte"));
        }
    }

    public void TogglePhotoPosition()
    {
        if (currentPhoto != null)
        {
            float offsetZ = -1.0f;

            if (isPhotoLowered)
            {
                currentPhoto.transform.position = originalPhotoPosition;
            }
            else
            {
                currentPhoto.transform.position += new Vector3(0, 0, offsetZ);
            }

            isPhotoLowered = !isPhotoLowered;
            Debug.Log("Foto " + (isPhotoLowered ? "spostata in avanti" : "ripristinata"));
        }
    }

    private void UpdateUIButtons()
    {
        bool hasPhoto = currentPhoto != null;

        rotatePhotoButton.gameObject.SetActive(hasPhoto);
        lowerPhotoButton.gameObject.SetActive(hasPhoto);
        placePhotoButton.gameObject.SetActive(false); // disabilitato il pulsante automatico
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
}
