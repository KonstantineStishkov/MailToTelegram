namespace TelegramConstants
{
    public enum RequestMessage
    {
        Start,
        AddMail,
        Imap,
        UserName,
        Password,
    }

    public enum BotMessage
    {
        Greetings,
        ImapRequest,
        UserNameRequest,
        PasswordRequest,
        MailAdded,
        MailFailedToAdd,
        NewMessage,
    }

    public enum ErrorMessage
    {
        NoMessage,
        WrongImap,
        WrongUserName,
        WrongPassword,
        UserNotFound,
    }
    public class MessageLibrary
    {
        public static Dictionary<RequestMessage, string> Requests => new Dictionary<RequestMessage, string>()
        {
            { RequestMessage.Start, "/start"},
            { RequestMessage.AddMail, "/addmail"},
            { RequestMessage.Imap, "/imap:"},
            { RequestMessage.UserName, "/userName:"},
            { RequestMessage.Password, "/password:"},
        };

        public static Dictionary<BotMessage, string> Messages => new Dictionary<BotMessage, string>()
        {
            { BotMessage.Greetings, "Приветствую, добро пожаловать"},
            { BotMessage.ImapRequest, "Введите Imap адррес"},
            { BotMessage.UserNameRequest, "Введите имя пользователя"},
            { BotMessage.PasswordRequest, "Введлите пароль"},
            { BotMessage.MailAdded, "Почта успешно добавлена"},
            { BotMessage.MailFailedToAdd, "Не удалось добавить почту. {0}"},
            { BotMessage.NewMessage, "Новое сообщение! [{0}] от:{1}"},
        };

        public static Dictionary<ErrorMessage, string> ErrorMessages => new Dictionary<ErrorMessage, string>()
        {
            { ErrorMessage.WrongImap, "Неправильный адрес Imap"},
            { ErrorMessage.WrongUserName, "Неправильное имя пользователя"},
            { ErrorMessage.WrongPassword, "Неправильный пароль"},
            { ErrorMessage.UserNotFound, "Пользователь не найден"},
        };
    }
}