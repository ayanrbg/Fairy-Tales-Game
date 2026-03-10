using FairyTales.UI.Core;
using UnityEngine;

namespace FairyTales.Audio
{
    public class BackgroundMusicManager : MonoBehaviour
    {
        private static BackgroundMusicManager _instance;
        public static BackgroundMusicManager Instance => _instance;

        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private float volume = 0.5f;

        private AudioSource _source;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            volume = ReadingState.LoadVolume();
            _source.volume = volume;
            _source.mute = ReadingState.LoadMuted();
        }

        public void PlayMenu()
        {
            if (menuMusic != null) Play(menuMusic);
        }

        public void PlayForTale(string taleId)
        {
            var clip = Resources.Load<AudioClip>($"Music/{taleId}");
            if (clip != null) Play(clip);
            else Debug.LogWarning($"[BGM] No music for tale: {taleId}");
        }

        public void Play(AudioClip clip)
        {
            if (_source.clip == clip && _source.isPlaying) return;
            _source.clip = clip;
            _source.Play();
        }

        public void Stop() => _source.Stop();

        public void SetVolume(float v)
        {
            volume = Mathf.Clamp01(v);
            _source.volume = volume;
            ReadingState.SaveVolume(volume);
        }

        public void SetMuted(bool muted)
        {
            _source.mute = muted;
            ReadingState.SaveMuted(muted);
        }
        public bool IsMuted => _source.mute;
    }
}
