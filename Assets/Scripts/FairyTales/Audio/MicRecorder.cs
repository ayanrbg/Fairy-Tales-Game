using System;
using System.IO;
using UnityEngine;

namespace FairyTales.Audio
{
    public class MicRecorder : MonoBehaviour
    {
        [SerializeField] private int maxDuration = 30;
        [SerializeField] private int sampleRate = 44100;

        private AudioClip _clip;
        private string _device;
        private float _startTime;

        public bool IsRecording { get; private set; }
        public float ElapsedTime => IsRecording ? Time.time - _startTime : 0f;

        public event Action OnStarted;
        public event Action<byte[]> OnStopped;

        public void StartRecording()
        {
            if (IsRecording) return;

            _device = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
            if (_device == null)
            {
                Debug.LogError("[MicRecorder] No microphone found");
                return;
            }

            _clip = Microphone.Start(_device, false, maxDuration, sampleRate);
            _startTime = Time.time;
            IsRecording = true;
            OnStarted?.Invoke();
        }

        public void StopRecording()
        {
            if (!IsRecording) return;

            var position = Microphone.GetPosition(_device);
            Microphone.End(_device);
            IsRecording = false;

            if (position == 0)
            {
                OnStopped?.Invoke(null);
                return;
            }

            var wavBytes = ClipToWav(_clip, position);
            OnStopped?.Invoke(wavBytes);
        }

        private void Update()
        {
            if (IsRecording && ElapsedTime >= maxDuration)
                StopRecording();
        }

        private byte[] ClipToWav(AudioClip clip, int samples)
        {
            var data = new float[samples * clip.channels];
            clip.GetData(data, 0);

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            var byteCount = data.Length * 2;
            WriteWavHeader(writer, clip.channels, sampleRate, byteCount);

            foreach (var sample in data)
            {
                var value = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                writer.Write(value);
            }

            return stream.ToArray();
        }

        private void WriteWavHeader(BinaryWriter w, int channels, int rate, int dataSize)
        {
            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(36 + dataSize);
            w.Write(new[] { 'W', 'A', 'V', 'E' });
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(16);                           // subchunk size
            w.Write((short)1);                     // PCM
            w.Write((short)channels);
            w.Write(rate);
            w.Write(rate * channels * 2);          // byte rate
            w.Write((short)(channels * 2));        // block align
            w.Write((short)16);                    // bits per sample
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(dataSize);
        }

        private void OnDestroy()
        {
            if (IsRecording) Microphone.End(_device);
        }
    }
}
