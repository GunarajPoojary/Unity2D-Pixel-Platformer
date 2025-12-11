using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMainMenu : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _creditsButton;
    [SerializeField] private Button _quitButton;

    private void OnEnable()
    {
        if (_playButton != null)
            _playButton.onClick.AddListener(OnPlayClicked);

        if (_optionsButton != null)
            _optionsButton.onClick.AddListener(OnOptionsClicked);

        if (_creditsButton != null)
            _creditsButton.onClick.AddListener(OnCreditsClicked);

        if (_quitButton != null)
            _quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnDisable()
    {
        if (_playButton != null)
            _playButton.onClick.RemoveListener(OnPlayClicked);

        if (_optionsButton != null)
            _optionsButton.onClick.RemoveListener(OnOptionsClicked);

        if (_creditsButton != null)
            _creditsButton.onClick.RemoveListener(OnCreditsClicked);

        if (_quitButton != null)
            _quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    private void OnPlayClicked()
    {
        Debug.Log("Play button clicked!");
        // Load your game scene - replace "GameScene" with your actual scene name
        // SceneManager.LoadScene("GameScene");

        // For now, just reload the current scene as a demo
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnOptionsClicked()
    {
        Debug.Log("Options button clicked!");
        // Load options menu scene or show options panel
        // You can implement this later
    }

    private void OnCreditsClicked()
    {
        Debug.Log("Credits button clicked!");
        // Load credits scene or show credits panel
        // You can implement this later
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit button clicked!");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}