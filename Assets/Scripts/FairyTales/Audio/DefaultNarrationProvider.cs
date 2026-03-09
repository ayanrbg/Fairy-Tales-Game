using UnityEngine;

namespace FairyTales.Audio
{
    public class DefaultNarrationProvider
    {
        // Expects clips at: Resources/Audio/Default/{taleId}/page_{page}
        public AudioClip GetPage(string taleId, int page)
        {
            var path = $"Audio/Default/{taleId}/page_{page}";
            var clip = Resources.Load<AudioClip>(path);
            if (clip == null)
                Debug.LogWarning($"[DefaultNarration] Missing: {path}");
            return clip;
        }

        public bool HasNarration(string taleId, int page)
        {
            var path = $"Audio/Default/{taleId}/page_{page}";
            return Resources.Load<AudioClip>(path) != null;
        }

        public bool HasAnyNarration(string taleId)
        {
            return Resources.Load<AudioClip>($"Audio/Default/{taleId}/page_0") != null;
        }
    }
}
