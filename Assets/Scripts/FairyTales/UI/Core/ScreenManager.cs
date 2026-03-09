using System.Collections.Generic;
using UnityEngine;

namespace FairyTales.UI.Core
{
    public class ScreenManager : MonoBehaviour
    {
        [SerializeField] private BaseScreen initialScreen;

        private readonly Dictionary<System.Type, BaseScreen> _screens = new();
        private BaseScreen _current;

        private void Awake()
        {
            var all = GetComponentsInChildren<BaseScreen>(true);
            foreach (var screen in all)
            {
                _screens[screen.GetType()] = screen;
                screen.HideImmediate();
            }
        }

        private void Start()
        {
            if (initialScreen != null)
                initialScreen.ShowImmediate();

            _current = initialScreen;
        }

        public void Show<T>() where T : BaseScreen
        {
            if (!_screens.TryGetValue(typeof(T), out var next))
            {
                Debug.LogError($"[ScreenManager] Screen not found: {typeof(T).Name}");
                return;
            }

            if (next == _current) return;

            var prev = _current;
            _current = next;

            if (prev != null)
                prev.Hide(() => next.Show());
            else
                next.Show();
        }

        public void ShowImmediate<T>() where T : BaseScreen
        {
            if (!_screens.TryGetValue(typeof(T), out var next)) return;
            if (next == _current) return;

            _current?.HideImmediate();
            _current = next;
            next.ShowImmediate();
        }

        public T Get<T>() where T : BaseScreen
        {
            _screens.TryGetValue(typeof(T), out var screen);
            return screen as T;
        }

        public BaseScreen Current => _current;
    }
}
