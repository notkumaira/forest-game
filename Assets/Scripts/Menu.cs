using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    public Button backButton;
    public Canvas settingsCanvas;
    public Canvas gameCanvas;
    public Slider slider;
    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(ExitGame);
        backButton.onClick.AddListener(BackToMenu);
        settingsCanvas.enabled = false; 
        gameCanvas.enabled = true;
        slider.value = PlayerPrefs.GetFloat("Volume", 1f);
        AudioListener.volume = slider.value;

        slider.onValueChanged.AddListener(SetVolume);
    }
    
    void StartGame()
    {
        SceneManager.LoadScene("Cave");
    }

    void OpenSettings()
    {
        settingsCanvas.enabled = true;
        gameCanvas.enabled = false;
    }

    void ExitGame()
    {
        Application.Quit();
    }

    void BackToMenu()
    {
        settingsCanvas.enabled = false;
        gameCanvas.enabled = true;
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
    }
}
