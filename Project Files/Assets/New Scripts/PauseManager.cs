using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseManager : MonoBehaviour
{
    public static PauseManager I;

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject gameOverPanel;

    [Header("Win Screen")]
    public GameObject winScreen;

    [Header("Settings Sliders")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Settings Labels")]
    public TMP_Text masterVolumeLabel;
    public TMP_Text sfxVolumeLabel;

    const string MasterVolumeKey = "MasterVolume";
    const string SFXVolumeKey = "SFXVolume";

    bool isPaused;
    bool isGameOver;
    bool isWon;

    public bool InputLocked =>
        isPaused || isGameOver || isWon;

    public bool IsGameOver => isGameOver;
    public bool IsWon => isWon;

    // =========================================
    // AWAKE
    // =========================================

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(this);
            return;
        }

        I = this;

        // Hide all panels at startup
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winScreen != null)
            winScreen.SetActive(false);

        // Reset states
        isPaused = false;
        isGameOver = false;
        isWon = false;

        Time.timeScale = 1f;
    }

    // =========================================
    // START
    // =========================================

    void Start()
    {
        LoadSettings();

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(
                OnMasterVolumeChanged
            );

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(
                OnSFXVolumeChanged
            );

        UpdateSettingsLabels();
    }

    // =========================================
    // UPDATE
    // =========================================

    void Update()
    {
        // Game Over has priority
        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                TryAgain();
            }

            return;
        }

        // Win Screen has priority
        if (isWon)
        {
            return;
        }

        // Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    // =========================================
    // PAUSE
    // =========================================

    public void Pause()
    {
        if (isGameOver || isWon)
            return;

        isPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            pausePanel.transform.SetAsLastSibling();
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (isGameOver || isWon)
            return;

        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    // =========================================
    // SETTINGS
    // =========================================

    public void OpenSettings()
    {
        if (isGameOver || isWon || !isPaused)
            return;

        LoadSettings();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
        }

        UpdateSettingsLabels();
    }

    public void BackToPause()
    {
        if (isGameOver || isWon)
            return;

        SaveSettings();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            pausePanel.transform.SetAsLastSibling();
        }
    }

    // =========================================
    // LOAD SETTINGS
    // =========================================

    void LoadSettings()
    {
        float masterVolume =
            PlayerPrefs.GetFloat(
                MasterVolumeKey,
                1f
            );

        float sfxVolume =
            PlayerPrefs.GetFloat(
                SFXVolumeKey,
                1f
            );

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = masterVolume;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVolume;

        AudioListener.volume = masterVolume;

        if (AudioManager.I != null)
            AudioManager.I.SyncVolume();

        UpdateSettingsLabels();
    }

    // =========================================
    // SAVE SETTINGS
    // =========================================

    public void SaveSettings()
    {
        float masterVolume =
            masterVolumeSlider != null
                ? masterVolumeSlider.value
                : 1f;

        float sfxVolume =
            sfxVolumeSlider != null
                ? sfxVolumeSlider.value
                : 1f;

        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            masterVolume
        );

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            sfxVolume
        );

        PlayerPrefs.Save();

        AudioListener.volume = masterVolume;

        if (AudioManager.I != null)
            AudioManager.I.SyncVolume();

        UpdateSettingsLabels();
    }

    // =========================================
    // LIVE SETTINGS
    // =========================================

    void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;

        UpdateSettingsLabels();
    }

    void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.I != null)
            AudioManager.I.SyncVolume();

        UpdateSettingsLabels();
    }

    void UpdateSettingsLabels()
    {
        if (
            masterVolumeLabel != null &&
            masterVolumeSlider != null
        )
        {
            int percentage =
                Mathf.RoundToInt(
                    masterVolumeSlider.value * 100f
                );

            masterVolumeLabel.text =
                "Master Volume : " +
                percentage +
                "%";
        }

        if (
            sfxVolumeLabel != null &&
            sfxVolumeSlider != null
        )
        {
            int percentage =
                Mathf.RoundToInt(
                    sfxVolumeSlider.value * 100f
                );

            sfxVolumeLabel.text =
                "SFX Volume : " +
                percentage +
                "%";
        }
    }

    // =========================================
    // GAME OVER
    // =========================================

    public void ShowGameOver()
    {
        if (isGameOver)
            return;

        Debug.Log(
            "PauseManager: SHOWING GAME OVER"
        );

        // Reset other states
        isWon = false;
        isGameOver = true;
        isPaused = false;

        // Hide other panels
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (winScreen != null)
            winScreen.SetActive(false);

        // Show Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();

            Debug.Log(
                "PauseManager: GameOverPanel activated."
            );
        }
        else
        {
            Debug.LogError(
                "PauseManager: GameOverPanel is NOT assigned!"
            );
        }

        // Play Game Over sound
        if (AudioManager.I != null)
            AudioManager.I.PlayGameOver();
    }

    // =========================================
    // WIN SCREEN
    // =========================================

    public void ShowWinScreen()
    {
        if (isWon)
            return;

        Debug.Log(
            "PauseManager: SHOWING WIN SCREEN"
        );

        // Set states
        isWon = true;
        isPaused = false;
        isGameOver = false;

        // Freeze game
        Time.timeScale = 0f;

        // Hide other panels
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Show Win Screen
        if (winScreen != null)
        {
            winScreen.SetActive(true);
            winScreen.transform.SetAsLastSibling();

            Debug.Log(
                "PauseManager: WinScreen activated."
            );
        }
        else
        {
            Debug.LogError(
                "PauseManager: WinScreen is NOT assigned!"
            );
        }
    }

    // =========================================
    // NEXT LEVEL
    // =========================================

    public void NextLevel()
    {
        Debug.Log(
            "PauseManager: Loading next level..."
        );

        Time.timeScale = 1f;

        int currentScene =
            SceneManager.GetActiveScene().buildIndex;

        int lastScene =
            SceneManager.sceneCountInBuildSettings - 1;

        if (currentScene < lastScene)
        {
            SceneManager.LoadScene(
                currentScene + 1
            );
        }
        else
        {
            Debug.Log(
                "No more levels!"
            );
        }
    }

    // =========================================
    // REPLAY LEVEL
    // =========================================

    public void ReplayLevel()
    {
        Debug.Log(
            "PauseManager: Replaying level..."
        );

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================
    // RESTART AFTER GAME OVER
    // =========================================

    public void TryAgain()
    {
        Debug.Log(
            "Restarting level..."
        );

        // Hide Game Over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Reset states
        isGameOver = false;
        isPaused = false;
        isWon = false;

        // Resume game
        Time.timeScale = 1f;

        // Reload current level
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================
    // MAIN MENU
    // =========================================

    public void BackToMainMenu()
    {
        SaveSettings();

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "MainMenu"
        );
    }
}