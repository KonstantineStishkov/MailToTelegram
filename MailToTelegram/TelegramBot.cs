using System.Configuration;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TelegramConstants;

namespace MailToTelegram
{
    internal class TelegramBot
    {
        const string defaultName = "John Doe";

        private readonly string? ApiKey = ConfigurationManager.AppSettings["ApiKey"];
        private string _path = ConfigurationManager.AppSettings["CredentialFile"] ?? "default.json";

        private readonly Dictionary<BotMessage, string> Messages = MessageLibrary.Messages;
        private readonly Dictionary<ErrorMessage, string> ErrorMessages = MessageLibrary.ErrorMessages;
        private List<Client> Clients { get; set; }
        private Dictionary<RequestMessage, Action<long, string?>> requestActions => new Dictionary<RequestMessage, Action<long, string?>>()
        {
            {RequestMessage.Start, StartHandle },
            {RequestMessage.AddMail, AddMailHandle },
            {RequestMessage.Imap, AddImapAddressHandle },
            {RequestMessage.UserName, AddUserNameHandle },
            {RequestMessage.Password, AddPasswordHandle },
        };

        private TelegramBotClient? bot;
        private MailAgent mailAgent;

        public TelegramBot(MailAgent agent)
        {
            Clients = GetClients();
            mailAgent = agent;
            StartBot();
        }

        private List<Client> GetClients()
        {
            try
            {
                FileStream fs = new FileStream(_path, FileMode.Open, FileAccess.Read);
                return JsonSerializer.Deserialize<List<Client>>(fs) ?? new List<Client>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Client>();
            }
        }

        public void StartBot()
        {
            if (ApiKey == null)
            {
                Console.WriteLine("Не найден ключ бота!");
                return;
            }

            bot = new TelegramBotClient(ApiKey);

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
        }

        public void AddClient(Client client, Action<string> notify)
        {
            if (Clients.Any(c => c.Id == client.Id))
            {
                var old_client = Clients.First(c => c.Id == client.Id);
                Clients.Remove(old_client);
            }

            Clients.Add(client);

            try
            {
                var fs = new FileStream(_path, FileMode.Open, FileAccess.Write);
                JsonSerializer.Serialize(fs, Clients);
                notify(Messages[BotMessage.MailAdded]);
            }
            catch (Exception ex)
            {
                string message = string.Format(Messages[BotMessage.MailFailedToAdd], ex.Message);
                notify(message);
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            Message? message = update.Message;
            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message
                || message == null
                || message.Text == null
                || !MessageLibrary.Requests.Any(r => message.Text.StartsWith(r.Value, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            KeyValuePair<RequestMessage, string> request = MessageLibrary.Requests.FirstOrDefault(r => message.Text.StartsWith(r.Value, StringComparison.OrdinalIgnoreCase));
            Action<long, string?> action = requestActions[request.Key];

            string? parameter;

            if (message.Text.StartsWith(MessageLibrary.Requests[RequestMessage.Start]))
            {
                parameter = message.Chat.Username;
            }
            else
            {
                parameter = message.Text.Substring(request.Value.Length).Trim();
            }

            action(message.Chat.Id, parameter);
            await Task.Delay(1);
        }

        private void StartHandle(long clientChatId, string? name)
        {
            const string defaultName = "John Doe";
            Client client;
            if (Clients.Any(c => c.Id == clientChatId))
            {
                client = Clients.First(c => c.Id == clientChatId);
            }
            else
            {
                client = new Client(clientChatId, name ?? defaultName);
            }

            mailAgent.AddClient(SendMessage, client);
        }

        /// <summary>
        /// Starting dialogue that leads to adding credentials. First question
        /// </summary>
        /// <param name="clientChatId"></param>
        /// <param name="name"></param>
        private void AddMailHandle(long clientChatId, string? name)
        {
            Client? client = Response(clientChatId, BotMessage.ImapRequest, ErrorMessage.UserNotFound);

            if (client != null)
            {
                client.CredentialInCurrentDialogue = new MailCredential();
            }
        }

        private void AddImapAddressHandle(long clientChatId, string? imap)
        {
            Client? client = Response(clientChatId, BotMessage.UserNameRequest, ErrorMessage.WrongImap, imap);

            if (client != null && imap != null)
            {
                client.CredentialInCurrentDialogue.Imap = imap;
            }
        }

        private void AddUserNameHandle(long clientChatId, string? userName)
        {
            Client? client = Response(clientChatId, BotMessage.PasswordRequest, ErrorMessage.WrongUserName, userName);

            if (client != null && userName != null)
            {
                client.CredentialInCurrentDialogue.User = userName;
            }
        }

        private void AddPasswordHandle(long clientChatId, string? password)
        {
            Client? client = Response(clientChatId, BotMessage.PasswordRequest, ErrorMessage.WrongImap, password);

            if (client != null && password != null)
            {
                client.CredentialInCurrentDialogue.User = password;
                mailAgent.AddCredentials(SendMessage, client);
            }
        }

        private Client? Response(long clientChatId, BotMessage botMessage, ErrorMessage errorMessage, string? parameter = "")
        {
            if (!Clients.Any(c => c.Id == clientChatId))
            {
                SendMessage(clientChatId, ErrorMessages[ErrorMessage.UserNotFound]);
                return null;
            }

            if (errorMessage != ErrorMessage.NoMessage
                && string.IsNullOrWhiteSpace(parameter))
            {
                SendMessage(clientChatId, ErrorMessages[errorMessage]);
                return null;
            }

            Client client = Clients.First(c => c.Id == clientChatId);
            string message = Messages[botMessage];
            SendMessage(clientChatId, message);
            client.LastQuestion = botMessage;

            return client;
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            await Task.Delay(0);
        }

        public void SendMessage(long clientChatId, string message)
        {
            if (bot == null) return;

            bot.SendTextMessageAsync(clientChatId, message);
        }
    }
}
