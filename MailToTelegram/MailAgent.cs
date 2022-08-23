using Limilabs.Client.IMAP;
using Limilabs.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailToTelegram
{
    internal class MailAgent
    {
        List<MailCrendetial> credentials;

        public MailAgent(List<MailCrendetial> credentials)
        {
            this.credentials = credentials;
        }

        public async Task StartReceiving(Action<string> notifier)
        {
            while (true)
            {
                foreach (MailCrendetial mailCrendetial in credentials)
                    StartReceiving(mailCrendetial, notifier);

                await Task.Delay(TimeSpan.FromSeconds(15));
            }
        }

        public void StartReceiving(MailCrendetial credential, Action<string> notifier)
        {
            using (Imap imap = new Imap())
            {
                imap.ConnectSSL(credential.imap, 993);
                imap.UseBestLogin(credential.user, credential.password);
                imap.SelectInbox();
                List<long> ids = imap.Search()
                    .Where(Expression.And
                                    (Expression.Not(Expression.Before(DateTime.Now.AddDays(-1))), 
                                     Expression.HasFlag(Flag.Unseen)));

                foreach (long id in ids)
                {
                    IMail email = new MailBuilder().CreateFromEml(imap.GetMessageByUID(id));
                    notifier($"New mail in {credential.imap.Split('.')[1]} \n {email.From}");
                }
            }
        }
    }
}
