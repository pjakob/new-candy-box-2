using UnityEngine;

namespace KawaiiCandyBox.Core
{
    /// <summary>
    /// Manages background music playback.
    /// Persists across scenes via DontDestroyOnLoad.
    /// Volume is driven by SaveData.musicVolume.
    /// </summary>
    public class AudioManager : SingletonManager<AudioManager>
    {
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioClip _backgroundMusic;

        protected override void OnInitialise()
        {
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }

            Debug.Log("[AudioManager] Initialised.");
        }

        public void StartMusic()
        {
            if (_backgroundMusic == null || _musicSource == null) return;

            float volume = SaveManager.Instance.Data.musicVolume;
            _musicSource.clip = _backgroundMusic;
            _musicSource.volume = volume;
            _musicSource.Play();

            Debug.Log("[AudioManager] Music started.");
        }

        public void SetMusicVolume(float volume)
        {
            if (_musicSource != null)
                _musicSource.volume = volume;

            SaveManager.Instance.Data.musicVolume = volume;
        }
    }
}
