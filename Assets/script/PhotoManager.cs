using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoManager : MonoBehaviour
{
    public List<GameObject> photoPrefabs; // Prefabs delle foto
    public Transform pilePosition; // Posizione della pila di foto
    public Transform handPosition; // Posizione della foto in mano
    public Transform reservePosition; // Posizione della zona di riserva
    public Transform album;
    public Button pickPhotoButton;
    public Button reservePhotoButton;
    public Button placePhotoButton;
    public Button rotatePhotoButton;
    public Button lowerPhotoButton;

    private Stack<GameObject> photoStack = new Stack<GameObject>();
    private GameObject currentPhoto;
    private bool isPhotoRotated = false;
    private bool isPhotoLowered = false;
    private Vector3 originalPhotoPosition;
    private Quaternion originalPhotoRotation;

    void Start()
    {
        InitializePhotos();
        pickPhotoButton.onClick.AddListener(PickPhoto);
        reservePhotoButton.onClick.AddListener(PlaceInReserve);
        placePhotoButton.onClick.AddListener(() => PlaceInAlbum(null)); // Aggiustare per usare slot corretti
        rotatePhotoButton.onClick.AddListener(RotatePhoto);
        lowerPhotoButton.onClick.AddListener(TogglePhotoPosition);
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
        Debug.Log("Pila di foto inizializzata con " + photoStack.Count + " foto.");
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
        if (currentPhoto == null && photoStack.Count > 0)
        {
            currentPhoto = photoStack.Pop();
            currentPhoto.SetActive(true);
            currentPhoto.transform.position = handPosition.position;
            originalPhotoPosition = handPosition.position;
            originalPhotoRotation = currentPhoto.transform.rotation;
            isPhotoRotated = false;
            isPhotoLowered = false;
            Debug.Log("Foto presa in mano: " + currentPhoto.name);
        }
        else
        {
            Debug.Log("Nessuna foto da prendere o già una in mano.");
        }
        UpdateUIButtons();
    }

    public void PlaceInReserve()
    {
        if (currentPhoto != null)
        {
            currentPhoto.transform.position = reservePosition.position;
            Debug.Log("Foto messa in riserva: " + currentPhoto.name);
            currentPhoto = null;
        }
        else
        {
            Debug.Log("Nessuna foto da mettere in riserva.");
        }
        UpdateUIButtons();
    }

    public void PlaceInAlbum(Transform slot)
    {
        if (currentPhoto != null && slot != null)
        {
            currentPhoto.transform.position = slot.position; // Piazza la foto nello slot
            currentPhoto.transform.SetParent(album); // La foto diventa figlia dell'album
            Debug.Log("Foto posizionata nell'album: " + currentPhoto.name);
            currentPhoto = null; // Ora non hai più la foto in mano
        }
        else
        {
            Debug.Log("Nessuna foto in mano o slot non valido.");
        }
        UpdateUIButtons(); // Ricarica i pulsanti
    }

    public void RotatePhoto()
    {
        if (currentPhoto != null)
        {
            float rotationZ = isPhotoRotated ? 0f : 180f; // Ruota solo sull'asse Z
            currentPhoto.transform.Rotate(0, 0, rotationZ);
            isPhotoRotated = !isPhotoRotated;
            Debug.Log("Foto ruotata: " + (isPhotoRotated ? "Retro" : "Fronte"));
        }
    }

    public void TogglePhotoPosition()
    {
        if (currentPhoto != null)
        {
            float offsetZ = -1.0f; // Quanto deve spostarsi la foto sull'asse Z
            if (isPhotoLowered)
            {
                currentPhoto.transform.position = originalPhotoPosition;
            }
            else
            {
                currentPhoto.transform.position += new Vector3(0, 0, offsetZ);
            }
            isPhotoLowered = !isPhotoLowered;
            Debug.Log("Foto " + (isPhotoLowered ? "Abbassata" : "Ripristinata"));
        }
    }

    private void UpdateUIButtons()
    {
        bool hasPhotoInHand = currentPhoto != null; // Verifica se hai una foto in mano
        rotatePhotoButton.gameObject.SetActive(hasPhotoInHand); // Mostra o nascondi il pulsante per ruotare
        lowerPhotoButton.gameObject.SetActive(hasPhotoInHand); // Mostra o nascondi il pulsante per abbassare
        placePhotoButton.gameObject.SetActive(hasPhotoInHand); // Mostra o nascondi il pulsante per piazzare nell'album
    
    }
}