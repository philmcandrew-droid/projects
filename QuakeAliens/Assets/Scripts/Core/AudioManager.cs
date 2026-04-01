using UnityEngine;

namespace QuakeAliens
{
    /// <summary>
    /// Handles game audio - music, ambient sounds, and SFX
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip battleMusic;
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip defeatMusic;

        [Header("Ambient")]
        [SerializeField] private AudioClip arenaAmbient;

        [Header("UI Sounds")]
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip menuOpen;
        [SerializeField] private AudioClip menuClose;

        [Header("Settings")]
        [SerializeField] private float musicVolume = 0.5f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float ambientVolume = 0.3f;
        [SerializeField] private float musicFadeSpeed = 1f;

        private AudioSource musicSource;
        private AudioSource ambientSource;
        private AudioSource sfxSource;
        private float targetMusicVolume;

        public static AudioManager Instance { get; private set; }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                if (musicSource != null)
                    musicSource.volume = musicVolume;
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set => sfxVolume = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetupAudioSources()
        {
            // Music source
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.playOnAwake = false;

            // Ambient source
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.volume = ambientVolume;
            ambientSource.playOnAwake = false;

            // SFX source
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        private void Update()
        {
            // Smooth music volume transitions
            if (musicSource != null && Mathf.Abs(musicSource.volume - targetMusicVolume) > 0.01f)
            {
                musicSource.volume = Mathf.Lerp(musicSource.volume, targetMusicVolume, Time.deltaTime * musicFadeSpeed);
            }
        }

        public void PlayMusic(MusicType type)
        {
            AudioClip clip = GetMusicClip(type);
            if (clip != null && musicSource.clip != clip)
            {
                musicSource.clip = clip;
                musicSource.Play();
                targetMusicVolume = musicVolume;
            }
        }

        public void StopMusic(bool fade = true)
        {
            if (fade)
            {
                targetMusicVolume = 0f;
                Invoke(nameof(StopMusicSource), 1f / musicFadeSpeed);
            }
            else
            {
                musicSource.Stop();
            }
        }

        private void StopMusicSource()
        {
            musicSource.Stop();
            musicSource.volume = musicVolume;
        }

        public void PlayAmbient()
        {
            if (arenaAmbient != null)
            {
                ambientSource.clip = arenaAmbient;
                ambientSource.Play();
            }
        }

        public void StopAmbient()
        {
            ambientSource.Stop();
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
            }
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, sfxVolume * volumeScale);
            }
        }

        public void PlayUISound(UISoundType type)
        {
            AudioClip clip = null;
            switch (type)
            {
                case UISoundType.ButtonClick:
                    clip = buttonClick;
                    break;
                case UISoundType.MenuOpen:
                    clip = menuOpen;
                    break;
                case UISoundType.MenuClose:
                    clip = menuClose;
                    break;
            }

            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        private AudioClip GetMusicClip(MusicType type)
        {
            switch (type)
            {
                case MusicType.Menu: return menuMusic;
                case MusicType.Battle: return battleMusic;
                case MusicType.Boss: return bossMusic;
                case MusicType.Victory: return victoryMusic;
                case MusicType.Defeat: return defeatMusic;
                default: return null;
            }
        }
    }

    public enum MusicType
    {
        Menu,
        Battle,
        Boss,
        Victory,
        Defeat
    }

    public enum UISoundType
    {
        ButtonClick,
        MenuOpen,
        MenuClose
    }
}

