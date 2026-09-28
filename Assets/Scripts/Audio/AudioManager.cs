using UnityEngine;

namespace RowdyRobots
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip battleMusic;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

        [Header("SFX")]
        [SerializeField] private AudioClip deathSFX;
        [SerializeField] private AudioClip alienAttackSFX;
        [SerializeField] private AudioClip pickupItemSFX;
        [SerializeField] private AudioClip taskCompletedSFX;
        [SerializeField] private AudioClip gameWinSFX;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

        private AudioSource m_musicSource;
        private AudioSource m_sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            m_musicSource = gameObject.AddComponent<AudioSource>();
            m_musicSource.loop = true;
            m_musicSource.playOnAwake = false;
            m_musicSource.volume = musicVolume;

            m_sfxSource = gameObject.AddComponent<AudioSource>();
            m_sfxSource.loop = false;
            m_sfxSource.playOnAwake = false;
            m_sfxSource.volume = sfxVolume;
        }

        private void Start()
        {
            PlayMainMenuMusic();
        }

        // ── Music ─────────────────────────────────────────────

        public void PlayMainMenuMusic()
        {
            PlayMusic(mainMenuMusic);
        }

        public void PlayBattleMusic()
        {
            PlayMusic(battleMusic);
        }

        private void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            if (m_musicSource.clip == clip && m_musicSource.isPlaying) return;

            m_musicSource.clip = clip;
            m_musicSource.Play();
        }

        public void StopMusic()
        {
            m_musicSource.Stop();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            m_musicSource.volume = musicVolume;
        }

        // ── SFX ───────────────────────────────────────────────

        public void PlayDeathSFX()         => PlaySFX(deathSFX);
        public void PlayAlienAttackSFX()   => PlaySFX(alienAttackSFX);
        public void PlayPickupItemSFX()    => PlaySFX(pickupItemSFX);
        public void PlayTaskCompletedSFX() => PlaySFX(taskCompletedSFX);
        public void PlayGameWinSFX()
        {
            StopMusic();
            PlaySFX(gameWinSFX);
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            m_sfxSource.volume = sfxVolume;
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            m_sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }
}
