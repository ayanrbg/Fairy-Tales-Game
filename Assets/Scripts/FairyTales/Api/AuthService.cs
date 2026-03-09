using System;
using System.Collections;
using FairyTales.Models;
using UnityEngine;

namespace FairyTales.Api
{
    public class AuthService
    {
        private readonly ApiClient _api;

        public AuthService(ApiClient api) => _api = api;

        public IEnumerator Login(string userId,
            Action onSuccess = null, Action<string> onError = null)
        {
            var body = JsonUtility.ToJson(new LoginRequest { userId = userId });

            yield return _api.PostJson("/api/auth/login", body,
                json =>
                {
                    var response = JsonUtility.FromJson<LoginResponse>(json);
                    _api.SetToken(response.token);
                    PlayerPrefs.SetString("ft_token", response.token);
                    PlayerPrefs.SetString("ft_userId", userId);
                    PlayerPrefs.Save();
                    onSuccess?.Invoke();
                },
                onError
            );
        }

        public IEnumerator Register(string userId, string name, string gender, string lang,
            Action<ProfileData> onSuccess = null, Action<string> onError = null)
        {
            var body = JsonUtility.ToJson(new RegisterRequest
            {
                userId = userId, name = name, gender = gender, lang = lang
            });

            yield return _api.PostJson("/api/auth/register", body,
                json =>
                {
                    var response = JsonUtility.FromJson<RegisterResponse>(json);
                    _api.SetToken(response.token);
                    PlayerPrefs.SetString("ft_token", response.token);
                    PlayerPrefs.SetString("ft_userId", userId);
                    PlayerPrefs.Save();
                    onSuccess?.Invoke(response.profile);
                },
                onError
            );
        }

        public IEnumerator GetProfile(
            Action<ProfileData> onSuccess, Action<string> onError = null)
        {
            yield return _api.Get("/api/user/profile",
                json =>
                {
                    var profile = JsonUtility.FromJson<ProfileData>(json);
                    onSuccess?.Invoke(profile);
                },
                onError
            );
        }

        public IEnumerator UpdateProfile(string name = null, string gender = null, string lang = null,
            Action<ProfileData> onSuccess = null, Action<string> onError = null)
        {
            var body = JsonUtility.ToJson(new ProfileUpdateRequest
            {
                name = name, gender = gender, lang = lang
            });

            yield return _api.PutJson("/api/user/profile", body,
                json =>
                {
                    var response = JsonUtility.FromJson<ProfileUpdateResponse>(json);
                    onSuccess?.Invoke(response.profile);
                },
                onError
            );
        }

        public bool TryRestoreSession()
        {
            var token = PlayerPrefs.GetString("ft_token", "");
            if (string.IsNullOrEmpty(token)) return false;
            _api.SetToken(token);
            return true;
        }

        public void Logout()
        {
            _api.SetToken(null);
            PlayerPrefs.DeleteKey("ft_token");
            PlayerPrefs.DeleteKey("ft_userId");
        }
    }
}
