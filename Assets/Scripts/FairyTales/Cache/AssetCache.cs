using System.IO;
using UnityEngine;

namespace FairyTales.Cache
{
    public static class AssetCache
    {
        private const int CacheVersion = 3; // v3: images stored as JPEG (was PNG)
        private static string Root => Path.Combine(Application.persistentDataPath, "cache");

        /// Call once at startup to wipe stale cache after format change.
        public static void MigrateIfNeeded()
        {
            var key = "cache_version";
            if (PlayerPrefs.GetInt(key, 0) >= CacheVersion) return;
            ClearAll();
            PlayerPrefs.SetInt(key, CacheVersion);
            PlayerPrefs.Save();
            Debug.Log($"[AssetCache] Migrated to v{CacheVersion}, old cache cleared");
        }

        // ── Keys ─────────────────────────────────────────────
        public static string CoverKey(string taleId)
            => $"covers/{taleId}";

        public static string IllustrationKey(string taleId, int page)
            => $"illustrations/{taleId}/page_{page}";

        public static string NarrationKey(string taleId, string lang, int page)
            => $"narration/{taleId}/{lang}/page_{page}.mp3";

        // ── Queries ──────────────────────────────────────────
        public static bool Exists(string key)
            => File.Exists(GetPath(key));

        public static string GetPath(string key)
            => Path.Combine(Root, key);

        public static string TaleReadyKey(string taleId)
            => $"tales/{taleId}.ready";

        public static bool IsCoverDownloaded(string taleId)
            => Exists(CoverKey(taleId))
               || Resources.Load<Sprite>($"Covers/{taleId}") != null;

        public static bool IsTaleDownloaded(string taleId)
        {
            // Available in bundled Resources — no download needed
            if (Resources.Load<Sprite>($"Illustrations/{taleId}/page_0") != null)
                return true;

            if (!Exists(TaleReadyKey(taleId))) return false;
            // Verify at least the first illustration actually exists
            if (!Exists(IllustrationKey(taleId, 0)))
            {
                Delete(TaleReadyKey(taleId));
                Debug.LogWarning($"[AssetCache] Stale .ready for {taleId}, re-download needed");
                return false;
            }
            return true;
        }

        public static void MarkTaleReady(string taleId)
            => Save(TaleReadyKey(taleId), new byte[] { 1 });

        // ── Read / Write ─────────────────────────────────────
        public static void Save(string key, byte[] data)
        {
            var path = GetPath(key);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, data);
        }

        public static byte[] Load(string key)
        {
            var path = GetPath(key);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public static Sprite LoadSprite(string key)
        {
            var bytes = Load(key);
            if (bytes == null || bytes.Length < 8) return null;

            var tex = new Texture2D(2, 2);
            if (!tex.LoadImage(bytes))
            {
                // Corrupted file — delete so it can be re-downloaded
                Delete(key);
                return null;
            }

            return Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
        }

        public static void Delete(string key)
        {
            var path = GetPath(key);
            if (File.Exists(path)) File.Delete(path);
        }

        // ── Maintenance ──────────────────────────────────────
        public static void ClearAll()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, true);
        }
    }
}
