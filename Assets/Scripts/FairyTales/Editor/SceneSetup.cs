using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FairyTales.Api;
using FairyTales.Audio;
using FairyTales.UI.Core;

namespace FairyTales.Editor
{
    public static class SceneSetup
    {
        [MenuItem("FairyTales/Setup Scene")]
        public static void Setup()
        {
            CreateServices();
            CreateAudio();
            CreateUICanvas();

            Debug.Log("[SceneSetup] Done! Don't forget to save the scene.");
        }

        // ── [Services] — ApiClient ─────────────────────────
        private static void CreateServices()
        {
            if (Object.FindAnyObjectByType<ApiClient>() != null) return;

            var go = new GameObject("[Services]");
            go.AddComponent<ApiClient>();
            Undo.RegisterCreatedObjectUndo(go, "Create Services");
        }

        // ── [Audio] — MicRecorder, NarrationPlayer, BGM ────
        private static void CreateAudio()
        {
            if (Object.FindAnyObjectByType<BackgroundMusicManager>() == null)
            {
                var bgm = new GameObject("[BackgroundMusic]");
                bgm.AddComponent<BackgroundMusicManager>();
                Undo.RegisterCreatedObjectUndo(bgm, "Create BGM");
            }

            if (Object.FindAnyObjectByType<MicRecorder>() != null) return;

            var go = new GameObject("[Audio]");
            go.AddComponent<MicRecorder>();
            go.AddComponent<NarrationPlayer>();
            Undo.RegisterCreatedObjectUndo(go, "Create Audio");
        }

        // ── UI Canvas + ScreenManager + screen stubs ───────
        private static void CreateUICanvas()
        {
            if (Object.FindAnyObjectByType<ScreenManager>() != null) return;

            // EventSystem
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }

            // Canvas
            var canvas = new GameObject("[UICanvas]");
            var c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 0;

            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvas.AddComponent<GraphicRaycaster>();

            var sm = canvas.AddComponent<ScreenManager>();
            Undo.RegisterCreatedObjectUndo(canvas, "Create UICanvas");

            // Screen stubs
            string[] screens =
            {
                "LanguageSelectScreen",
                "PersonalizationScreen",
                "LoadingScreen",
                "LibraryScreen",
                "TaleDetailScreen",
                "ReadingScreen",
                "NarrationSetupScreen",
                "VoiceRecordingScreen",
                "NarrationProgressScreen"
            };

            foreach (var name in screens)
                CreateScreenStub(canvas.transform, name);

            // Set initial screen reference via SerializedObject
            var so = new SerializedObject(sm);
            var prop = so.FindProperty("initialScreen");
            var first = canvas.transform.GetChild(0);
            prop.objectReferenceValue = first.GetComponent<CanvasGroup>();
            // CanvasGroup isn't BaseScreen yet — will be set when real screens exist
            so.ApplyModifiedProperties();
        }

        private static void CreateScreenStub(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            // Full-stretch RectTransform
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            go.AddComponent<CanvasGroup>();
            go.SetActive(false);
        }
    }
}
