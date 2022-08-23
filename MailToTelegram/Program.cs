// See https://aka.ms/new-console-template for more information
using MailToTelegram;
using Newtonsoft.Json;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {
        string path = Path.GetFullPath("mailOptions.json");
        if (!File.Exists(path))
        {
            Console.WriteLine("No options file!");
            return;
        }
        TelegramBot bot = new TelegramBot();
        var mailCredentials = GetMailCrendetials(path);
        new MailAgent(mailCredentials).StartReceiving(bot.SendMessage);

        Console.ReadLine();
    }

    private static List<MailCrendetial> GetMailCrendetials(string path)
    {
        List<MailCrendetial> mailCrendetials = new List<MailCrendetial>();

        var serializer = new JsonSerializer();

        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            mailCrendetials = serializer.Deserialize<List<MailCrendetial>>(new JsonTextReader(new StreamReader(fs)));
        }

        return mailCrendetials;
    }
}