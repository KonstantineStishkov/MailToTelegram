using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TelegramConstants;

namespace MailToTelegram
{
    [Serializable]
    public class Client
    {
        [JsonInclude] public long Id { get; set; }
        [JsonInclude] public string Name { get; set; }
        [JsonInclude] public List<MailCredential> Credentials { get; set; }
        public CancellationTokenSource TokenSource { get; private set; }
        public MailCredential CredentialInCurrentDialogue { get; set; } = new MailCredential();
        public BotMessage LastQuestion { get; set; }
        
        public Client(long id, string name)
        {
            Id = id;
            Name = name;
            Credentials = new List<MailCredential>();
            TokenSource = new CancellationTokenSource();
        }
    }
}
