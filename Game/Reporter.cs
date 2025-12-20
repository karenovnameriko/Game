using System;
using System.IO;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Game
{
    public static class Reporter
    {
        private static string _token;
        private static string _chatId;

        static Reporter()
        {
            LoadConfig();
        }

        private static void LoadConfig()
        {
            if (File.Exists("telegram-config.json"))
            {
                try
                {
                    string json = File.ReadAllText("telegram-config.json");
                    var config = JsonSerializer.Deserialize<Config>(json);
                    _token = config?.telegram_token;
                    _chatId = config?.telegram_chat_id;

                    if (!string.IsNullOrEmpty(_token))
                        Console.WriteLine("✅ Конфиг загружен");
                }
                catch { }
            }

            if (string.IsNullOrEmpty(_token))
                Console.WriteLine("❌ telegram-config.json не найден или неверный");
        }

        public static async Task Notify(string message)
        {
            if (string.IsNullOrEmpty(_token))
            {
                Console.WriteLine("❌ Не настроен Telegram");
                return;
            }

            await Send($"📝 {message}");
        }

        public static async Task NotifyChange(string whatChanged)
        {
            if (string.IsNullOrEmpty(_token)) return;

            await Send($"🔄 <b>ИЗМЕНЕНИЕ В КОДЕ</b>\n\n" +
                      $"🎮 <b>Ловушка</b>\n" +
                      $"📋 {whatChanged}\n" +
                      $"👤 {Environment.UserName}\n" +
                      $"🕐 {DateTime.Now:HH:mm}\n\n" +
                      $"#ловушка #изменение");
        }

        public static async Task NotifyCommit(string commitMessage)
        {
            if (string.IsNullOrEmpty(_token)) return;

            await Send($"📌 <b>НОВЫЙ КОММИТ</b>\n\n" +
                      $"💬 {commitMessage}\n" +
                      $"👤 {Environment.UserName}\n" +
                      $"🕐 {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
                      $"#коммит #ловушка");
        }

        public static async Task Test()
        {
            if (string.IsNullOrEmpty(_token))
            {
                Console.WriteLine("❌ Не настроен Telegram");
                return;
            }

            await Send($"✅ <b>ТЕСТ</b>\nТелеграм-репортер работает!\n🕐 {DateTime.Now:HH:mm}");
            Console.WriteLine("✅ Тест отправлен");
        }

        private static async Task Send(string text)
        {
            try
            {
                var client = new HttpClient();
                string url = $"https://api.telegram.org/bot{_token}/sendMessage";

                var data = new
                {
                    chat_id = _chatId,
                    text = text,
                    parse_mode = "HTML"
                };

                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                    Console.WriteLine("✅ Отправлено в Telegram");
                else
                    Console.WriteLine($"❌ Ошибка: {await response.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
            }
        }

        private class Config
        {
            public string telegram_token { get; set; }
            public string telegram_chat_id { get; set; }
        }
    }
}