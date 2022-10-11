using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MailToTelegram
{
    [Serializable]
    public class MailCredential
    {
        [JsonInclude] public string Imap { get; set; }
        [JsonInclude] public string User { get; set; }
        [JsonInclude] public string Password { get; set; }

        public MailCredential()
        {
            Imap = string.Empty;
            User = string.Empty;
            Password = string.Empty;
        }
    }
}
