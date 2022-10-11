// See https://aka.ms/new-console-template for more information
using MailToTelegram;
using Newtonsoft.Json;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {
        new TelegramBot(new MailAgent()).StartBot();
    }
}