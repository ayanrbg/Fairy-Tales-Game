using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace FairyTales.Audio
{
    public class NarrationPlayer : MonoBehaviour
    {
        private AudioSource _source;
        private Coroutine _playRoutine;
        private string _tempPath;

        public bool IsPlaying => _source != null && _source.isPlaying;
        public float Progress => _source != null && _source.clip != null
            ? _source.time / _source.clip.length : 0f;

        public event Action OnFinished;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _tempPath = Path.Combine(Application.temporaryCachePath, "narration.wav");
        }

        public void PlayFromBytes(byte[] audioData)
        {
            Stop();
            _playRoutine = StartCoroutine(LoadAndPlay(audioData));
        }

        public void PlayClip(AudioClip clip)
        {
            Stop();
            _source.clip = clip;
            _source.Play();
            _playRoutine = StartCoroutine(WaitForEnd());
        }

        public void Pause() => _source.Pause();
        public void Resume() => _source.UnPause();

        public void Stop()
        {
            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }
            _source.Stop();
            _source.clip = null;
        }

        private IEnumerator LoadAndPlay(byte[] data)
        {
            File.WriteAllBytes(_tempPath, data);

            var uri = "file:///" + _tempPath.Replace("\\", "/");
            using var request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.UNKNOWN);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[NarrationPlayer] {request.error}");
                yield break;
            }

            var clip = DownloadHandlerAudioClip.GetContent(request);
            _source.clip = clip;
            _source.Play();
            yield return WaitForEnd();
        }

        private IEnumerator WaitForEnd()
        {
            while (_source.isPlaying)
                yield return null;

            _playRoutine = null;
            OnFinished?.Invoke();
        }
    }
}
