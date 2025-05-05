using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private AudioClip cursorSound;
    private AudioClip clickSound;
    private float cursorSoundCooldown = 0.1f;
    private float lastCursorSoundTime = -1f;
    private AudioSource loopedSFXSource;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;

        sfxSource = gameObject.AddComponent<AudioSource>();

        loopedSFXSource = gameObject.AddComponent<AudioSource>();
        loopedSFXSource.loop = true;
        
        cursorSound = Resources.Load<AudioClip>("Sounds/cursor_sound");
        clickSound = Resources.Load<AudioClip>("Sounds/click_sound");

        AudioClip mainMenuSong = Resources.Load<AudioClip>("Sounds/main_menu_song");
        if (mainMenuSong != null)
        {
            PlayMusic(mainMenuSong);
        }
    }

    public void PlayCursorSound()
    {
        if (cursorSound == null) return;

        if (Time.time - lastCursorSoundTime >= cursorSoundCooldown)
        {
            sfxSource.PlayOneShot(cursorSound);
            lastCursorSoundTime = Time.time;
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null) sfxSource.PlayOneShot(clickSound);
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null) return;

        if (musicSource.isPlaying)
            musicSource.Stop();

        musicSource.clip = musicClip;
        musicSource.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }

    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxClip != null)
        {
            sfxSource.PlayOneShot(sfxClip);
        }
    }

    public void PlayLoopedSFX(AudioClip clip)
    {
        if (clip == null) return;

        if (loopedSFXSource.isPlaying)
            loopedSFXSource.Stop();

        loopedSFXSource.clip = clip;
        loopedSFXSource.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        loopedSFXSource.Play();
    }

    public void StopLoopedSFX()
    {
        if (loopedSFXSource.isPlaying)
            loopedSFXSource.Stop();
    }

    public void StopAllSFX()
    {
        sfxSource.Stop();
        loopedSFXSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

        public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
