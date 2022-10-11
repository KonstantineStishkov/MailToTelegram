using Limilabs.Client.IMAP;
using Limilabs.Mail;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using TelegramConstants;

namespace MailToTelegram
{
    public class MailAgent
    {

        private string _path = ConfigurationManager.AppSettings["CredentialFile"] ?? "default.json";
        private List<Client> clients = new List<Client>();

        public MailAgent()
        {
            if (!File.Exists(_path))
            {
                File.Create(_path);
            }
        }

        public bool AddClient(Action<long, string> notify, Client client)
        {
            if (clients.Contains(client)) return false;

            clients.Add(client);

            try
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                StartReceiving(notify, client);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool RemoveClient(Action<long,string> notify, Client client)
        {
            if (!clients.Contains(client)) return false;

            clients.First(c => c.Id == client.Id).TokenSource.Cancel();
            clients.Remove(client);
            return true;
        }

        public bool AddCredentials(Action<long,string> notify, Client client)
        {
            if (!clients.Contains(client)) return false;

            try
            {
                var clientInList = clients.First(c => c.Id == client.Id);
                clientInList.Credentials = client.Credentials;
                clientInList.TokenSource.Cancel();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                StartReceiving(notify, client);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task StartReceiving(Action<long,string> notify, Client client)
        {
            while (!client.TokenSource.Token.IsCancellationRequested)
            {
                foreach (MailCredential mailCrendetial in client.Credentials)
                {
                    Receive(client.Id, mailCrendetial, notify);
                }

                await Task.Delay(TimeSpan.FromSeconds(15));
            }
        }

        private void Receive(long clientId, MailCredential credential, Action<long,string> notifier)
        {
            using (Imap imap = new Imap())
            {
                imap.ConnectSSL(credential.Imap, 993);
                imap.UseBestLogin(credential.User, credential.Password);
                imap.SelectInbox();
                List<long> ids = imap.Search()
                    .Where(Expression.And
                                    (Expression.Not(Expression.Before(DateTime.Now.AddDays(-1))),
                                     Expression.HasFlag(Flag.Unseen)));

                foreach (long id in ids)
                {
                    IMail email = new MailBuilder().CreateFromEml(imap.GetMessageByUID(id));
                    string serverName = credential.Imap.Split('.')[1];
                    string messageFormat = MessageLibrary.Messages.GetValueOrDefault(BotMessage.NewMessage) ?? string.Empty;
                    notifier(clientId, string.Format(messageFormat, serverName, email.From));
                }
            }
        }
    }
}
