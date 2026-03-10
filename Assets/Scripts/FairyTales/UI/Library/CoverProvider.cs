using UnityEngine;

namespace FairyTales.UI.Library
{
    public static class CoverProvider
    {
        private const string BasePath = "Covers";

        public static Sprite Get(string taleId)
        {
            var sprite = Resources.Load<Sprite>($"{BasePath}/{taleId}");
            if (sprite == null)
                Debug.LogWarning($"[CoverProvider] Missing: {BasePath}/{taleId}");
            return sprite;
        }
    }
}
