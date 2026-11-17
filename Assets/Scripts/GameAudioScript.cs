using UnityEngine;

public class GameAudioScript : MonoBehaviour
{
    public static GameAudioScript Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    GameDataScript gameData;

    void Awake()
    {
        Instance = this;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var playerScript = player.GetComponent<PlayerScript>();
            if (playerScript != null)
                gameData = playerScript.gameData;
        }

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            var sfxObject = new GameObject("SFX Audio");
            sfxObject.transform.SetParent(transform, false);
            sfxSource = sfxObject.AddComponent<AudioSource>();
        }

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        ApplySettings();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ApplySettings()
    {
        if (gameData == null)
            return;

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.volume = gameData.musicVolume;

            if (gameData.music)
            {
                if (!musicSource.isPlaying)
                    musicSource.Play();
            }
            else if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        if (sfxSource != null)
            sfxSource.volume = gameData.soundVolume;
    }

    public void SetMusicEnabled(bool enabled)
    {
        if (gameData == null)
            return;

        gameData.music = enabled;
        ApplySettings();
    }

    public void SetSoundEnabled(bool enabled)
    {
        if (gameData == null)
            return;

        gameData.sound = enabled;
        ApplySettings();
    }

    public void SetMusicVolume(float volume)
    {
        if (gameData == null)
            return;

        gameData.musicVolume = Mathf.Clamp01(volume);
        ApplySettings();
    }

    public void SetSoundVolume(float volume)
    {
        if (gameData == null)
            return;

        gameData.soundVolume = Mathf.Clamp01(volume);
        ApplySettings();
    }

    public void ToggleMusic()
    {
        if (gameData != null)
            SetMusicEnabled(!gameData.music);
    }

    public void ToggleSound()
    {
        if (gameData != null)
            SetSoundEnabled(!gameData.sound);
    }

    public void PlayEffect(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null || gameData == null || !gameData.sound)
            return;

        sfxSource.PlayOneShot(clip, volumeScale);
    }
}
