using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace MailToTelegram
{
    internal class TelegramBot
    {
        const string ApiKey = "5661142646:AAGjQphaV0hJ-cH7Qn3r0KQWzq4M9i-piEI";
        TelegramBotClient bot;

        internal TelegramBot()
        {
            bot = new TelegramBotClient(ApiKey);

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/start")
                {
                    await bot.SendTextMessageAsync(message.Chat, "Добро пожаловать на борт, добрый путник!");
                    return;
                }
                await bot.SendTextMessageAsync(message.Chat, "Привет-привет!!");
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        public void SendMessage(string message)
        {
            bot.SendTextMessageAsync(-1001749918779, message);
        }
    }
}
