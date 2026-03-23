using FairyTales.Cache;
using UnityEngine;

namespace FairyTales.Audio
{
    public class DefaultNarrationProvider
    {
        private string _lang;

        public DefaultNarrationProvider()
        {
            _lang = PlayerPrefs.GetString("ft_lang", "ru");
        }

        public void SetLang(string lang) => _lang = lang;

        /// <summary>
        /// Returns cached audio bytes for default narration, or null.
        /// Use with NarrationPlayer.PlayFromBytes().
        /// </summary>
        public byte[] GetPageBytes(string taleId, int page)
        {
            return AssetCache.Load(AssetCache.NarrationKey(taleId, _lang, page));
        }

        /// <summary>
        /// Legacy sync method — loads from Resources only.
        /// </summary>
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
            return AssetCache.Exists(AssetCache.NarrationKey(taleId, _lang, page))
                || Resources.Load<AudioClip>($"Audio/Default/{taleId}/page_{page}") != null;
        }

        public bool HasAnyNarration(string taleId)
        {
            return HasNarration(taleId, 0);
        }
    }
}
