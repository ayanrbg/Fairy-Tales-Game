using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Api;
using FairyTales.Models;
using FairyTales.UI.Core;
using FairyTales.UI.Library;

namespace FairyTales.UI.Onboarding
{
    public class LoadingScreen : BaseScreen
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text statusText;

        private ScreenManager _screens;
        private ApiClient _api;
        private AuthService _auth;
        private TalesService _tales;

        public event Action OnLoadingComplete;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();
            _api = FindAnyObjectByType<ApiClient>();
            _auth = new AuthService(_api);
            _tales = new TalesService(_api);
        }

        protected override void OnShown()
        {
            SetProgress(0f, "Подготавливаем библиотеку...");
            StartCoroutine(RunOnboarding());
        }

        private IEnumerator RunOnboarding()
        {
            var lang = PlayerPrefs.GetString("ft_lang", "ru");
            var childName = PlayerPrefs.GetString("ft_childName", "");
            var gender = PlayerPrefs.GetString("ft_gender", "male");
            var userId = GetOrCreateUserId();

            // Step 1 — Register
            SetProgress(0.1f, "Создаём профиль...");
            string error = null;
            yield return _auth.Register(userId, childName, gender, lang,
                onSuccess: _ => { },
                onError: e => error = e);

            if (error != null)
            {
                Debug.LogError($"[Loading] Register failed: {error}");
                SetProgress(0f, "Ошибка регистрации");
                yield break;
            }

            // Step 2 — Fetch tales
            SetProgress(0.3f, "Загружаем сказки...");
            TaleSummary[] tales = null;
            yield return _tales.GetTales(lang,
                onSuccess: t => tales = t,
                onError: e => error = e);

            if (error != null)
            {
                Debug.LogError($"[Loading] GetTales failed: {error}");
                SetProgress(0.3f, "Ошибка загрузки");
                yield break;
            }

            // Step 3 — Personalize each tale
            if (tales != null && tales.Length > 0)
            {
                float step = 0.6f / tales.Length;
                for (int i = 0; i < tales.Length; i++)
                {
                    SetProgress(0.3f + step * i,
                        "Персонализируем текст...");

                    yield return _tales.Personalize(tales[i].id, childName, gender,
                        onSuccess: _ => { },
                        onError: e => Debug.LogWarning(
                            $"[Loading] Personalize {tales[i].id}: {e}"));
                }
            }

            SetProgress(1f, "Готово!");
            yield return new WaitForSeconds(0.5f);

            OnLoadingComplete?.Invoke();
            PlayerPrefs.SetInt("ft_onboarded", 1);
            PlayerPrefs.Save();

            _screens.Show<LibraryScreen>();
        }

        private void SetProgress(float value, string text)
        {
            if (progressBar) progressBar.value = value;
            if (statusText) statusText.text = text;
        }

        private string GetOrCreateUserId()
        {
            var id = PlayerPrefs.GetString("ft_userId", "");
            if (!string.IsNullOrEmpty(id)) return id;

            id = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("ft_userId", id);
            return id;
        }
    }
}
