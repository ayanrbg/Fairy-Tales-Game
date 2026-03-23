using FairyTales.Cache;
using UnityEngine;

namespace FairyTales.UI.Reading
{
    public static class IllustrationProvider
    {
        public static Sprite GetPage(string taleId, int page)
        {
            // Try cache first (downloaded from server)
            var sprite = AssetCache.LoadSprite(AssetCache.IllustrationKey(taleId, page));
            if (sprite != null) return sprite;

            // Fallback to Resources (local dev)
            sprite = Resources.Load<Sprite>($"Illustrations/{taleId}/page_{page}");
            if (sprite == null)
                Debug.LogWarning($"[Illustration] Missing: {taleId}/page_{page}");
            return sprite;
        }

        public static Sprite GetThumbnail(string taleId, int page)
        {
            return GetPage(taleId, page);
        }
    }
}
