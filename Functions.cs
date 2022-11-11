using System;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using TL;
using System.Threading.Tasks;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

namespace Functions
{
    public class MessageParser
    {
        private static int api_id = 0;
        private static string api_hash = "api_hash";

        public static bool ScanOnBadWords(string message)
        {
            string[] words = message.Split(' ');
            bool status = false;
            try
            {
                for (int i = 0; i < words.Length; i++)
                {
                    for (int j = 0; j <= BadLyrics.data.Length; j++)
                    {
                        if (words[i] == BadLyrics.data[j])
                        {
                            status = true;
                            break;
                        }
                        else
                        {
                            status = false;
                        }
                    }
                }
            }
            catch(IndexOutOfRangeException exc)
            {
                for (int i = 0; i < BadLyrics.data.Length; i++)
                {
                    if(message == BadLyrics.data[i])
                    {
                        status = true;
                        break;
                    }
                    else
                    {
                        status = false;
                    }
                }
            }
            return status;
        }

        public static async Task<object[]> GetRandomMemberAsync(long chatId, string token, ChatType chatType, int messageId)
        {
            Client client = new Client(api_id, api_hash);
            await client.LoginBotIfNeeded(token);
            client.CollectAccessHash = true;
            Messages_ChatFull chatFull = null;


            if (chatType == ChatType.Supergroup)
            {
                chatId = Convert.ToInt64(chatId.ToString().Split("-100")[1]);
                var acs_h = client.GetAccessHashFor<Channel>(chatId);
                InputChannelBase peerChan = new InputChannel(chatId, acs_h);
                chatFull = await client.Channels_GetFullChannel(peerChan);
            }
            else
                chatId = Convert.ToInt64(chatId.ToString().Split("-100")[1]);
                chatFull = await client.Messages_GetFullChat(chatId);

            Random random = new Random();

            var resul = random.Next(chatFull.users.Count);
            var firstName = chatFull.users[resul].first_name;
            var userId = chatFull.users[resul].id;
            object[] data = { firstName, userId };
            return data;
        }

        public static async Task<object[]> GetUserIdByUsernameAsync(string username, string token, Client client)
        {
            try
            {
                await client.LoginBotIfNeeded(token);
                var user = await client.Contacts_ResolveUsername(username);
                object[] userData = { user.User.id, user.User.first_name };
                await client.Auth_LogOut();
                return userData;
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc);
                return null;
            }
        }
    }
    public class CapchaButtons
    {
        private static InlineKeyboardMarkup Variant1 = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("😧", "yes_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha")
        });
        private static InlineKeyboardMarkup Variant2 = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😧", "yes_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha")
        });
        private static InlineKeyboardMarkup Variant3 = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😧", "yes_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha")
        });
        private static InlineKeyboardMarkup Variant4 = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😧", "yes_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha")
        });
        private static InlineKeyboardMarkup Variant5 = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😦", "no_capcha"),
            InlineKeyboardButton.WithCallbackData("😧", "yes_capcha")
        });

        public static InlineKeyboardMarkup GetRandomCapcha()
        {
            Random random = new Random();
            var Variant = random.Next(1,5);
            InlineKeyboardMarkup keyboard;
            InlineKeyboardMarkup result = null;
            if (Variant == 1)
            {
                result = Variant1;
            }
            if (Variant == 2)
            {
                result = Variant2;
            }
            if (Variant == 3)
            {
                result = Variant3;
            }
            if (Variant == 4)
            {
                result = Variant4;
            }
            if (Variant == 5)
            {
                result = Variant5;
            }
            keyboard = result;
            return keyboard;
        }
    }
    public class Cleaner
    {
        public static void DeleteChatData(long chatId)
        {
            Connector.Connector.DeleteUserBot(chatId);
            Connector.Connector.DeleteChatRules(chatId);
            Connector.Connector.DeleteCapchaSetting(chatId);
            Connector.Connector.DeleteNightMode(chatId);
        }
    }
}
