using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MailToTelegram
{
    [Serializable]
    public class MailCrendetial
    {

        [JsonInclude] public string imap { get; set; }
        [JsonInclude] public string user;
        [JsonInclude] public string password;
    }
}
