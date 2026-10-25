using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I;

    [Header("Audio Source")]
    public AudioSource sfxSource;

    [Header("SFX Clips")]
    public AudioClip buttonClick;
    public AudioClip jump;
    public AudioClip roll;
    public AudioClip shadowAction;
    public AudioClip platePressed;
    public AudioClip doorOpen;
    public AudioClip playerHit;
    public AudioClip shadowHit;
    public AudioClip walk;
    public AudioClip win;
    public AudioClip gameOver;

    const string SFXVolumeKey = "SFXVolume";

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(this);
            return;
        }

        I = this;

        // Load SFX volume from PlayerPrefs
        SyncVolume();
    }

    void Update()
    {
        // Keep AudioManager synced with PlayerPrefs
        SyncVolume();
    }

    public void SyncVolume()
    {
        if (sfxSource == null)
            return;

        float sfxVolume =
            PlayerPrefs.GetFloat(SFXVolumeKey, 1f);

        sfxSource.volume = sfxVolume;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick()
    {
        Play(buttonClick);
    }

    public void PlayJump()
    {
        Play(jump);
    }

    public void PlayRoll()
    {
        Play(roll);
    }

    public void PlayShadowAction()
    {
        Play(shadowAction);
    }

    public void PlayPlatePressed()
    {
        Play(platePressed);
    }

    public void PlayDoorOpen()
    {
        Play(doorOpen);
    }

    public void PlayPlayerHit()
    {
        Play(playerHit);
    }

    public void PlayShadowHit()
    {
        Play(shadowHit);
    }

    public void PlayWin()
    {
        Play(win);
    }

    public void PlayWalk()
    {
        Play(walk);
    }

    public void PlayGameOver()
    {
        Play(gameOver);
    }
}