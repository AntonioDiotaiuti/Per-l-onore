using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Loading : MonoBehaviour
{
    public string gameplaySceneName;           // Nome della scena di gioco
    public TextMeshProUGUI narrativeText;      // Riferimento al TextMeshProUGUI per il testo narrativo
    public float typingSpeed = 0.05f;          // Velocità di scrittura
    public AudioSource typingSound;            // Suono di scrittura (opzionale)

    void Start()
    {
        StartCoroutine(PlayNarrativeSequence());
    }

    private IEnumerator PlayNarrativeSequence()
    {
        narrativeText.text = "";  // Inizializza il testo

        // Aggiungi il testo narrativo
        string fullNarrative = "In un giorno di pioggia, mentre rovistavi fra vecchi libri dimenticati...\n" +
                               "ti imbatti in un vecchio album impolverato.\n\n" +
                               "Lo apri con delicatezza, ma la copertina cede:\n" +
                               "una cascata di fotografie si riversa a terra, confondendosi fra loro.\n\n" +
                               "Ora tocca a te\n" +
                               "rimettere insieme i pezzi di questa storia.\n" +
                               "Foto dopo foto. Ricordo dopo ricordo.";

        // Visualizza il testo un carattere alla volta
        yield return StartCoroutine(TypeText(fullNarrative));

        yield return new WaitForSeconds(1f);  // Pausa finale

        // Carica la scena di gioco
        SceneManager.LoadScene(gameplaySceneName);
    }

    private IEnumerator TypeText(string text)
    {
        foreach (char c in text)
        {
            narrativeText.text += c;  // Aggiungi un carattere
            if (typingSound != null && !char.IsWhiteSpace(c))
            {
                typingSound.Play();  // Suono di scrittura (se presente)
            }
            yield return new WaitForSeconds(typingSpeed);  // Pausa per la velocità di scrittura
        }
    }
}

