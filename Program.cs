using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using System.Net.Http;
using System.Net;
using Telegram.Bot.Types;
using QiwiShop;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using WeatherApiClass;
using File = System.IO.File;
using System.Data.OleDb;

namespace Laura_Bot_Chat_Manager
{

    class Program
    {
        private static string token { get; set; } = "5565507778:AAGDcO0ytb3RlnyWLdUKIUZOklyPl-s_Wik";
        private static TelegramBotClient client;
        public static string Username { get; set; }
        public long Id { get; set; }
        private static User user;
        static SqlConnection sql = new SqlConnection(connectionString: "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Ivanin Kirill\\Documents\\LauraBotRes\\Laura Bot Chat Manager\\DataBase_moder.mdf;Integrated Security=True");
        static BotCommand command;
        private static ChatPermissions ChatPermissions;

        static void Main(string[] args)
        {
            var proxy = new WebProxy("socks5://127.0.0.1:9050");

            var httpClient = new HttpClient(
                new SocketsHttpHandler { Proxy = proxy, UseProxy = true }
            );

            client = new TelegramBotClient(token);
            client.StartReceiving(new UpdateType[] { UpdateType.Message });
            client.OnMessage += OnMessageHandler;
            Console.Title = "LauraBotLogConsole";
            Console.WriteLine("Бот Лаура запущен!");
            Console.WriteLine("===============================");
            Console.WriteLine("Bot Developed on C#\nDev: @LanceMinecarft,\n@TheShadow_hk (Telegram)");
            Console.WriteLine("Version: 0.6.9 closed alpha");
            Console.WriteLine("===============================");
            Console.WriteLine($"Time of start: {DateTime.Now}");
            Console.WriteLine("Begin of console log:");
            try
            {
                Thread.Sleep(Timeout.Infinite);
            }
            catch (ArgumentOutOfRangeException exc)
            {
                Console.WriteLine(exc.Message + "\nBot working is long!");
                return;
            }
            client.StopReceiving();
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {

            var msg = e.Message;
            var Admin = ChatMemberStatus.Administrator;
            var Member = ChatMemberStatus.Member;
            var Owner = ChatMemberStatus.Creator;
            var Banned = ChatMemberStatus.Kicked;
            var Muted = ChatMemberStatus.Restricted;




            if (msg == null)
            {
                return;
            }

            if (msg.NewChatMembers != null)
            {
                await client.SendStickerAsync(chatId: msg.Chat.Id, sticker: "https://t.me/ScladOfRes/57", replyToMessageId: msg.MessageId);
                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Добро пожаловать в чат, <a href = \"tg://openmessage?user_id={msg.NewChatMembers[0].Id}\">{msg.NewChatMembers[0].FirstName} {msg.NewChatMembers[0].LastName}</a>!\nВы попали в «{msg.Chat.Title}»!", parseMode: ParseMode.Html, disableWebPagePreview: true);
                Console.WriteLine($"{DateTime.Now} New member in chat: {msg.Chat.Id}");
            }

            if (msg.LeftChatMember != null)
            {
                await client.SendStickerAsync(chatId: msg.Chat.Id, sticker: "https://t.me/ScladOfRes/59");
                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.LeftChatMember.Id}\">{msg.LeftChatMember.FirstName} {msg.LeftChatMember.LastName}</a> покинул чат!", parseMode: ParseMode.Html, disableWebPagePreview: true);

                Console.WriteLine($"{DateTime.Now} Member ID{msg.LeftChatMember.Id} left from chat: {msg.Chat.Id}");
            }


