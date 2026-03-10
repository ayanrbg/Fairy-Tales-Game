using System.Collections.Generic;
using UnityEngine;

namespace FairyTales.UI.Core
{
    public static class Loc
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Strings = new()
        {
            ["no_narration"] = new() { ["ru"] = "Нет озвучки. Нажмите «Озвучить»", ["kz"] = "Дыбыстау жоқ. «Озвучить» басыңыз", ["en"] = "No narration. Tap \"Narrate\"" },
            ["read"] = new() { ["ru"] = "Читать", ["kz"] = "Оқу", ["en"] = "Read" },
            ["listen"] = new() { ["ru"] = "Слушать", ["kz"] = "Тыңдау", ["en"] = "Listen" },
            ["narrate"] = new() { ["ru"] = "Озвучить", ["kz"] = "Дыбыстау", ["en"] = "Narrate" },
            ["settings"] = new() { ["ru"] = "Настройки", ["kz"] = "Баптаулар", ["en"] = "Settings" },
            ["continue"] = new() { ["ru"] = "Продолжить", ["kz"] = "Жалғастыру", ["en"] = "Continue" },
            ["boy"] = new() { ["ru"] = "Мальчик", ["kz"] = "Ұл бала", ["en"] = "Boy" },
            ["girl"] = new() { ["ru"] = "Девочка", ["kz"] = "Қыз бала", ["en"] = "Girl" },
            ["child_name"] = new() { ["ru"] = "Имя ребёнка", ["kz"] = "Баланың аты", ["en"] = "Child's name" },
            ["unlock_all"] = new() { ["ru"] = "Разблокировать все", ["kz"] = "Барлығын ашу", ["en"] = "Unlock all" },
            ["pages"] = new() { ["ru"] = "стр.", ["kz"] = "бет", ["en"] = "pages" },
            ["new_recording"] = new() { ["ru"] = "Новая запись", ["kz"] = "Жаңа жазба", ["en"] = "New recording" },
            ["drafts"] = new() { ["ru"] = "Черновики", ["kz"] = "Жобалар", ["en"] = "Drafts" },
            ["narrator_name"] = new() { ["ru"] = "Имя рассказчика", ["kz"] = "Әңгімешінің аты", ["en"] = "Narrator name" },
            ["start"] = new() { ["ru"] = "Начать", ["kz"] = "Бастау", ["en"] = "Start" },
            ["send"] = new() { ["ru"] = "Отправить", ["kz"] = "Жіберу", ["en"] = "Send" },
            ["done"] = new() { ["ru"] = "Готово", ["kz"] = "Дайын", ["en"] = "Done" },
            ["loading"] = new() { ["ru"] = "Загрузка...", ["kz"] = "Жүктелуде...", ["en"] = "Loading..." },
            ["back"] = new() { ["ru"] = "Назад", ["kz"] = "Артқа", ["en"] = "Back" },
            ["unlock_coming_soon"] = new() { ["ru"] = "Скоро будет доступно!", ["kz"] = "Жақында қолжетімді болады!", ["en"] = "Coming soon!" },
        };

        public static string Lang
        {
            get => PlayerPrefs.GetString("ft_lang", "ru");
            set => PlayerPrefs.SetString("ft_lang", value);
        }

        public static string Get(string key)
        {
            if (!Strings.TryGetValue(key, out var translations)) return key;
            if (translations.TryGetValue(Lang, out var text)) return text;
            if (translations.TryGetValue("ru", out var fallback)) return fallback;
            return key;
        }
    }
}
