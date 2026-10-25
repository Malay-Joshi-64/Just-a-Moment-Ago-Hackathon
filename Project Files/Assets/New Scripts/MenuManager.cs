using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager I;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("First Level")]
    public string firstLevelScene = "Level1";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;

    [Header("Settings Sliders")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Settings Labels")]
    public TMP_Text masterVolumeLabel;
    public TMP_Text sfxVolumeLabel;
    public TMP_Text fullscreenLabel;

    [Header("Fullscreen")]
    public Toggle fullscreenToggle;

    const string MasterVolumeKey = "MasterVolume";
    const string SFXVolumeKey = "SFXVolume";
    const string FullscreenKey = "Fullscreen";

    bool loading;

    // =========================
    // AWAKE
    // =========================

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
    }

    // =========================
    // START
    // =========================

    void Start()
    {
        LoadSettings();

        ShowMainMenu();

        // Listen for slider changes
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(
                OnMasterVolumeChanged
            );
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(
                OnSFXVolumeChanged
            );
        }

        // Listen for fullscreen toggle
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(
                OnFullscreenChanged
            );
        }

        UpdateSettingsLabels();
    }

    // =========================
    // MAIN MENU
    // =========================

    void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        if (loading)
            return;

        PlayClickSound();

        loading = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene(firstLevelScene);
    }

    public void OpenSettings()
    {
        PlayClickSound();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        LoadSettings();
        UpdateSettingsLabels();
    }

    public void BackToMainMenu()
    {
        PlayClickSound();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        PlayClickSound();

        Debug.Log("QUIT GAME");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // =========================
    // SETTINGS
    // =========================

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

        bool fullscreen =
            PlayerPrefs.GetInt(
                FullscreenKey,
                1
            ) == 1;

        // Set sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = masterVolume;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVolume;

        // Set fullscreen
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = fullscreen;

        // Apply settings immediately
        AudioListener.volume = masterVolume;

        Screen.fullScreen = fullscreen;

        // Update SFX volume
        if (AudioManager.I != null)
            AudioManager.I.SyncVolume();

        UpdateSettingsLabels();
    }

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

        bool fullscreen =
            fullscreenToggle != null &&
            fullscreenToggle.isOn;

        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            masterVolume
        );

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            sfxVolume
        );

        PlayerPrefs.SetInt(
            FullscreenKey,
            fullscreen ? 1 : 0
        );

        PlayerPrefs.Save();

        // Apply immediately
        AudioListener.volume = masterVolume;

        Screen.fullScreen = fullscreen;

        if (AudioManager.I != null)
            AudioManager.I.SyncVolume();

        UpdateSettingsLabels();

        BackToMainMenu();
    }

    // =========================
    // LIVE SETTINGS
    // =========================

    void OnMasterVolumeChanged(float value)
    {
        // Change master volume immediately
        AudioListener.volume = value;

        UpdateSettingsLabels();
    }

    void OnSFXVolumeChanged(float value)
    {
        // Change SFX volume immediately
        if (AudioManager.I != null)
            AudioManager.I.SyncVolume();

        UpdateSettingsLabels();
    }

    void OnFullscreenChanged(bool value)
    {
        // Change fullscreen immediately
        Screen.fullScreen = value;

        UpdateSettingsLabels();
    }

    // =========================
    // SETTINGS LABELS
    // =========================

    void UpdateSettingsLabels()
    {
        if (masterVolumeLabel != null &&
            masterVolumeSlider != null)
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

        if (sfxVolumeLabel != null &&
            sfxVolumeSlider != null)
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

        if (fullscreenLabel != null &&
            fullscreenToggle != null)
        {
            fullscreenLabel.text =
                "Fullscreen : " +
                (
                    fullscreenToggle.isOn
                        ? "ON"
                        : "OFF"
                );
        }
    }

    // =========================
    // AUDIO
    // =========================

    void PlayClickSound()
    {
        if (audioSource != null &&
            buttonClickSound != null)
        {
            audioSource.PlayOneShot(
                buttonClickSound
            );
        }
    }
}