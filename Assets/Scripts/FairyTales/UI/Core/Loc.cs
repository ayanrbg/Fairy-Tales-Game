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
            ["plan_monthly"] = new() { ["ru"] = "Ежемесячная подписка", ["kz"] = "Ай сайынғы жазылым", ["en"] = "Monthly plan" },
            ["plan_yearly"] = new() { ["ru"] = "Годовая подписка", ["kz"] = "Жылдық жазылым", ["en"] = "Yearly plan" },
            ["coming_soon"] = new() { ["ru"] = "Скоро!", ["kz"] = "Жақында!", ["en"] = "Coming soon!" },
            ["restore_coming_soon"] = new() { ["ru"] = "Восстановление покупок скоро!", ["kz"] = "Сатып алуды қалпына келтіру жақында!", ["en"] = "Restore purchases coming soon!" },
            ["rec_hint"] = new() { ["ru"] = "Нажмите запись и читайте вслух", ["kz"] = "Жазуды басып, дауыстап оқыңыз", ["en"] = "Tap record and read aloud" },
            ["rec_recording"] = new() { ["ru"] = "Запись...", ["kz"] = "Жазылуда...", ["en"] = "Recording..." },
            ["rec_done"] = new() { ["ru"] = "Запись завершена", ["kz"] = "Жазба аяқталды", ["en"] = "Recording complete" },
            ["rec_error"] = new() { ["ru"] = "Ошибка записи", ["kz"] = "Жазу қатесі", ["en"] = "Recording error" },
            ["rec_cloning"] = new() { ["ru"] = "Клонирование голоса...", ["kz"] = "Дауысты клондау...", ["en"] = "Cloning voice..." },
            ["rec_narrating"] = new() { ["ru"] = "Запуск озвучки...", ["kz"] = "Дыбыстау басталуда...", ["en"] = "Starting narration..." },
            ["record"] = new() { ["ru"] = "Запись", ["kz"] = "Жазу", ["en"] = "Record" },
            ["stop"] = new() { ["ru"] = "Стоп", ["kz"] = "Тоқта", ["en"] = "Stop" },
            ["rerecord"] = new() { ["ru"] = "Перезаписать", ["kz"] = "Қайта жазу", ["en"] = "Re-record" },
            ["error"] = new() { ["ru"] = "Ошибка", ["kz"] = "Қате", ["en"] = "Error" },
            ["drafts_limit"] = new() { ["ru"] = "Максимум 3 черновика", ["kz"] = "Ең көбі 3 жоба", ["en"] = "Maximum 3 drafts" },
            ["download.preparing"] = new() { ["ru"] = "Подготовка к загрузке...", ["kz"] = "Жүктеуге дайындалуда...", ["en"] = "Preparing download..." },
            ["download.loading"] = new() { ["ru"] = "Загружаем:", ["kz"] = "Жүктелуде:", ["en"] = "Downloading:" },
            ["download.narration"] = new() { ["ru"] = "Загружаем озвучку:", ["kz"] = "Дыбыстау жүктелуде:", ["en"] = "Downloading narration:" },
            ["download.done"] = new() { ["ru"] = "Загрузка завершена!", ["kz"] = "Жүктеу аяқталды!", ["en"] = "Download complete!" },
            ["per_month"] = new() { ["ru"] = "мес", ["kz"] = "ай", ["en"] = "mo" },
            ["per_year"] = new() { ["ru"] = "год", ["kz"] = "жыл", ["en"] = "yr" },
            ["start_trial"] = new() { ["ru"] = "Попробовать бесплатно", ["kz"] = "Тегін байқап көру", ["en"] = "Start free trial" },
            ["subscribe"] = new() { ["ru"] = "Подписаться", ["kz"] = "Жазылу", ["en"] = "Subscribe" },
            ["iap_not_ready"] = new() { ["ru"] = "Магазин загружается...", ["kz"] = "Дүкен жүктелуде...", ["en"] = "Store loading..." },
            ["purchase_success"] = new() { ["ru"] = "Подписка оформлена!", ["kz"] = "Жазылым рәсімделді!", ["en"] = "Subscription activated!" },
            ["restore_success"] = new() { ["ru"] = "Покупки восстановлены!", ["kz"] = "Сатып алулар қалпына келтірілді!", ["en"] = "Purchases restored!" },
            ["restore_none"] = new() { ["ru"] = "Нет покупок для восстановления", ["kz"] = "Қалпына келтіретін сатып алу жоқ", ["en"] = "No purchases to restore" },
            ["email_subject"] = new() { ["ru"] = "Обращение из приложения Сказки", ["kz"] = "Ертегілер қосымшасынан хабарлама", ["en"] = "Message from Fairy Tales app" },
            ["write_email"] = new() { ["ru"] = "Написать на почту", ["kz"] = "Хат жазу", ["en"] = "Write email" },
            ["solve_problem"] = new() { ["ru"] = "Решите пример", ["kz"] = "Есепті шешіңіз", ["en"] = "Solve the problem" },
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