            if (msg.Text != null)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now} New Message from ID{msg.Chat.Id}, it's message: {msg.Text}");

                    var LogChat = -763013536;
                    if (msg.From.Username == null)
                    {
                        if (msg.Chat.Id == msg.From.Id)
                        {
                            await client.SendTextMessageAsync(chatId: LogChat, $"ФИ пользователя: <b>{msg.From.FirstName} {msg.From.LastName}</b>\nID пользователя: <code>{msg.From.Id}</code>\n\nТекст пользователя:\n<i>{msg.Text}</i>", parseMode: ParseMode.Html);
                        }
                        else
                        {
                            await client.SendTextMessageAsync(chatId: LogChat, $"ФИ пользователя: <b>{msg.From.FirstName} {msg.From.LastName}</b>\nID пользователя: <code>{msg.From.Id}</code>\n\nНазвание беседы: <b>{msg.Chat.Title}</b>\nID данной беседы: <code>{msg.Chat.Id}</code>\n\nТекст пользователя:\n<i>{msg.Text}</i>", parseMode: ParseMode.Html);
                        }
                    }
                    else
                    {
                        if (msg.Chat.Id != msg.From.Id)
                        {
                            await client.SendTextMessageAsync(chatId: LogChat, $"ФИ пользователя: <b>{msg.From.FirstName} {msg.From.LastName}</b>\nЮзернейм пользователя: @{msg.From.Username}\nID пользователя: <code>{msg.From.Id}</code>\n\nНазвание беседы: <b>{msg.Chat.Title}</b>\nID данной беседы: <code>{msg.Chat.Id}</code>\n\nТекст пользователя:\n<i>{msg.Text}</i>", parseMode: ParseMode.Html);
                        }
                        else
                        {
                            await client.SendTextMessageAsync(chatId: LogChat, $"ФИ пользователя: <b>{msg.From.FirstName} {msg.From.LastName}</b>\nЮзернейм пользователя: @{msg.From.Username}\nID пользователя: <code>{msg.From.Id}</code>\n\nТекст пользователя:\n<i>{msg.Text}</i>", parseMode: ParseMode.Html);
                        }
                    }

                    switch (msg.Text)
                    {
                        case "+":
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    break;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    break;
                                }
                                else if (msg.ReplyToMessage != null)
                                {
                                    if (msg.ReplyToMessage.From.Id == msg.From.Id)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, а у вас, как я вижу, высокая самооценка😏", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        break;
                                    }
                                }
                                break;
                            }

                        case "-":
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    break;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    break;
                                }
                                else if (msg.ReplyToMessage != null)
                                {
                                    if (msg.ReplyToMessage.From.Id == msg.From.Id)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, зачем себя так унижать😕", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        break;
                                    }
                                }
                                break;
                            }

                        case "Назад🔃":
                            {
                                if (msg.Chat.Id != msg.From.Id)
                                {
                                    break;
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Вы вернулись в меню", replyMarkup: GetButtons());
                                    break;
                                }

                            }
                        case "Бан":
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    break;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, "⛔Вы не указали пользователя!");
                                    break;
                                }
                                else if (msg.ReplyToMessage.From.IsBot)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} является ботом🤖!");
                                    break;
                                }
                                else if (msg.ReplyToMessage != null)
                                {


                                    Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                    var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                    var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;

                                    }
                                    else
                                    {
                                        await client.KickChatMemberAsync(userId: msg.ReplyToMessage.From.Id, chatId: msg.Chat.Id, cancellationToken: default);
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❌Участник <a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был забанен модератором:<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                break;
                            }

                        case "бан":
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    break;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, "⛔Вы не указали пользователя!");
                                    break;
                                }
                                else if (msg.ReplyToMessage.From.IsBot)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} является ботом🤖!");
                                    break;
                                }
                                else if (msg.ReplyToMessage != null)
                                {


                                    Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                    var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                    var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;

                                    }
                                    else
                                    {
                                        await client.KickChatMemberAsync(userId: msg.ReplyToMessage.From.Id, chatId: msg.Chat.Id, cancellationToken: default);
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❌Участник <a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был забанен модератором:<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                break;
                            }


                        case "Разбан":
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    break;
                                }

                                else if (msg.ReplyToMessage.From.IsBot)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} является ботом🤖!");
                                    break;
                                }

                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, "⛔Вы не указали пользователя!");
                                    break;
                                }
                                else if (msg.ReplyToMessage != null)
                                {
                                    Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                    var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                    var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else if (MemberTarget.Status != Banned)
                                    {
                                        if (msg.ReplyToMessage.From.Username == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> не находиться в чёрном списке😕!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        await client.UnbanChatMemberAsync(userId: msg.ReplyToMessage.From.Id, chatId: msg.Chat.Id, cancellationToken: default);
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был разбанен модератором <a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>»!\nТеперь его можно вернуть в чат🤗.", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                break;
                            }


                        case "разбан":
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    break;
                                }

                                else if (msg.ReplyToMessage.From.IsBot)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} является ботом🤖!");
                                    break;
                                }

                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, "⛔Вы не указали пользователя!");
                                    break;
                                }
                                else if (msg.ReplyToMessage != null)
                                {
                                    Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                    var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                    var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else if (MemberTarget.Status != Banned)
                                    {
                                        if (msg.ReplyToMessage.From.Username == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> не находиться в чёрном списке😕!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        await client.UnbanChatMemberAsync(userId: msg.ReplyToMessage.From.Id, chatId: msg.Chat.Id, cancellationToken: default);
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был разбанен модератором <a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>»!\nТеперь его можно вернуть в чат🤗.", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                break;
                            }

                        case "Размут":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }

                            else if (msg.ReplyToMessage.From.IsBot)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} является ботом🤖!");
                                break;
                            }

                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, "⛔Вы не указали пользователя!");
                                break;
                            }
                            else if (msg.ReplyToMessage != null)
                            {
                                Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                {
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                {
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else if (MemberTarget.Status != ChatMemberStatus.Restricted)
                                {
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> не был заглушен😐!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> не был заглушен😐!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else
                                {
                                    await client.PromoteChatMemberAsync(userId: msg.ReplyToMessage.From.Id, chatId: msg.Chat.Id, cancellationToken: default);
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {

                                        if (msg.From.Username == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> теперь может общаться!\n\nТолько лучше следить за своим языком и не разжигать ссоры😊.\nМодератор: <a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> теперь может общаться!\n\nТолько лучше следить за своим языком и не разжигать ссоры😊.\nМодератор: <a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                    }
                                    else
                                    {
                                        if (msg.From.Username == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> теперь может общаться!\n\nТолько лучше следить за своим языком и не разжигать ссоры😊.\nМодератор: <a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> теперь может общаться!\n\nТолько лучше следить за своим языком и не разжигать ссоры😊.\nМодератор: <a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case "размут":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }

                            else if (msg.ReplyToMessage.From.IsBot)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} является ботом🤖!");
                                break;
                            }

                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, "⛔Вы не указали пользователя!");
                                break;
                            }
                            else if (msg.ReplyToMessage != null)
                            {
                                Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                {
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                {
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else if (MemberTarget.Status != ChatMemberStatus.Restricted)
                                {
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> не был заглушен😐!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> не был заглушен😐!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else
                                {
                                    await client.PromoteChatMemberAsync(userId: msg.ReplyToMessage.From.Id, chatId: msg.Chat.Id, cancellationToken: default);
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {

                                        if (msg.From.Username == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> теперь может общаться!\n\nТолько лучше следить за своим языком и не разжигать ссоры😊.\nМодератор: <a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> теперь может общаться!\n\nТолько лучше следить за своим языком и не разжигать ссоры😊.\nМодератор: <a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                    }
                                    else
                                    {
                                        if (msg.From.Username == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> теперь может общаться!\n\nТолько лучше следить за своим языком и не разжигать ссоры😊.\nМодератор: <a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> теперь может общаться!\n\nТолько лучше следить за своим языком и не разжигать ссоры😊.\nМодератор: <a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case "Мут":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, "⛔Вы не указали пользователя!");
                                break;
                            }
                            else if (msg.ReplyToMessage.From.IsBot)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} является ботом🤖!");
                                break;
                            }

                            else if (msg.ReplyToMessage != null)
                            {
                                Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                bool isAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                {
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                {
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else if (MemberTarget.Status == ChatMemberStatus.Restricted)
                                {
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> уже был заглушен📛!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> уже был заглушен📛!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else
                                {
                                    await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(15), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {

                                        if (msg.From.Username == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был заглушен на 15 минут!\nМодератор: <a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был заглушен на 15 минут!\nМодератор: <a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                    }
                                    else
                                    {
                                        if (msg.From.Username == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был заглушен на 15 минут!\nМодератор: <a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был заглушен на 15 минут!\nМодератор: <a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case "мут":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, "⛔Вы не указали пользователя!");
                                break;
                            }
                            else if (msg.ReplyToMessage.From.IsBot)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} является ботом🤖!");
                                break;
                            }

                            else if (msg.ReplyToMessage != null)
                            {
                                Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                bool isAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                {
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                {
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else if (MemberTarget.Status == ChatMemberStatus.Restricted)
                                {
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> уже был заглушен📛!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> уже был заглушен📛!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                else
                                {
                                    await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(15), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                    if (msg.ReplyToMessage.From.Username == null)
                                    {

                                        if (msg.From.Username == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был заглушен на 15 минут!\nМодератор: <a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был заглушен на 15 минут!\nМодератор: <a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                    }
                                    else
                                    {
                                        if (msg.From.Username == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был заглушен на 15 минут!\nМодератор: <a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }

                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был заглушен на 15 минут!\nМодератор: <a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case "/nightmode":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }

                            else if (msg.Chat.Id != msg.From.Id)
                            {
                                Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                {
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                //else if (ChatPermissions == new ChatPermissions{ CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false })
                                //{
                                //    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.From.Username}, уже другой модератор объявил ночной режим🔇!");
                                //    break;
                                //}
                                else
                                {
                                    await client.SetChatPermissionsAsync(chatId: msg.Chat.Id, permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> объявляет ночной режим🤫!", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a> объявляет ночной режим🤫!", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    }
                                    break;
                                }
                            }
                            break;

                        case "/nightmode@Laura_cm_bot":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }

                            else if (msg.Chat.Id != msg.From.Id)
                            {
                                Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                {
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                //else if (ChatPermissions == new ChatPermissions{ CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false })
                                //{
                                //    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.From.Username}, уже другой модератор объявил ночной режим🔇!");
                                //    break;
                                //}
                                else
                                {
                                    await client.SetChatPermissionsAsync(chatId: msg.Chat.Id, permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> объявляет ночной режим🤫!", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a> объявляет ночной режим🤫!", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    }
                                    break;
                                }
                            }
                            break;

                        case "/statemode":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }

                            else if (msg.Chat.Id != msg.From.Id)
                            {
                                Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                {
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                //else if (ChatPermissions == new ChatPermissions{ CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false })
                                //{
                                //    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.From.Username}, уже другой модератор объявил ночной режим🔇!");
                                //    break;
                                //}
                                else
                                {
                                    await client.SetChatPermissionsAsync(chatId: msg.Chat.Id, permissions: new ChatPermissions { CanSendMessages = true, CanSendMediaMessages = true, CanSendOtherMessages = true, CanAddWebPagePreviews = true });
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> возвращает чат в штатный режим✅", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a> возвращает чат в штатный режим✅", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    }
                                }
                            }
                            break;

                        case "/statemode@Laura_cm_bot":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }

                            else if (msg.Chat.Id != msg.From.Id)
                            {
                                Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                {
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        break;
                                    }
                                }
                                //else if (ChatPermissions == new ChatPermissions{ CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false })
                                //{
                                //    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.From.Username}, уже другой модератор объявил ночной режим🔇!");
                                //    break;
                                //}
                                else
                                {
                                    await client.SetChatPermissionsAsync(chatId: msg.Chat.Id, permissions: new ChatPermissions { CanSendMessages = true, CanSendMediaMessages = true, CanSendOtherMessages = true, CanAddWebPagePreviews = true });
                                    if (msg.From.Username == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> возвращает чат в штатный режим✅", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a> возвращает чат в штатный режим✅", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    }
                                }
                            }
                            break;

                        case "/help@Laura_cm_bot":
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "<b>Внимание!\nВ инструкции пока что лежат не все команды, так как бот ещё в разработке!\n</b><a href = \"https://telegra.ph/Polnyj-spisok-komand-bota-Laura-06-21\">Инструкция</a>", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;

                        case "/help":
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "<b>Внимание!\nВ инструкции пока что лежат не все команды, так как бот ещё в разработке!\n</b><a href = \"https://telegra.ph/Polnyj-spisok-komand-bota-Laura-06-21\">Инструкция</a>", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;

                        case "/start@Laura_cm_bot":
                            if (msg.Chat.Id != msg.From.Id)
                            {
                                
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Здравствуйте, я чат-менеджер бот Лаура!\nНапишите 👉/help, чтобы получить список команд.");
                                BotCommand[] botCommands = { new() { Command = "start", Description = "Обновить список команд" }, new() { Command = "help", Description = "Получить справочник по использованию бота" }, new() { Command = "getchatid", Description = "Получить ID данного чата" }, new() { Command = "nightmode", Description = "Включить ночной режим в чате" }, new() { Command = "statemode", Description = "Вернуть чат в штатный режим" } };
                                await client.SetMyCommandsAsync(botCommands);
                                await client.GetMyCommandsAsync();
                                await client.SetMyCommandsAsync(botCommands);
                                Console.WriteLine($"Bot was started in chat: ID{msg.Chat.Id}");
                                break;
                            }
                            else
                            {
                                BotCommand[] botCommands = { new() { Command = "start", Description = "Обновить список команд" }, new() { Command = "getmyid", Description = "Получить свой личный ID" } };
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Здравствуйте, я чат-менеджер бот Лаура!\nНажмите на кнопку👉«Инструкция📚», чтобы получить список команд.");
                                await client.SetMyCommandsAsync(botCommands);
                                await client.GetMyCommandsAsync();
                                await client.SetMyCommandsAsync(botCommands);
                                Console.WriteLine($"Bot was started in chat: ID{msg.Chat.Id}");
                                break;
                            }

                        case "/start":
                            if (msg.Chat.Id != msg.From.Id)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Здравствуйте, я чат-менеджер бот Лаура!\nНапишите 👉/help, чтобы получить список команд.");
                                BotCommand[] botCommands = { new() { Command = "start", Description = "Обновить список команд" }, new() { Command = "help", Description = "Получить справочник по использованию бота" }, new() { Command = "getchatid", Description = "Получить ID данного чата" }, new() { Command = "nightmode", Description = "Включить ночной режим в чате" }, new() { Command = "statemode", Description = "Вернуть чат в штатный режим" } };
                                await client.SetMyCommandsAsync(botCommands);
                                await client.GetMyCommandsAsync();
                                await client.SetMyCommandsAsync(botCommands);
                                Console.WriteLine($"Bot was started in chat: ID{msg.Chat.Id}");
                                break;
                            }
                            else
                            {
                                BotCommand[] botCommands = { new() { Command = "start", Description = "Обновить список команд" }, new() { Command = "getmyid", Description = "Получить свой личный ID" } };
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Здравствуйте, я чат-менеджер бот Лаура!\nНажмите на кнопку👉«Инструкция📚», чтобы получить список команд.");
                                await client.SetMyCommandsAsync(botCommands);
                                await client.GetMyCommandsAsync();
                                await client.SetMyCommandsAsync(botCommands);
                                Console.WriteLine($"Bot was started in chat: ID{msg.Chat.Id}");
                                break;
                            }

                        //Кнопки класса "GetButtons"
                        case "Инструкция📚":
                            if (msg.Chat.Id != msg.From.Id)
                            {
                                break;
                            }
                            else
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Бот в разработке, поэтому инструкция является не полной!\n<a href = \"https://telegra.ph/Polnyj-spisok-komand-bota-Laura-06-21\">Инструкция</a>", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }

                        case "О нас📒":
                            if (msg.Chat.Id != msg.From.Id)
                            {
                                break;
                            }
                            else
                            {
                                InlineKeyboardButton ProjectNews = new InlineKeyboardButton();
                                InlineKeyboardButton Dev1 = new InlineKeyboardButton();
                                InlineKeyboardButton Dev2 = new InlineKeyboardButton();
                                InlineKeyboardButton ScladOfRes = new InlineKeyboardButton();

                                ProjectNews.Text = "Канал по новостям проектов📡";
                                ProjectNews.Url = "https://t.me/FBA_Studio";

                                Dev1.Text = "Основатель🤴";
                                Dev1.Url = "https://t.me/LanceMinecraft";

                                Dev2.Text = "Разработчик 2👨‍💻";
                                Dev2.Url = "https://t.me/TheShadow_hk";

                                ScladOfRes.Text = "Канал с ресурсами для бота📚";
                                ScladOfRes.Url = "https://t.me/ScladOfRes";

                                InlineKeyboardButton[] Row1 = new InlineKeyboardButton[] { ProjectNews };
                                InlineKeyboardButton[] Row2 = new InlineKeyboardButton[] { Dev1, Dev2 };
                                InlineKeyboardButton[] Row3 = new InlineKeyboardButton[] { ScladOfRes };

                                InlineKeyboardButton[][] InfoButtons = new InlineKeyboardButton[][] { Row1, Row2, Row3 };
                                InlineKeyboardMarkup InfoKeyboard = new InlineKeyboardMarkup(InfoButtons);

                                await client.SendTextMessageAsync(
                                    chatId: msg.Chat.Id,
                                    "Пока ещё нечего сказать, но можешь подписаться на тг канал, где будут выкладываться новости по всем проектам нашим)",
                                    replyMarkup: InfoKeyboard);
                                break;
                            }


                        case "Добавить бота в чат➕":
                            if (msg.Chat.Id != msg.From.Id)
                            {
                                break;
                            }
                            else
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, text: $"<b>❗Права, необходимые для модерировани бота:</b>\n\n<i>-измененния прав участников чата</i>\n<i>-удаление участников из чата</i>\n<i>-удаление чужих сообщений</i>\n<i>Изменения разрешений чата</i>", parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Добавить бота в группу", "http://t.me/Laura_cm_bot?startgroup=start")));
                                break;
                            }


                        case "Игровые режимы🎮":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Игровые режимы в разработке🛠!", replyMarkup: GameModes());
                                break;
                            }
                            else
                                break;

                        //Развлекательные рп команды
                        case "Обнять":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                if (msg.From.Username != null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> обнял себя🤗", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    break;
                                }

                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName}</a> обнял себя🤗", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    break;
                                }
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> обнял <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🤗", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;

                        case "обнять":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> обнял себя🤗", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> обнял <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🤗", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;



                        case "Ударить":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> ударил со всей силой в пустоту😶", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> ударил со всей силой в <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🤕", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;

                        case "ударить":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> ударил со всей силой в пустоту😶", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> ударил со всей силой в <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🤕", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;



                        case "Убить":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> покончил свою жизнь самоубийством🤡🔪", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> убивает <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🔪😢", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;

                        case "убить":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> покончил свою жизнь самоубийством🤡🔪", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> убивает <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🔪😢", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;



                        case "Укусить":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> укусил себя🤡", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> делает укус <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🐺", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;

                        case "укусить":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> укусил себя🤡", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> делает укус <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🐺", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;



                        case "Показать язык":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> просто показал язык👅", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> показал язык <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>😜", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;

                        case "показать язык":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> просто показал язык👅", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> показал язык <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>😜", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;



                        case "Накормить":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> вкусно покормил себя😋", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> вкусно покормил участника <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🍔🍟🌭", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;

                        case "накормить":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else if (msg.ReplyToMessage == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> вкусно покормил себя😋", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                break;
                            }
                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName}</a> вкусно покормил участника <a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🍔🍟🌭", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            break;
                        //Другие развлекательные команды
                        case "Обосрался":
                            await client.SendVideoNoteAsync(chatId: msg.Chat.Id, videoNote: "https://telesco.pe/ScladOfRes/63");
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Так вот кто дверь испачкал😏", replyToMessageId: msg.MessageId);
                            break;

                        case "обосрался":
                            await client.SendVideoNoteAsync(chatId: msg.Chat.Id, videoNote: "https://telesco.pe/ScladOfRes/63");
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Так вот кто дверь испачкал😏", replyToMessageId: msg.MessageId);
                            break;

                        case "Обосралась":
                            await client.SendVideoNoteAsync(chatId: msg.Chat.Id, videoNote: "https://telesco.pe/ScladOfRes/63");
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Так вот кто дверь испачкал😏", replyToMessageId: msg.MessageId);
                            break;

                        case "обосралась":
                            await client.SendVideoNoteAsync(chatId: msg.Chat.Id, videoNote: "https://telesco.pe/ScladOfRes/63");
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Так вот кто дверь испачкал😏", replyToMessageId: msg.MessageId);
                            break;


                        case "Иди нахуй шлюха":
                            await client.SendVideoAsync(chatId: msg.Chat.Id, video: "https://t.me/ScladOfRes/61", replyToMessageId: msg.MessageId);
                            break;

                        case "Кто такой Кинаут":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else
                                await client.SendVideoAsync(chatId: msg.Chat.Id, video: "https://t.me/ScladOfRes/2");
                            Console.WriteLine(" ");
                            Console.WriteLine($"{msg.From.FirstName} {msg.From.LastName} ask question who this Kinaut!");
                            Console.WriteLine(" ");
                            await client.SendTextMessageAsync(chatId: msg.From.Id, "Пояснительная бригада:\n Кинаут - враг создателя бота в том плане, что он шантажом с него вытряхивал деньги. Кинаут также рейдил его тг канал и чат 8-9 раз @RiceTeamStudio, вплоть до тг акка(первый раз удалил тг акк, второй раз - довёл до вечного спам бана).\nТяжёлые у него были времена😕.");
                            break;

                        case "Кидаем плотную зигу":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else
                                await client.SendVideoAsync(chatId: msg.Chat.Id, video: "https://t.me/LanceYouTube/13");
                            Console.WriteLine(" ");
                            Console.WriteLine($"Warning! User @{msg.From.Username} in chat ID{msg.Chat.Id} drop the Ziga!");
                            Console.WriteLine($"Sending warning to @{msg.From.Username}...");
                            Console.WriteLine(" ");
                            await client.SendTextMessageAsync(chatId: msg.From.Id, $"@{msg.From.Username}, ваша зига спалена разработчику бота! ");
                            break;

                        case "Этот бот будет взломан":
                            await client.SendVideoAsync(chatId: msg.Chat.Id, video: "https://t.me/RiceTeamChat/2354", replyToMessageId: msg.MessageId);
                            break;

                        case "Я гуль":
                            if (msg.Chat.Id == msg.From.Id)
                            {
                                break;
                            }
                            else
                                await client.SendVideoAsync(chatId: msg.Chat.Id, video: "https://t.me/DichBlogOfLance/723", caption: "1000-7, прости, ты умер");
                            Console.WriteLine(" ");
                            Console.WriteLine($"@{msg.From.Username} - is dead inside!");
                            Console.WriteLine(" ");
                            await client.SendTextMessageAsync(chatId: msg.From.Id, "Попался, дед инсайдик!");
                            await client.SendAudioAsync(chatId: msg.Chat.Id, audio: "https://t.me/DichBlogOfLance/731");
                            break;

                        //Другое
                        case "Партнёры бота":
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, text: "<b>Наши партнёры бота🤝:</b>\n<i>-@FlushaStudio</i>\n<i>-@RiceTeamStudio</i>\n<i>-@banan4ikmoder</i>\n<i>@TheShadow_hk(Dev)</i>\n<i>Список обновляется</i>", parseMode: ParseMode.Html);
                            break;
                    }

                    if (msg.Text == "Логин" | msg.Text == "логин")
                    {
                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Введите пароль:", replyMarkup: new ForceReplyMarkup { Selective = true });
                        if (msg.Text == "123")
                        {
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Пароль верен!");
                        }
                        else if (msg.ReplyToMessage != null & msg.Text != "123")
                        {
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Пароль неверен!");
                        }
                        else
                        {
                            return;
                        }
                    }
                    if (msg.Text.StartsWith("Мут ") | msg.Text.StartsWith("мут "))
                    {
                        if (msg.Chat.Id == msg.From.Id)
                        {
                            return;
                        }
                        else if (msg.ReplyToMessage == null)
                        {
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "⛔Вы не указали пользователя!");
                            return;
                        }
                        else if (msg.ReplyToMessage.From.IsBot)
                        {
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} является ботом🤖!");
                            return;
                        }

                        else if (msg.ReplyToMessage != null)
                        {
                            var MuteTimeout = msg.Text;
                            String[] SplitSymbol = MuteTimeout.Split(' ');
                            var TimeOut = Convert.ToDouble(SplitSymbol[1]);

                            Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                            bool isAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                            var MemberOutput = client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                            var MemberTarget = client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);

                            if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                            {
                                if (msg.From.Username == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://openmessage?user_id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!");
                                    return;
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"https://t.me/{msg.From.Username}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!");
                                    return;
                                }
                            }
                            else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                            {
                                if (msg.ReplyToMessage.From.Username == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"https://t.me/{msg.ReplyToMessage.From.Username}\"></a> является администратором чата «{msg.Chat.Title}»");
                                    return;
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗@{msg.ReplyToMessage.From.Username} является администратором чата «{msg.Chat.Title}»");
                                    return;
                                }
                            }
                            else
                                switch (SplitSymbol[2].Trim())
                                {
                                    //Часовой тип
                                    case "Час":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddHours(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} час🔇!");
                                        break;

                                    case "час":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddHours(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} час🔇!");
                                        break;

                                    case "Часа":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddHours(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} часа🔇!");
                                        break;

                                    case "часа":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddHours(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} часа🔇!");
                                        break;

                                    case "Часов":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddHours(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} часов🔇!");
                                        break;

                                    case "часов":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddHours(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} часов🔇!");
                                        break;


                                    //Минутный тип
                                    case "Минут":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} минут🔇!");
                                        break;

                                    case "минут":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} минут🔇!");
                                        break;

                                    case "Минуту":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} минуту🔇!");
                                        break;

                                    case "минуту":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} минуту🔇!");
                                        break;

                                    case "Минута":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} минуту🔇!");
                                        break;

                                    case "минута":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} минуту🔇!");
                                        break;


                                    //Дневной тип
                                    case "День":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddDays(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} день🔇!");
                                        break;

                                    case "день":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddDays(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} день🔇!");
                                        break;

                                    case "Дней":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddDays(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} дней🔇!");
                                        break;

                                    case "дней":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddDays(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} дней🔇!");
                                        break;

                                    case "Дня":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddDays(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} дня🔇!");
                                        break;

                                    case "дня":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddDays(TimeOut), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} дня🔇!");
                                        break;


                                    //Месячный тип
                                    case "Месяц":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMonths(Convert.ToInt32(TimeOut)), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} месяц🔇!");
                                        break;

                                    case "месяц":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMonths(Convert.ToInt32(TimeOut)), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} месяц🔇!");
                                        break;

                                    case "Месяца":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMonths(Convert.ToInt32(TimeOut)), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} месяца🔇!");
                                        break;

                                    case "месяца":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMonths(Convert.ToInt32(TimeOut)), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} месяца🔇!");
                                        break;

                                    case "Месяцев":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMonths(Convert.ToInt32(TimeOut)), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} месяцев🔇!");
                                        break;

                                    case "месяцев":
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMonths(Convert.ToInt32(TimeOut)), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} был заглушен на {SplitSymbol[1]} месяцев🔇!");
                                        break;
                                }
                        }
                        return;
                    }

                    if (msg.Text.StartsWith("Бан\n") | msg.Text.StartsWith("бан\n"))
                    {
                        if (msg.Chat.Id == msg.From.Id)
                        {
                            return;
                        }

                        else if (msg.ReplyToMessage.From.IsBot)
                        {
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} является ботом🤖!");
                            return;
                        }

                        else if (msg.ReplyToMessage == null)
                        {
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "⛔Вы не указали пользователя!");
                            return;
                        }
                        else if (msg.ReplyToMessage != null)
                        {
                            var InMsg = msg.Text;
                            InMsg = msg.Text.Split('\n')[1];
                            var BanText = InMsg;

                            Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                            bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                            var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                            var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                            if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔@{msg.From.Username}, вы не являетесь администратором/создателем чата!");
                                return;
                            }
                            else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗@{msg.ReplyToMessage.From.Username} является администратором чата «{msg.Chat.Title}»");
                                return;
                            }
                            else
                                await client.KickChatMemberAsync(userId: msg.ReplyToMessage.From.Id, chatId: msg.Chat.Id, cancellationToken: default);
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❌Участник @{msg.ReplyToMessage.From.Username} был забанен модератором: @{msg.From.Username}!\nПричина: <i>{BanText}</i>", parseMode: ParseMode.Html);
                        }
                    }

                    if (msg.Text.StartsWith("/NewRules("))
                    {
                        if (msg.Chat.Id == msg.From.Id)
                        {
                            return;
                        }
                        else if (msg.Chat.Id != msg.From.Id)
                        {
                            Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                            bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                            var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                            if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔@{msg.From.Username}, вы не являетесь администратором/создателем чата!");
                                return;
                            }

                            else
                            {
                                string InText = msg.Text;
                                InText = msg.Text.Split('(')[1].Split(')')[0];
                                var NewRule = InText;

                                if (sql.State == ConnectionState.Closed)
                                {
                                    sql.Open();
                                    SqlCommand command = new SqlCommand($"insert into ChatRules(ChatId, TextRule) values({msg.Chat.Id}, {NewRule})");
                                    await command.ExecuteNonQueryAsync();
                                    sql.Close();
                                }
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Правила чата успешно обновлены✅");

                            }
                        }

                    }

                    if (msg.Text.ToUpper() == "ПРАВИЛА")
                    {
                        if (sql.State == ConnectionState.Closed)
                        {
                            sql.Open();
                            SqlCommand command = new SqlCommand($"");
                            await command.ExecuteNonQueryAsync();

                        }
                    }

                    if (msg.Text.StartsWith("/getchatid"))
                    {
                        if (msg.Chat.Id == msg.From.Id)
                        {
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Если вы хотите получить ID вашего аккаунта введите команду /getmyid");
                            return;
                        }
                        else if (msg.Chat.Id != msg.From.Id)
                        {
                            Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                            bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                            var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                            if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔@{msg.From.Username}, вы не являетесь администратором/создателем чата!");
                                return;
                            }

                            else
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"ID данного чата🆔: <code>{msg.Chat.Id}</code>", parseMode: ParseMode.Html); ;
                            return;
                        }
                    }

                    if (msg.Text == "/getmyid")
                    {
                        if (msg.Chat.Id == msg.From.Id)
                        {
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Ваш личный ID: <code>{msg.From.Id}</code>", parseMode: ParseMode.Html);
                            return;
                        }
                        else if (msg.Chat.Id != msg.From.Id)
                        {
                            return;
                        }
                    }

                    if (msg.Text.StartsWith("оценку") | msg.Text.StartsWith("Оценку"))
                    {
                        string[] rateAnswer = { "-10/10, что за кринж ты кинул🤢", "4/10, так себе сделано🙄", "0/10, фигня, переделывай", "100/10, просто круче некуда🤩", "7/10, неплохо, вполне достойно🙂", "10/10, супер✨" };
                        Random rndAnswer = new Random();
                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, rateAnswer[rndAnswer.Next(rateAnswer.Length)]);
                    }

                    if (msg.Text.ToUpper() == "ЛИВАЙ, ЛАУРА")
                    {
                        if (msg.From.Id != 5287392256)
                        {
                            return;
                        }
                        else
                        {
                            if (msg.Chat.Id != msg.From.Id)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Создатель приказал выйти с данного чата!\nВсем удачи...");
                                await Task.Delay(1500);
                                await client.LeaveChatAsync(chatId: msg.Chat.Id);
                                return;
                            }
                            else
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Ланс, это лс .-.");
                                return;
                            }
                        }
                    }    

                    if (msg.Text.ToUpper() == "DELMSG")
                    {
                        if (msg.Chat.Id == msg.From.Id)
                        {
                            return;
                        }

                        else if (msg.ReplyToMessage == null)
                        {
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "⛔Вы не указали сообщение пользователя!");
                            return;
                        }
                        else if (msg.ReplyToMessage != null)
                        {
                            Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                            bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                            var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                            var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                            if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔@{msg.From.Username}, вы не являетесь администратором/создателем чата!");
                                return;
                            }
                            else
                                await client.DeleteMessageAsync(messageId: msg.ReplyToMessage.MessageId, chatId: msg.Chat.Id, cancellationToken: default);
                                await client.DeleteMessageAsync(messageId: msg.MessageId, chatId: msg.Chat.Id);
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Сообщение было успешно удалено✅", parseMode: ParseMode.Html);
                        }
                    }

                    if (msg.Text.StartsWith("Погода в") | msg.Text.StartsWith("погода в"))
                    {
                        String[] InpResponse = msg.Text.Split(' ');
                        var inputCityName = InpResponse[2];
                        WeatherApi.Weather(inputCityName);
                        WeatherApi.Celsius(celsius: WeatherApi.temperatureCity);
                        WeatherApi.WindCourse(WeatherApi.windDegCity);

                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<b>{WeatherApi.answer}</b>\n\n\n<b>Информация города <code>{WeatherApi.nameCity}</code>:</b>\n\nТемпература города🌡: <code>{Math.Round(WeatherApi.temperatureCity)}°C</code>\nОщущается как: <code>{Math.Round(WeatherApi.tempFeelsLikeCity)}°C</code>\nКоординаты города🗺: <code>{WeatherApi.lonCity} {WeatherApi.latCity}</code>\nПогода⛅: <code>{WeatherApi.weatherCity}</code>\nДавление⬇:<code>{WeatherApi.pressureCity} гПа</code>\nСтрана🏳: <code>{WeatherApi.countryCity}</code>\nВидимость👁: <code>{WeatherApi.visibilityCity} км</code>\nВлажность💧: <code>{WeatherApi.humidityCity}%</code>\nСкорость ветра🌫: <code>{WeatherApi.windSpeedCity} км/ч</code>\nНаправление ветра: <code>{WeatherApi.windDegCity}° ({WeatherApi.WindAnswer})</code>", parseMode: ParseMode.Html);
                    }

                    if (msg.Text.StartsWith("Лаура инфа") | msg.Text.StartsWith("лаура инфа"))
                    {
                        Random rndm_count = new Random();
                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"🤔Я думаю, что это возможно на {rndm_count.Next(0, 100)}%");
                    }

                    //if (msg.Text.StartsWith("Лаура кто сегодня ") | msg.Text.StartsWith("лаура кто сегодня "))
                    //{
                    //    var SplitPredictPhrase = msg.Text.Split("Лаура кто сегодня ");
                    //    var PredictPhrase = SplitPredictPhrase[1];

                    //    String[]MembersList = client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: );


                    //    await client.SendTextMessageAsync();
                    //}

                }


                catch(ApiRequestException exc)
                {
                    Console.WriteLine(exc.ToString());
                    return;
                }
                catch(FormatException exc2)
                {
                    Console.WriteLine(exc2.ToString());
                    return;
                }
                //if (msg.Text.StartsWith("Бан @") | msg.Text.StartsWith("бан @"))
                //{
                //    if (msg.Chat.Id == msg.From.Id)
                //    {
                //        return;
                //    }
                //    else
                //    {
                //        string MemberTargetUsername = msg.Text;
                //        MemberTargetUsername = msg.Text.Split('@')[1].Split(Username)[0];
                //        var username = MemberTargetUsername;



                //        Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                //        bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                //        if (admins.FirstOrDefault(a => { return a.User != null && a.User.Username == msg.From.Username; }) == null)
                //        {
                //            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔@{msg.From.Username}, вы не являетесь администратором/создателем чата!");
                //            return;
                //        }
                //        else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Username == username; }) != null)
                //        {
                //            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗@{username} является администратором чата «{msg.Chat.Title}»");
                //            return;
                //        }
                //        else
                //            await client.KickChatMemberAsync(username: user.Id, chatId: msg.Chat.Id, cancellationToken: default);
                //        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❌Участник @{username} был забанен модератором: @{msg.From.Username}!");
                //        return;
                //    }

                //}


            }

        }

            private static IReplyMarkup GetButtons() => new ReplyKeyboardMarkup
            {
                ResizeKeyboard = true,
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{new KeyboardButton{ Text = "Инструкция📚" }, new KeyboardButton { Text = "О нас📒"}, },
                    new List<KeyboardButton>{new KeyboardButton{ Text = "Добавить бота в чат➕"}, new KeyboardButton{ Text = "Игровые режимы🎮" } }
                }
            };


            private static IReplyMarkup GameModes() => new ReplyKeyboardMarkup
            {
                ResizeKeyboard = true,
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{new KeyboardButton{ Text = "Кнопка недоступна" } },
                    new List<KeyboardButton>{new KeyboardButton{ Text = "Кнопка недоступна" } },
                    new List<KeyboardButton>{new KeyboardButton{ Text = "Назад🔃"} }
                }
            };

    }
}