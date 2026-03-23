using FairyTales.Cache;
using UnityEngine;

namespace FairyTales.UI.Library
{
    public static class CoverProvider
    {
        public static Sprite Get(string taleId)
        {
            // Bundled Resources first — no need to download
            var sprite = Resources.Load<Sprite>($"Covers/{taleId}");
            if (sprite != null) return sprite;

            // Fallback to server cache
            sprite = AssetCache.LoadSprite(AssetCache.CoverKey(taleId));
            if (sprite == null)
                Debug.LogWarning($"[CoverProvider] Missing: {taleId}");
            return sprite;
        }
    }
}
