using System;
using Telegram.Bot;

static string Env(string name) =>
    Environment.GetEnvironmentVariable(name) ?? "—";

var bot = new TelegramBotClient(Env("TG_BOT_TOKEN"));

var message =
$"""
🚀 Изменения в проекте

Проект: {Env("GITHUB_REPOSITORY")}
Ветка: {Env("GITHUB_REF_NAME")}
Автор: {Env("GITHUB_ACTOR")}
Commit: {Env("GITHUB_SHA")[..7]}
Дата: {DateTime.Now:yyyy.MM.dd HH:mm:ss}

🔗 Репозиторий:
https://github.com/{Env("GITHUB_REPOSITORY")}
""";

await bot.SendTextMessageAsync(
    chatId: Env("TG_CHAT_ID"),
    text: message
);
