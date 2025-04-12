using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName = "Gameplay";

    [Header("UI")]
    public Button playButton;

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(LoadScene);
        }
        else
        {
            Debug.LogWarning("Play button is not assigned in the inspector.");
        }
    }

    public void LoadScene()
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
