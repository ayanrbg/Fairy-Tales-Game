using UnityEngine;

namespace FairyTales.UI.Reading
{
    public static class IllustrationProvider
    {
        private const string BasePath = "Illustrations";

        public static Sprite GetPage(string taleId, int page)
        {
            var sprite = Resources.Load<Sprite>($"{BasePath}/{taleId}/page_{page}");
            if (sprite == null)
                Debug.LogWarning($"[Illustration] Missing: {BasePath}/{taleId}/page_{page}");
            return sprite;
        }

        public static Sprite GetThumbnail(string taleId, int page)
        {
            // Use same sprite for thumbnails; swap to separate folder if needed
            return GetPage(taleId, page);
        }
    }
}
