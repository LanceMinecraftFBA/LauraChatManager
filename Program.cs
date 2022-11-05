using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using Telegram.Bot.Types;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Threading.Tasks;

using Connector;
using WeatherApiClass;
using AIClass;
using BotSettings;
using Functions;

namespace Laura_Bot_Chat_Manager
{

    class Program
    {

        private static string token { get; set; } = "5565507778:AAEP06oTph93z6Y2BeGxZX0kjXMROxPlQRQ";
        private static long myId = 5565507778;

        //private static string token { get; set; } = "5562501226:AAHKm5EtifOR_SLYH2Qx160JCOlxj9z8Pjg";
        //private static long myId = 5562501226;

        private static TelegramBotClient client;
        private static long channel = 777000;
        private static long group = 1087968824;
        private static long anonim = 136817688;
        private static string chat_id;
        private static long fbaNews = -1001562946820;
        private static DateTime timeOfStart = DateTime.Now;

        private static string host { get; set; } = "185.252.147.37";
        private static string user { get; set; } = "lance";
        private static string passwrd { get; set; } = "V1oletIsTop!";
        private static string database { get; set; } = "lance_db";

        private static string conn { get; set; } = $"server={host};user={user};database={database};password={passwrd}";

        static void Main(string[] args)
        {

            client = new TelegramBotClient(token);
            client.StartReceiving(new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery, UpdateType.Unknown});
            client.OnMessage += OnMessageHandler;
            client.OnCallbackQuery += OnCallbackQueryHandler;
            Console.Title = "LauraBotLogConsole";
            Console.WriteLine("Бот Лаура запущен!");
            Console.WriteLine("===============================");
            Console.WriteLine("Bot Developed on C#\nDev: @LanceMinecraft,\n@TheShadow_hk (Telegram)");
            Console.WriteLine("Version: 1.6.3 open Beta");
            Console.WriteLine("===============================");
            Console.WriteLine($"Time of start: {DateTime.Now}");
            Console.WriteLine("Begin of console log:");
            Console.WriteLine(DateTime.Now + " Checker was started!");
            while (true)
            {
                Checker();
                Thread.Sleep(1250);
            }
            client.StopReceiving();
        }

        private static async void OnCallbackQueryHandler(object sender, CallbackQueryEventArgs e)
        {
            var call = e.CallbackQuery;
            if(call != null)
            {
                try
                {
                    var data = Connector.Connector.GetCapchaByUser(call.From.Id, call.Message.Chat.Id);
                    if(data != null)
                    {
                        if (call.Data == "no_capcha" && call.From.Id == Convert.ToInt64(data[1].ToString()))
                        {
                            var data1 = Connector.Connector.GetCapchaByUser(call.From.Id, call.Message.Chat.Id);
                            if (data1 != null)
                            {
                                var attemps = Convert.ToInt32(data1[4]) - 1;
                                var deadLine = DateTime.Parse(data1[3].ToString());
                                var message = Convert.ToInt32(data1[2].ToString());
                                if (attemps <= 0)
                                {
                                    Connector.Connector.DeleteCapchaUser(call.Message.Chat.Id, call.From.Id);
                                    try
                                    {
                                        await client.DeleteMessageAsync(call.Message.Chat.Id, message);
                                        await client.SendTextMessageAsync(call.Message.Chat.Id, $"Пользователь <a href=\"tg://user?id={call.From.Id}\">{call.From.FirstName}</a> будет изгнан из чата, потому что он потратил все попытки❌", ParseMode.Html, disableWebPagePreview: true);
                                        await client.KickChatMemberAsync(call.Message.Chat.Id, call.From.Id);
                                    }
                                    catch (ApiRequestException exe)
                                    {
                                        await client.SendTextMessageAsync(call.Message.Chat.Id, $"<code>👾Error occured: {exe.Message}</code>", ParseMode.Html);
                                    }
                                }
                                else
                                    Connector.Connector.UpdateAttempsUser(call.From.Id, call.Message.Chat.Id, attemps);
                                    await client.AnswerCallbackQueryAsync(call.Id, showAlert: true, text: $"🙅🏻‍♀️Вы выбрали неверный ответ!\nОсталось {deadLine.Minute - DateTime.Now.Minute} минут⏱\nПопытки: {attemps}");
                            }
                        }
                        if (call.Data == "yes_capcha" && call.From.Id == Convert.ToInt64(data[1].ToString()))
                        {
                            var message = Convert.ToInt32(Connector.Connector.GetCapchaByUser(call.From.Id, call.Message.Chat.Id)[2]);
                            Connector.Connector.DeleteCapchaUser(call.Message.Chat.Id, call.From.Id);
                            await client.RestrictChatMemberAsync(call.Message.Chat.Id, call.From.Id, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false }, DateTime.Now.AddMinutes(1));
                            await client.DeleteMessageAsync(call.Message.Chat.Id, message);
                            await client.SendTextMessageAsync(call.Message.Chat.Id, $"<b>Вы решили капчу✅!</b> Через минуту вы можете свободно общаться🤗", ParseMode.Html);
                        }
                    }
                    else if(data == null)
                    {
                        if (call.Data != null)
                        {
                            await client.AnswerCallbackQueryAsync(call.Id, showAlert: true, text: "Капча не для вас⛔️");
                        }
                    }
                }
                catch(ApiRequestException exc)
                {
                    Console.WriteLine(exc);
                }
                catch(IndexOutOfRangeException exc2)
                {
                    Console.WriteLine(exc2);
                }
                catch(NullReferenceException exc3)
                {
                    Console.WriteLine(exc3);
                }
            }
        }

        private static async void Checker()
        {
            long chatId = 0;
            long userId = 0;

            try
            {
                MySqlConnection mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                #region Антиспам хэндлер
                var counter = 0;
                mySql.Open();
                myCmd = new MySqlCommand("SELECT COUNT(*) FROM antispam", mySql);
                myReader = myCmd.ExecuteReader();
                if (myReader.Read() == false)
                {
                    Console.WriteLine("no spam");
                    mySql.Close();
                }
                else
                {
                    counter = Convert.ToInt32(myReader[0].ToString());
                    mySql.Close();

                    var i = 0;
                    while (i < counter)
                    {
                        mySql.Open();
                        var getSpam = "SELECT * FROM antispam";

                        myCmd = new MySqlCommand(getSpam, mySql);
                        myReader = myCmd.ExecuteReader();
                        if (myReader.Read() != false)
                        {
                            userId = Convert.ToInt64(myReader[2].ToString());
                            chatId = Convert.ToInt64(myReader[1].ToString());

                            mySql.Close();

                            Connector.Connector.DeleteAntiSpam(chatId, userId);
                            Console.WriteLine($"Deleting AS for {userId} from {chatId}");
                        }
                        else
                        {
                            mySql.Close();
                            return;
                        }
                        i++;
                    }
                }
                #endregion

                #region Варны
                mySql.Open();
                var checkDL = "SELECT * FROM warn_users";
                myCmd = new MySqlCommand(checkDL, mySql);
                myReader = myCmd.ExecuteReader();
                while (myReader.Read())
                {
                    userId = Convert.ToInt64(myReader[1].ToString());
                    chatId = Convert.ToInt64(myReader[2].ToString());
                    var warns = myReader[3].ToString();
                    var deadLine = DateTime.Parse(myReader[4].ToString());
                    var dateNow = DateTime.Now;
                    if (deadLine <= dateNow)
                    {
                        Connector.Connector.DeleteWarnUser(userId, chatId);
                        Connector.Connector.DeleteReasonsOfWarns(userId, chatId);
                        Console.WriteLine($"DeadLine for {userId} was expired!");
                    }
                    else
                    {
                        Console.WriteLine($"DeadLine for {userId} ended in {deadLine}!");
                    }

                }
                mySql.Close();
                #endregion

                #region Проверка nightmode
                mySql.Open();
                var nightmode = $"SELECT chat_id, nightmode, statemode, status FROM nightmode";
                myCmd = new MySqlCommand(nightmode, mySql);
                myReader = myCmd.ExecuteReader();
                if(myReader.Read() == false)
                {
                    Console.WriteLine("not found");
                    mySql.Close();
                }
                else
                {
                    mySql.Close();

                    mySql.Open();
                    myCmd = new MySqlCommand(nightmode, mySql);
                    myReader = myCmd.ExecuteReader();
                    while (myReader.Read())
                    {
                        chatId = Convert.ToInt64(myReader[0].ToString());
                        var night = myReader[1].ToString().Split(':');
                        var state = myReader[2].ToString().Split(':');
                        var status = myReader[3].ToString();

                        if(status == "statemode")
                        {
                            if (Convert.ToInt32(night[0]) == DateTime.Now.Hour && Convert.ToInt32(night[1]) <= DateTime.Now.Minute)
                            {
                                Connector.Connector.UpdateStatusNight(chatId, "nightmode");
                                await client.SetChatPermissionsAsync(chatId, permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                await client.SendTextMessageAsync(chatId, $"<b>🌙Объявляется ночной режим!</b>\nТеперь все участники чата не могут писать сообщения до {myReader[2].ToString()}", ParseMode.Html);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("not today");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                        else if(status == "nightmode")
                        {
                            if (Convert.ToInt32(state[0]) == DateTime.Now.Hour && Convert.ToInt32(state[1]) <= DateTime.Now.Minute)
                            {
                                Connector.Connector.UpdateStatusNight(chatId, "statemode");
                                await client.SetChatPermissionsAsync(chatId, permissions: new ChatPermissions { CanSendMessages = true, CanSendMediaMessages = true, CanSendOtherMessages = true });
                                await client.SendTextMessageAsync(chatId, $"<b>🌞Ночной режим завершён!</b>\nТеперь все участники чата могут свободно писать сообщения", ParseMode.Html);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("not today");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }

                    }
                    mySql.Close();
                }
                #endregion

                #region Рассылка погоды
                mySql.Open();
                var query = "SELECT user_id, city, next_response FROM weather_subs";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                if (myReader.Read() == false)
                {
                    mySql.Close();
                }
                else
                {
                    mySql.Close();

                    mySql.Open();
                    myCmd = new MySqlCommand(query, mySql);
                    myReader = myCmd.ExecuteReader();
                    while (myReader.Read())
                    {
                        var dl = DateTime.Parse(myReader[2].ToString());
                        userId = Convert.ToInt64(myReader[0].ToString());
                        var city = myReader[1].ToString();

                        if (dl > DateTime.Now)
                        {
                            Console.WriteLine("He will not get response");
                        }
                        else
                        {
                            WeatherApi.Weather(city);
                            WeatherApi.Celsius(celsius: WeatherApi.temperatureCity);
                            WeatherApi.WindCourse(WeatherApi.windDegCity);

                            var MainWeather = await client.SendTextMessageAsync(chatId: userId, $"<b>Прогноз погоды {dl}</b>\n<b>Информация города <code>{WeatherApi.nameCity}</code>:</b>\nИндекс Air Pollution💨: <code>{WeatherApi.aqi}</code>\nТемпература города🌡: <code>{Math.Round(WeatherApi.temperatureCity)}°C</code>\nОщущается как: <code>{Math.Round(WeatherApi.tempFeelsLikeCity)}°C</code>\nПогода⛅: <code>{WeatherApi.weatherCity}</code>\nДавление⬇:<code>{WeatherApi.pressureCity} гПа</code>\nВидимость👁: <code>{WeatherApi.visibilityCity} км</code>\nВлажность💧: <code>{WeatherApi.humidityCity}%</code>\nСкорость ветра🌫: <code>{WeatherApi.windSpeedCity} м/c</code>\nНаправление ветра: <code>{WeatherApi.windDegCity}° ({WeatherApi.WindAnswer})</code>", parseMode: ParseMode.Html);
                            await client.SendTextMessageAsync(
                                        userId,
                                        replyToMessageId: MainWeather.MessageId,
                                        text:
@$"<b>AirPollution - Компоненты воздуха:</b>
CO: <i>{WeatherApi.co} мкг/м3</i>
NO: <i>{WeatherApi.no} мкг/м3</i>
NO2: <i>{WeatherApi.no2} мкг/м3</i>
O3: <i>{WeatherApi.o3} мкг/м3</i>
SO2: <i>{WeatherApi.so2} мкг/м3</i>
PM2.5: <i>{WeatherApi.pm2_5} мкг/м3</i>
PM10: <i>{WeatherApi.pm10} мкг/м3</i>
NH3: <i>{WeatherApi.nh3} мкг/м3</i>
",
                                        parseMode: ParseMode.Html);

                            Thread.Sleep(125);
                            await client.SendTextMessageAsync(userId, "🌦Вы получили прогноз погоды, потому что вы подписались на рассылку прогноза погоды по выбранному региону.\n\nВы можете в любой момент отключить рассылку прогноза командой /unsub_weather🔕");
                            Connector.Connector.UpdateSubData(userId, city, DateTime.Now.AddHours(3));
                        }
                    }
                    mySql.Close();
                }
                #endregion
                Thread.Sleep(750);

                #region Проверка капчи юзера
                mySql.Open();
                var temp_capcha = "SELECT * FROM capcha_temp";
                myCmd = new MySqlCommand(temp_capcha, mySql);
                myReader = myCmd.ExecuteReader();
                if(myReader.Read() == false)
                {
                    Console.WriteLine("No users with capcha : (");
                    mySql.Close();
                }
                else
                {
                    mySql.Close();

                    mySql.Open();
                    myCmd = new MySqlCommand(temp_capcha, mySql);
                    myReader = myCmd.ExecuteReader();
                    while(myReader.Read())
                    {
                        chatId = Convert.ToInt64(myReader[1].ToString());
                        userId = Convert.ToInt64(myReader[2].ToString());
                        var messageId = Convert.ToInt32(myReader[3].ToString());
                        var end_in = DateTime.Parse(myReader[4].ToString());

                        if(end_in <= DateTime.Now)
                        {
                            Connector.Connector.DeleteCapchaUser(chatId, userId);
                            await client.DeleteMessageAsync(chatId, messageId);
                            try
                            {
                                await client.KickChatMemberAsync(chatId, userId);
                                await client.UnbanChatMemberAsync(chatId, userId);
                                await client.SendTextMessageAsync(chatId, $"Пользователь кикнут из чата, потому что он не доказал, что он не робот🙅🏻‍♀️", ParseMode.Html, disableWebPagePreview: true);
                            }
                            catch (ApiRequestException exe)
                            {
                                await client.SendTextMessageAsync(chatId, $"<code>👾Error occured: {exe.Message}</code>", ParseMode.Html);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Is capcha");
                        }
                    }
                    mySql.Close();
                }
                #endregion

                #region Проверка контроллера рейтинга
                mySql.Open();
                var get_soc_control = $"SELECT * FROM social_control";
                myCmd = new MySqlCommand(get_soc_control, mySql);
                myReader = myCmd.ExecuteReader();
                if (myReader.Read() == false)
                {
                    Console.WriteLine("not found");
                }
                else
                {
                    mySql.Close();

                    mySql.Open();
                    myCmd = new MySqlCommand(get_soc_control, mySql);
                    myReader = myCmd.ExecuteReader();
                    while (myReader.Read())
                    {
                        DateTime deadL = DateTime.Parse(myReader[4].ToString());
                        var targId = Convert.ToInt64(myReader[2].ToString());
                        userId = Convert.ToInt64(myReader[1].ToString());
                        if (deadL <= DateTime.Now)
                        {
                            Connector.Connector.DeleteControlSocData(userId, targId);
                        }
                        else
                        {
                            Console.WriteLine("not now");
                        }
                    }
                    mySql.Close();
                }
                #endregion
            }
            catch (ApiRequestException except)
            {
                Console.WriteLine(except);
                if(except.Message == "chat not found")
                {
                    Cleaner.DeleteChatData(chatId);
                }    
            }
            catch (MySqlException eeee)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error with db!\n" + eeee.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {

            var msg = e.Message;
            var Admin = ChatMemberStatus.Administrator;
            var Member = ChatMemberStatus.Member;
            var Owner = ChatMemberStatus.Creator;
            var Banned = ChatMemberStatus.Kicked;
            var Muted = ChatMemberStatus.Restricted;


            if (msg != null)
            {
                if (msg.NewChatMembers != null)
                {
                    try
                    {
                        if (msg.NewChatMembers[0].Id == myId)
                        {
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "😌Я так рада, что меня добавили в чат\nЯ обещаю работать качественно в посте модераторов😉!");
                            Console.WriteLine($"Bot was added in chat: {msg.Chat.Id}");
                        }
                        else
                        {
                            var USERID = msg.NewChatMembers[0].Id;
                            var FIRSTNAME = msg.NewChatMembers[0].FirstName;
                            var LASTNAME = msg.NewChatMembers[0].LastName;
                            var URLFRSTNAME = $"<a href={USERID}>{FIRSTNAME}</a>";
                            var URLFULLNAME = $"<a href={USERID}>{FIRSTNAME} {LASTNAME}</a>";
                            var TITLECHAT = msg.Chat.Title;

                            Connector.Connector.GetSocialData(msg.NewChatMembers[0].Id);
                            await client.SendStickerAsync(chatId: msg.Chat.Id, sticker: "https://t.me/ScladOfRes/110", replyToMessageId: msg.MessageId);
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Добро пожаловать в чат, <a href = \"tg://user?id={msg.NewChatMembers[0].Id}\">{msg.NewChatMembers[0].FirstName}</a>!\nВы попали в «{msg.Chat.Title}»!", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            Thread.Sleep(250);
                            if (Connector.Connector.message == "not rated")
                            {
                                await client.SendTextMessageAsync(msg.Chat.Id, "Об этом участнике чата мне неизвестно, будьте аккуратны с ним!");
                            }
                            else
                            {
                                var mesg = "";
                                if (Connector.Connector.user_rating < 0)
                                {
                                    mesg = $"📉\n📛Данный участник чата может быть негативным, диалог с ним может быть токсичным☢️";
                                }
                                else
                                {
                                    mesg = $"📈\nДанный участник чата является нормальным, общение с ним должно быть нормальным🧸";
                                }
                                await client.SendTextMessageAsync(msg.Chat.Id, $"<b>🗄Рейтинг участника чата:</b> {Connector.Connector.user_rating}{mesg}", ParseMode.Html);
                            }
                            Console.WriteLine($"{DateTime.Now} New member in chat: {msg.Chat.Id}");
                            Thread.Sleep(350);
                            ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                            if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                            {
                                return;
                            }
                            else
                            {
                                Connector.Connector.GetCapchaSetting(msg.Chat.Id);
                                if (Connector.Connector.message == "not found")
                                {
                                    Connector.Connector.CreateCapchaSetting(msg.Chat.Id, "off", 5);
                                }
                                else
                                {
                                    if (Connector.Connector.capcha_status == "off")
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        var mesg = await client.SendTextMessageAsync(msg.Chat.Id, $"👾<a href=\"{msg.NewChatMembers[0].Id}\">{msg.NewChatMembers[0].FirstName}</a>, у вас есть {Connector.Connector.minutes} минут, чтобы доказать, что вы не бот!\nНайдите смайл, который будет отличаться от 4 остальных смайлов🔍:", ParseMode.Html, replyMarkup: CapchaButtons.GetRandomCapcha());
                                        try
                                        {
                                            await client.RestrictChatMemberAsync(msg.Chat.Id, msg.NewChatMembers[0].Id, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false }, DateTime.Now.AddYears(1));
                                            Connector.Connector.TempingCapchaUser(msg.NewChatMembers[0].Id, msg.Chat.Id, mesg.MessageId, DateTime.Now.AddMinutes(Connector.Connector.minutes), 4);
                                        }
                                        catch (ApiRequestException exe)
                                        {
                                            await client.SendTextMessageAsync(msg.Chat.Id, $"<code>👾Error occured: {exe.Message}</code>", ParseMode.Html);
                                            Connector.Connector.DeleteCapchaUser(msg.Chat.Id, msg.NewChatMembers[0].Id);
                                        }
                                        catch (IndexOutOfRangeException exece)
                                        {
                                            Console.WriteLine("Oh shit");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (ApiRequestException exc)
                    {
                        Console.WriteLine(exc);
                    }

                }

                if (msg.LeftChatMember != null)
                {
                    if (msg.LeftChatMember.Id == myId)
                    {
                        Connector.Connector.DeleteUserBot(msg.Chat.Id);
                        Connector.Connector.DeleteNightMode(msg.Chat.Id);
                        return;
                    }
                    else if(msg.From.Id != myId)
                    {
                        Connector.Connector.DeleteReasonsOfWarns(msg.LeftChatMember.Id, msg.Chat.Id);
                        Connector.Connector.DeleteWarnUser(msg.LeftChatMember.Id, msg.Chat.Id);
                        await client.SendStickerAsync(chatId: msg.Chat.Id, sticker: "https://t.me/ScladOfRes/59");
                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.LeftChatMember.Id}\">{msg.LeftChatMember.FirstName}</a> покинул чат!", parseMode: ParseMode.Html, disableWebPagePreview: true);

                        Console.WriteLine($"{DateTime.Now} Member ID{msg.LeftChatMember.Id} left from chat: {msg.Chat.Id}");
                    }
                }

                if (msg.Photo != null)
                {
                    if (msg.Chat.Id == fbaNews && msg.From.Id == channel)
                    {
                        try
                        {
                            var news = msg.Caption;
                            var photo = msg.Photo[0].FileId;

                            var myComment = Connector.Connector.GetNotifComment(msg.Chat.Id);
                            if (myComment == null)
                            {
                                Console.WriteLine("not found");
                            }
                            else
                                await client.SendTextMessageAsync(msg.Chat.Id, myComment, ParseMode.Html, replyToMessageId: msg.MessageId);

                            MySqlConnection MySql1 = new MySqlConnection(conn);
                            MySqlCommand sqlCommand1;
                            MySqlDataReader sqlReader1;

                            MySql1.Open();
                            var sql1 = $"SELECT chat_id FROM userbot WHERE is_get = 'on'";
                            sqlCommand1 = new MySqlCommand(sql1, MySql1);
                            sqlReader1 = sqlCommand1.ExecuteReader();
                            while (sqlReader1.Read())
                            {
                                try
                                {
                                    await client.SendPhotoAsync(Convert.ToInt64(sqlReader1[0]), photo: photo, caption: $"<b>📬Говорит <a href=\"https://t.me/FBA_Studio\">FBA Studio</a>:</b>\n<i>{news}</i>", ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Посетить канал", "https://t.me/FBA_Studio")));
                                    Thread.Sleep(1500);
                                }
                                catch (ApiRequestException exception)
                                {
                                    Console.WriteLine(exception.Message);
                                    Connector.Connector.DeleteUserBot(Convert.ToInt64(sqlReader1[0]));
                                }
                            }
                        }
                        catch(Exception er)
                        {
                            Console.WriteLine(er);
                        }
                    }
                    else if (msg.From.Id == channel)
                    {
                        var myComment = Connector.Connector.GetNotifComment(msg.Chat.Id);
                        if (myComment == null)
                        {
                            return;
                        }
                        else
                            await client.SendTextMessageAsync(msg.Chat.Id, myComment, ParseMode.Html, replyToMessageId: msg.MessageId);
                    }
                    else
                    {
                        return;
                    }
                }

                if (msg.Video != null)
                {
                    if (msg.Chat.Id == fbaNews & msg.From.Id == channel)
                    {
                        try
                        {
                            var news = msg.Caption;
                            var video = msg.Video.FileId;

                            var myComment = Connector.Connector.GetNotifComment(msg.Chat.Id);
                            if (myComment == null)
                            {
                                Console.WriteLine("not found");
                            }
                            else
                                await client.SendTextMessageAsync(msg.Chat.Id, myComment, ParseMode.Html, replyToMessageId: msg.MessageId);

                            MySqlConnection MySql1 = new MySqlConnection(conn);
                            MySqlCommand sqlCommand1;
                            MySqlDataReader sqlReader1;

                            MySql1.Open();
                            var sql1 = $"SELECT chat_id FROM userbot WHERE is_get = 'on'";
                            sqlCommand1 = new MySqlCommand(sql1, MySql1);
                            sqlReader1 = sqlCommand1.ExecuteReader();
                            while (sqlReader1.Read())
                            {
                                try
                                {
                                    await client.SendVideoAsync(chatId: Convert.ToInt64(sqlReader1[0]), video: video, caption: $"<b>📬Говорит <a href=\"https://t.me/FBA_Studio\">FBA Studio</a>:</b>\n<i>{news}</i>", parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Посетить канал", "https://t.me/FBA_Studio")));
                                    Thread.Sleep(1500);
                                }
                                catch (ApiRequestException exception)
                                {
                                    Console.WriteLine(exception.Message);
                                    Connector.Connector.DeleteUserBot(Convert.ToInt64(sqlReader1[0]));
                                }
                            }
                        }
                        catch(Exception er)
                        {
                            Console.WriteLine(er);
                        }
                    }
                    else if (msg.From.Id == channel)
                    {
                        var myComment = Connector.Connector.GetNotifComment(msg.Chat.Id);
                        if (myComment == null)
                        {
                            return;
                        }
                        else
                            await client.SendTextMessageAsync(msg.Chat.Id, myComment, ParseMode.Html, replyToMessageId: msg.MessageId);
                    }
                    else
                    {
                        return;
                    }
                }

                if (msg.Audio != null)
                {
                    if (msg.Chat.Id == fbaNews && msg.From.Id == channel)
                    {
                        try
                        {
                            var news = msg.Caption;
                            var audio = msg.Audio.FileId;

                            var myComment = Connector.Connector.GetNotifComment(msg.Chat.Id);
                            if (myComment == null)
                            {
                                Console.WriteLine("not found");
                            }
                            else
                                await client.SendTextMessageAsync(msg.Chat.Id, myComment, ParseMode.Html, replyToMessageId: msg.MessageId);

                            MySqlConnection MySql1 = new MySqlConnection(conn);
                            MySqlCommand sqlCommand1;
                            MySqlDataReader sqlReader1;

                            MySql1.Open();
                            var sql1 = $"SELECT chat_id FROM userbot WHERE is_get = 'on'";
                            sqlCommand1 = new MySqlCommand(sql1, MySql1);
                            sqlReader1 = sqlCommand1.ExecuteReader();
                            while (sqlReader1.Read())
                            {
                                try
                                {
                                    await client.SendAudioAsync(Convert.ToInt64(sqlReader1[0]), audio, $"<b>📬Говорит <a href=\"https://t.me/FBA_Studio\">FBA Studio</a>:</b>\n<i>{news}</i>", ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Посетить канал", "https://t.me/FBA_Studio")));
                                    Thread.Sleep(1500);
                                }
                                catch (ApiRequestException exception)
                                {
                                    Console.WriteLine(exception.Message);
                                    Connector.Connector.DeleteUserBot(Convert.ToInt64(sqlReader1[0]));
                                }
                            }
                        }
                        catch(Exception err)
                        {
                            Console.WriteLine(err);
                        }
                    }
                    else if (msg.From.Id == channel)
                    {
                        var myComment = Connector.Connector.GetNotifComment(msg.Chat.Id);
                        if (myComment == null)
                        {
                            return;
                        }
                        else
                            await client.SendTextMessageAsync(msg.Chat.Id, myComment, ParseMode.Html, replyToMessageId: msg.MessageId);
                    }
                    else
                    {
                        return;
                    }
                }

                if (msg.Sticker != null)
                {
                    if(msg.ReplyToMessage != null && msg.Chat.Id != msg.From.Id)
                        switch (msg.Sticker.Emoji)
                        {
                            case "👍":
                                {
                                    if (msg.ReplyToMessage.From.Id == msg.From.Id)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, а у вас, как я вижу, высокая самооценка😏", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    }
                                    else
                                    {
                                        Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                        if (Connector.Connector.message == "don't rate")
                                        {
                                            Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                            if (Connector.Connector.message == "not rated")
                                            {
                                                Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, 1);
                                                Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: 1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                            }
                                            else
                                            {
                                                Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                var rating = Connector.Connector.user_rating + 1;
                                                Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                            }
                                        }
                                        else
                                        {
                                            Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                            if (Connector.Connector.message == "not rated")
                                            {
                                                Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, 1);
                                                Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: 1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                            }
                                            else
                                            {
                                                Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                if (Connector.Connector.count_control >= 4)
                                                {
                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"🚫Вы уже исчерпали лимит оценивания <b>{msg.ReplyToMessage.From.FirstName}</b>\n⏱Дата сброса лимита: {Connector.Connector.soc_control}", ParseMode.Html);
                                                }
                                                else
                                                {
                                                    var control = Connector.Connector.count_control + 1;
                                                    Connector.Connector.UpdateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, control, DateTime.Now.AddDays(1));
                                                    var rating = Connector.Connector.user_rating + 1;
                                                    Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            case "👎":
                                {
                                    if (msg.ReplyToMessage.From.Id == msg.From.Id)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, зачем себя так унижать😕", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    }
                                    else
                                    {
                                        Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                        if (Connector.Connector.message == "don't rate")
                                        {
                                            Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                            if (Connector.Connector.message == "not rated")
                                            {
                                                Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, -1);
                                                Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: -1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                            }
                                            else
                                            {
                                                Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                var rating = Connector.Connector.user_rating - 1;
                                                Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                            }
                                        }
                                        else
                                        {
                                            Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                            if (Connector.Connector.message == "not rated")
                                            {
                                                Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, -1);
                                                Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: -1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                            }
                                            else
                                            {
                                                Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                if (Connector.Connector.count_control >= 4)
                                                {
                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"🚫Вы уже исчерпали лимит оценивания <b>{msg.ReplyToMessage.From.FirstName}</b>\n⏱Дата сброса лимита: {Connector.Connector.soc_control}", ParseMode.Html);
                                                }
                                                else
                                                {
                                                    var control = Connector.Connector.count_control + 1;
                                                    Connector.Connector.UpdateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, control, DateTime.Now.AddDays(1));
                                                    var rating = Connector.Connector.user_rating - 1;
                                                    Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                        }
                }

                if (msg.Document != null)
                {
                    if (msg.Chat.Id == fbaNews && msg.From.Id == channel)
                    {
                        try
                        {
                            var news = msg.Caption;
                            var document = msg.Document.FileId;

                            var myComment = Connector.Connector.GetNotifComment(msg.Chat.Id);
                            if (myComment == null)
                            {
                                Console.WriteLine("not found");
                            }
                            else
                                await client.SendTextMessageAsync(msg.Chat.Id, myComment, ParseMode.Html, replyToMessageId: msg.MessageId);

                            MySqlConnection MySql1 = new MySqlConnection(conn);
                            MySqlCommand sqlCommand1;
                            MySqlDataReader sqlReader1;

                            MySql1.Open();
                            var sql1 = $"SELECT chat_id FROM userbot WHERE is_get = 'on'";
                            sqlCommand1 = new MySqlCommand(sql1, MySql1);
                            sqlReader1 = sqlCommand1.ExecuteReader();
                            while (sqlReader1.Read())
                            {
                                try
                                {
                                    await client.SendDocumentAsync(Convert.ToInt64(sqlReader1[0]), document, $"<b>📬Говорит <a href=\"https://t.me/FBA_Studio\">FBA Studio</a>:</b>\n<i>{news}</i>", parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Посетить канал", "https://t.me/FBA_Studio")));
                                    Thread.Sleep(1500);
                                }
                                catch (ApiRequestException exception)
                                {
                                    Console.WriteLine(exception.Message);
                                    Connector.Connector.DeleteUserBot(Convert.ToInt64(sqlReader1[0]));
                                }
                            }
                        }
                        catch (Exception errrr)
                        {
                            Console.WriteLine(errrr);
                        }
                    }
                    else if (msg.From.Id == channel)
                    {
                        var myComment = Connector.Connector.GetNotifComment(msg.Chat.Id);
                        if (myComment == null)
                        {
                            return;
                        }
                        else
                            await client.SendTextMessageAsync(msg.Chat.Id, myComment, ParseMode.Html, replyToMessageId: msg.MessageId);
                    }
                    else if (msg.Chat.Id != msg.From.Id && msg.Document.MimeType == "video/mp4")
                    {
                        if (msg.From.Id == channel || msg.From.Id == group || msg.From.Id == anonim)
                        {
                            return;
                        }
                        Connector.Connector.GetAntiSpam(msg.From.Id, msg.Chat.Id);
                        if (Connector.Connector.message == "not found")
                        {
                            Connector.Connector.GetSpamConfig(msg.Chat.Id);
                            if (Connector.Connector.message == "not found")
                            {
                                return;
                            }
                            else
                            {
                                if (Connector.Connector.as_active == "off")
                                {
                                    return;
                                }
                                else
                                {
                                    Connector.Connector.CreateAntiSpam(msg.From.Id, msg.Chat.Id, 1);
                                }
                            }
                        }
                        else
                        {
                            Connector.Connector.GetSpamConfig(msg.Chat.Id);
                            if (Connector.Connector.message == "not found")
                            {
                                return;
                            }
                            else
                            {
                                if (Connector.Connector.as_active == "off")
                                {
                                    return;
                                }
                                else
                                {
                                    if (Connector.Connector.countMess >= Connector.Connector.maxMess)
                                    {
                                        ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                        if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                        {
                                            return;
                                        }
                                        else if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == msg.From.Id; }) != null)
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            Connector.Connector.DeleteAntiSpam(msg.Chat.Id, msg.From.Id);
                                            await client.RestrictChatMemberAsync(msg.Chat.Id, msg.From.Id, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false }, DateTime.Now.AddMinutes(30));
                                            await client.SendTextMessageAsync(msg.Chat.Id, $"<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> флудит, я заглушила его на 30 минут🔇", ParseMode.Html, disableWebPagePreview: true);
                                        }
                                    }
                                    else
                                    {
                                        var max = Connector.Connector.countMess + 1;
                                        Connector.Connector.UpdateAntiSpam(msg.Chat.Id, msg.From.Id, max);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("new document");
                        return;
                    }
                }

                if (msg.Animation != null)
                {
                    if(msg.Chat.Id != msg.From.Id)
                    {
                        if (msg.From.Id == channel || msg.From.Id == group || msg.From.Id == anonim)
                        {
                            return;
                        }
                        Connector.Connector.GetAntiSpam(msg.From.Id, msg.Chat.Id);
                        if (Connector.Connector.message == "not found")
                        {
                            Connector.Connector.GetSpamConfig(msg.Chat.Id);
                            if (Connector.Connector.message == "not found")
                            {
                                return;
                            }
                            else
                            {
                                if (Connector.Connector.as_active == "off")
                                {
                                    return;
                                }
                                else
                                {
                                    Connector.Connector.CreateAntiSpam(msg.From.Id, msg.Chat.Id, 1);
                                }
                            }
                        }
                        else
                        {
                            Connector.Connector.GetSpamConfig(msg.Chat.Id);
                            if (Connector.Connector.message == "not found")
                            {
                                return;
                            }
                            else
                            {
                                if (Connector.Connector.as_active == "off")
                                {
                                    return;
                                }
                                else
                                {
                                    if (Connector.Connector.countMess >= Connector.Connector.maxMess)
                                    {
                                        ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                        if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                        {
                                            return;
                                        }
                                        else if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == msg.From.Id; }) != null)
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            Connector.Connector.DeleteAntiSpam(msg.Chat.Id, msg.From.Id);
                                            await client.RestrictChatMemberAsync(msg.Chat.Id, msg.From.Id, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false }, DateTime.Now.AddMinutes(30));
                                            await client.SendTextMessageAsync(msg.Chat.Id, $"<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> флудит, я заглушила его на 30 минут🔇", ParseMode.Html, disableWebPagePreview: true);
                                        }
                                    }
                                    else
                                    {
                                        var max = Connector.Connector.countMess + 1;
                                        Connector.Connector.UpdateAntiSpam(msg.Chat.Id, msg.From.Id, max);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                if (msg.Text != null)
                {
                    try
                    {
                        MySqlConnection MySql = new MySqlConnection(conn);
                        MySqlCommand sqlCommand;
                        MySqlDataReader sqlReader;

                        MySql.Open();
                        var sql = $"SELECT * FROM userbot WHERE chat_id = '{msg.Chat.Id}'";
                        sqlCommand = new MySqlCommand(sql, MySql);
                        sqlReader = sqlCommand.ExecuteReader();
                        while (sqlReader.Read())
                        {
                            chat_id = sqlReader[1].ToString();
                        }
                        MySql.Close();

                        if(chat_id == null)
                        {
                            MySql = new MySqlConnection(conn);
                            MySql.Open();
                            var sqlIns = $"INSERT INTO userbot (id, chat_id, is_get) VALUES ('0', '{msg.Chat.Id}', 'on')";
                            sqlCommand = new MySqlCommand(sqlIns, MySql);
                            sqlCommand.ExecuteNonQuery();
                            MySql.Close();
                        }

                        chat_id = null;

                        Console.WriteLine($"{DateTime.Now} New Message from ID{msg.Chat.Id}, it's message: {msg.Text}");

                        if (msg.Chat.Id == fbaNews && msg.From.Id == channel)
                        {
                            try
                            {
                                var news = msg.Text;
                                var myComment = Connector.Connector.GetNotifComment(msg.Chat.Id);
                                if (myComment == null)
                                {
                                    Console.WriteLine("not found");
                                }
                                else
                                    await client.SendTextMessageAsync(msg.Chat.Id, myComment, ParseMode.Html, replyToMessageId: msg.MessageId);

                                MySqlConnection MySql1 = new MySqlConnection(conn);
                                MySqlCommand sqlCommand1;
                                MySqlDataReader sqlReader1;

                                MySql1.Open();
                                var sql1 = "SELECT chat_id FROM userbot WHERE is_get = 'on'";
                                sqlCommand1 = new MySqlCommand(sql1, MySql1);
                                sqlReader1 = sqlCommand1.ExecuteReader();
                                while (sqlReader1.Read())
                                {
                                    try
                                    {
                                        await client.SendTextMessageAsync(Convert.ToInt64(sqlReader1[0]), $"<b>📬Говорит <a href=\"https://t.me/FBA_Studio\">FBA Studio</a>:</b>\n<i>{news}</i>", ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Посетить канал", "https://t.me/FBA_Studio")), disableWebPagePreview: true);
                                        Thread.Sleep(1500);
                                    }
                                    catch(ApiRequestException exception)
                                    {
                                        Console.WriteLine(exception.Message);
                                        Connector.Connector.DeleteUserBot(Convert.ToInt64(sqlReader1[0]));
                                    }
                                }
                            }
                            catch(Exception err)
                            {
                                Console.WriteLine(err);
                            }
                        }
                        else if(msg.From.Id == channel)
                        {
                            var myComment = Connector.Connector.GetNotifComment(msg.Chat.Id);
                            if (myComment == null)
                            {
                                return;
                            }
                            else
                                await client.SendTextMessageAsync(msg.Chat.Id, myComment, ParseMode.Html, replyToMessageId: msg.MessageId);
                        }
                        else
                        {
                            Connector.Connector.GetNoBadLyrics(msg.Chat.Id);
                            if (Connector.Connector.message == "not found" && msg.Chat.Id != msg.From.Id)
                            {
                                Connector.Connector.RegisterNoBadLyrics(msg.Chat.Id, "off");
                            }
                            if (Connector.Connector.isActive == "on")
                            {
                                if (MessageParser.ScanOnBadWords(msg.Text.ToLower()) == false)
                                {
                                    Console.WriteLine("No bad words");
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "😶");
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == msg.From.Id; }) != null)
                                    {
                                        await client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                        await client.SendTextMessageAsync(msg.Chat.Id, $"<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a>, вы как себе позволяете использовать нецензурную лексику😕?", ParseMode.Html, disableWebPagePreview: true);
                                    }
                                    else
                                    {
                                        await client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                        await client.RestrictChatMemberAsync(msg.Chat.Id, msg.From.Id, untilDate: DateTime.Now.AddMinutes(30), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(msg.Chat.Id, $"<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> получает мут 30 минут за употребление нецензурной лексики🗣!");
                                    }
                                }

                            }

                            //start command & help command
                            if (msg.Text.StartsWith("/start"))
                            {
                                if (msg.Chat.Id != msg.From.Id)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Здравствуйте, я чат-менеджер бот Лаура!\nНапишите 👉/help, чтобы получить список команд.");
                                    BotCommand[] botCommands = { new() { Command = "start", Description = "Обновить список команд" }, new() { Command = "help", Description = "Получить справочник по использованию бота" }, new() { Command = "getchatid", Description = "Получить ID данного чата" }, new() { Command = "nightmode", Description = "Включить ночной режим в чате" }, new() { Command = "statemode", Description = "Вернуть чат в штатный режим" }, new() { Command = "getourid", Description = "Получить ID участника чата" }, new() { Command = "warn", Description = "Кинуть предупреждение участнику" }, new() { Command = "setmaxwarns", Description = "Изменить максимальное количество предупреждений" }, new() { Command = "setpunish", Description = "Сменить наказание" }, new() { Command = "setlyriccontrol", Description = "Настройка контроля общения" }, new() { Command = "uptime", Description = "Узнать аптайм бота" } };
                                    await client.SetMyCommandsAsync(botCommands);
                                    await client.GetMyCommandsAsync();
                                    await client.SetMyCommandsAsync(botCommands);
                                    Console.WriteLine($"Bot was started in chat: ID{msg.Chat.Id}");
                                    return;
                                }
                                else
                                {
                                    BotCommand[] botCommands = { new() { Command = "start", Description = "Обновить список команд" }, new() { Command = "getmyid", Description = "Получить свой личный ID" }, new() { Command = "uptime", Description = "Узнать аптайм бота" }, new() { Command = "sub_weather", Description = "Подписаться на рассылку прогноза погоды" }, new() { Command = "unsub_weather", Description = "Отписаться от рассылки прогноза погоды" } };
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Здравствуйте, я чат-менеджер бот Лаура!\nНажмите на кнопку👉«Инструкция📚», чтобы получить список команд.", replyMarkup: GetButtons());
                                    await client.SetMyCommandsAsync(botCommands);
                                    await client.GetMyCommandsAsync();
                                    await client.SetMyCommandsAsync(botCommands);
                                    Console.WriteLine($"Bot was started in chat: ID{msg.Chat.Id}");
                                    return;
                                }
                            }

                            if (msg.Text.StartsWith("/help"))
                            {
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Инструкция для работы с телеграм ботом:\ntelegra.ph/Rabota-s-botom-Laura-10-18", parseMode: ParseMode.Html, disableWebPagePreview: false);
                                return;
                            }

                            if (msg.Text.ToLower() == "мой рейтинг")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    Connector.Connector.GetSocialData(msg.From.Id);
                                    if (Connector.Connector.message == "not rated")
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, $"<b>Ваш рейтинг📊</b>: 0", ParseMode.Html);
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, $"<b>Ваш рейтинг📊</b>: {Connector.Connector.user_rating}", ParseMode.Html);
                                    }
                                }
                            }

                            //Admins Commands
                            if (msg.Text.ToUpper() == "БАН")
                            {
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
                                        ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                        bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                        var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                        var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                        if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == 5565507778; }) == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            return;
                                        }
                                        else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            return;
                                        }
                                        else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            return;

                                        }
                                        else
                                        {
                                            await client.KickChatMemberAsync(userId: msg.ReplyToMessage.From.Id, chatId: msg.Chat.Id, cancellationToken: default);
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❌Участник <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был забанен модератором:<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            return;
                                        }
                                    }
                                }
                            }

                            if (msg.Text.ToLower().StartsWith("бан\n"))
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
                                    InMsg = msg.Text.Split(msg.Text.Split('\n')[0] + '\n')[1];
                                    var BanText = InMsg;

                                    Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                    var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                    var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == 5565507778; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> является администратором чата «{msg.Chat.Title}»");
                                        return;
                                    }
                                    else
                                        await client.KickChatMemberAsync(userId: msg.ReplyToMessage.From.Id, chatId: msg.Chat.Id, cancellationToken: default);
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❌Участник <a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> был забанен модератором: <a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a>!\nПричина: <i>{BanText}</i>", parseMode: ParseMode.Html);
                                }
                            }

                            if (msg.Text.ToLower().StartsWith("бан @"))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    var username = msg.Text.Split('@')[1];
                                    if (username.EndsWith(' '))
                                        username = username.Split(' ')[0];
                                    var target = await MessageParser.GetUserIdByUsernameAsync(username, token);
                                    if (target == null)
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "⚠️Возникла ошибка Telegram API для проверки пользователя по Юзернейму, повторите попытку позже!");
                                        return;
                                    }
                                    var targetId = Convert.ToInt64(target[0]);
                                    var firstName = target[1].ToString();

                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == targetId; }) != null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://user?id={targetId}\">{firstName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;

                                    }
                                    else
                                    {
                                        await client.KickChatMemberAsync(userId: targetId, chatId: msg.Chat.Id, cancellationToken: default);
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❌Участник <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был забанен модератором:<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                }
                            }

                            if (msg.Text.ToUpper() == "РАЗБАН")
                            {
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
                                        Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                        bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                        var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                        var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                        if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == 5565507778; }) == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            return;
                                        }
                                        else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            return;
                                        }
                                        else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            return;
                                        }
                                        else if (MemberTarget.Status != Banned)
                                        {
                                            if (msg.ReplyToMessage.From.Username == null)
                                            {
                                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> не находиться в чёрном списке😕!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            await client.UnbanChatMemberAsync(userId: msg.ReplyToMessage.From.Id, chatId: msg.Chat.Id, cancellationToken: default);
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был разбанен модератором <a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>!\nТеперь его можно обратно вернуть в чат🤗.", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            return;
                                        }
                                    }
                                    return;
                                }
                            }

                            if (msg.Text.ToUpper() == "РАЗМУТ")
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
                                    Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    bool IsAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                    var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                    var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == 5565507778; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (MemberTarget.Status != ChatMemberStatus.Restricted)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> не был заглушен😐!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else
                                    {
                                        await client.PromoteChatMemberAsync(userId: msg.ReplyToMessage.From.Id, chatId: msg.Chat.Id, cancellationToken: default);
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"✅Участник <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> теперь может общаться!\n\nТолько лучше следить за своим языком и не разжигать ссоры в чате😊.\nМодератор: <a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                }
                                return;
                            }

                            if (msg.Text.ToUpper() == "МУТ")
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
                                    Telegram.Bot.Types.ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    bool isAdmin = admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) != null;
                                    var MemberOutput = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.From.Id);
                                    var MemberTarget = await client.GetChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == 5565507778; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> является администратором чата «{msg.Chat.Title}»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (MemberTarget.Status == ChatMemberStatus.Restricted)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> уже был заглушен📛!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else
                                    {
                                        await client.RestrictChatMemberAsync(chatId: msg.Chat.Id, userId: msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(15), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a> был заглушен на 15 минут!\nМодератор: <a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                }
                                return;
                            }

                            if (msg.Text.ToLower().StartsWith("мут "))
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
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!");
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

                            if (msg.Text.StartsWith("/nightmode"))
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
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == 5565507778; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SetChatPermissionsAsync(chatId: msg.Chat.Id, permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> объявляет ночной режим🤫!", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                }
                                return;
                            }

                            if (msg.Text.StartsWith("/statemode"))
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
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == 5565507778; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SetChatPermissionsAsync(chatId: msg.Chat.Id, permissions: new ChatPermissions { CanSendMessages = true, CanSendMediaMessages = true, CanSendOtherMessages = true, CanAddWebPagePreviews = true });
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> возвращает чат в штатный режим✅", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;

                                    }
                                }
                                return;
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
                                    if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href=\"tg://user?id={msg.From.Id}\"></a>, вы не являетесь администратором/создателем чата!");
                                        return;
                                    }
                                    else
                                        await client.DeleteMessageAsync(messageId: msg.ReplyToMessage.MessageId, chatId: msg.Chat.Id, cancellationToken: default);
                                    await client.DeleteMessageAsync(messageId: msg.MessageId, chatId: msg.Chat.Id);
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Сообщение было успешно удалено✅", parseMode: ParseMode.Html);
                                }
                            }

                            if (msg.Text.StartsWith("/warn") | msg.Text.StartsWith("/warn@Laura_cm_bot"))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, "⛔️Вы не указали пользователя!");
                                }
                                else if (msg.ReplyToMessage.From.IsBot)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"@{msg.ReplyToMessage.From.Username} является ботом🤖!");
                                    return;
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "Я не являюсь администратором чата🙅🏻‍♀️!");
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a>, вы не являетесь администратором/создателем чата!", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == msg.ReplyToMessage.From.Id; }) != null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"❗<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> является администратором чата «{msg.Chat.Title}»", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    }
                                    else
                                    {
                                        if (msg.Text.Contains(' '))
                                        {
                                            var reason = msg.Text.Split(msg.Text.Split(' ')[0] + " ")[1];
                                            Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                            if (Connector.Connector.message == "not found")
                                            {
                                                Connector.Connector.RegisterChatWarns(msg.Chat.Id, 3, "mute");
                                                Connector.Connector.CheckUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                if (Connector.Connector.message == "not found")
                                                {
                                                    var dateNow = DateTime.Now;
                                                    var date = DateTime.Now.AddDays(3);

                                                    Connector.Connector.RegisterUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, 1, date);
                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                    var maxWarns = Connector.Connector.maxWarns;

                                                    if (reason == null)
                                                    {
                                                        Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                    }
                                                    else if (reason != null)
                                                    {
                                                        Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n💬Причина: <i>{reason}</i>\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                    }
                                                }
                                                else if (Connector.Connector.message == "has warn data")
                                                {
                                                    var dateNow = DateTime.Now;
                                                    var date = DateTime.Now.AddDays(3);

                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                    Connector.Connector.CheckUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                    Connector.Connector.GetDeadLine(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                    Connector.Connector.GetPunishment(msg.Chat.Id);

                                                    var maxWarns = Connector.Connector.maxWarns;
                                                    var userWarns = Connector.Connector.UserWarns;
                                                    var punish = Connector.Connector.punishment;
                                                    var deadLine = Connector.Connector.deadline;

                                                    var newUserWarns = userWarns + 1;

                                                    if (maxWarns <= newUserWarns)
                                                    {
                                                        if (deadLine <= dateNow)
                                                        {
                                                            Connector.Connector.UpdateUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, 1, dateNow, date);
                                                            if (Connector.Connector.message == "deadline lost")
                                                            {
                                                                if (reason == null)
                                                                {
                                                                    Connector.Connector.DeleteReasonsOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                                    Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                                }
                                                                else if (reason != null)
                                                                {
                                                                    Connector.Connector.DeleteReasonsOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                                    Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n💬Причина: <i>{reason}</i>\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                                }
                                                            }
                                                            else if (Connector.Connector.message == "deadline updated")
                                                            {
                                                                Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                                await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                            }
                                                        }
                                                        else if (punish == "mute")
                                                        {
                                                            await client.RestrictChatMemberAsync(msg.Chat.Id, msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(45), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"⛔️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> превысил лимит равному {maxWarns}!\nТеперь он заглушен на 45 минут🔇!", ParseMode.Html);
                                                            Connector.Connector.DeleteWarnUser(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                        }
                                                        else if (punish == "ban")
                                                        {
                                                            await client.KickChatMemberAsync(msg.Chat.Id, msg.ReplyToMessage.From.Id);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"⛔️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> превысил лимит равному {maxWarns}!\nТеперь он исключён из чата🙅🏻‍♀️!", ParseMode.Html);
                                                            Connector.Connector.DeleteWarnUser(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                        }
                                                    }
                                                    else if (maxWarns > newUserWarns)
                                                    {
                                                        Connector.Connector.UpdateUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, newUserWarns, dateNow, date);
                                                        if (reason == null)
                                                        {
                                                            Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {newUserWarns}/{maxWarns}!\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                        }
                                                        else if (reason != null)
                                                        {
                                                            Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {newUserWarns}/{maxWarns}!\n💬Причина: <i>{reason}</i>\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                        }
                                                    }

                                                }
                                            }
                                            else if (Connector.Connector.message == "has max warns")
                                            {
                                                Connector.Connector.CheckUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                if (Connector.Connector.message == "not found")
                                                {
                                                    var dateNow = DateTime.Now;
                                                    var date = DateTime.Now.AddDays(3);
                                                    Connector.Connector.RegisterUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, 1, date);
                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                    Connector.Connector.CheckUserWarns(msg.Chat.Id, msg.ReplyToMessage.From.Id);
                                                    Connector.Connector.GetPunishment(msg.Chat.Id);

                                                    var maxWarns = Connector.Connector.maxWarns;
                                                    if (reason == null)
                                                    {
                                                        Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                    }
                                                    else if (reason != null)
                                                    {
                                                        Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n💬Причина: <i>{reason}</i>\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                    }
                                                }
                                                else if (Connector.Connector.message == "has warn data")
                                                {
                                                    var dateNow = DateTime.Now;
                                                    var date = DateTime.Now.AddDays(3);

                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                    Connector.Connector.CheckUserWarns(msg.Chat.Id, msg.From.Id);
                                                    Connector.Connector.GetPunishment(msg.Chat.Id);
                                                    Connector.Connector.GetDeadLine(msg.ReplyToMessage.From.Id, msg.Chat.Id);

                                                    var maxWarns = Connector.Connector.maxWarns;
                                                    var userWarns = Connector.Connector.UserWarns;
                                                    var punish = Connector.Connector.punishment;
                                                    var deadLine = Connector.Connector.deadline;

                                                    var newUserWarns = userWarns + 1;
                                                    if (maxWarns <= newUserWarns)
                                                    {
                                                        if (deadLine <= dateNow)
                                                        {
                                                            Connector.Connector.UpdateUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, 1, dateNow, date);
                                                            if (Connector.Connector.message == "deadline lost")
                                                            {
                                                                if (reason == null)
                                                                {
                                                                    Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                                    Connector.Connector.DeleteReasonsOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                                }
                                                                else if (reason != null)
                                                                {
                                                                    Connector.Connector.DeleteReasonsOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                                    Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                                }
                                                            }
                                                            else if (Connector.Connector.message == "deadline updated")
                                                            {
                                                                Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                                await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                            }
                                                        }
                                                        else if (punish == "mute")
                                                        {
                                                            await client.RestrictChatMemberAsync(msg.Chat.Id, msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(45), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"⛔️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> превысил лимит равному {maxWarns}!\nТеперь он заглушен на 45 минут🔇!", ParseMode.Html);
                                                            Connector.Connector.DeleteWarnUser(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                        }
                                                        else if (punish == "ban")
                                                        {
                                                            await client.KickChatMemberAsync(msg.Chat.Id, msg.ReplyToMessage.From.Id);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"⛔️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> превысил лимит равному {maxWarns}!\nТеперь он исключён из чата🙅🏻‍♀️!", ParseMode.Html);
                                                            Connector.Connector.DeleteWarnUser(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                        }
                                                    }
                                                    else if (maxWarns > newUserWarns)
                                                    {
                                                        Connector.Connector.UpdateUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, newUserWarns, dateNow, date);
                                                        if (reason == null)
                                                        {
                                                            Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {newUserWarns}/{maxWarns}!\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                        }
                                                        else if (reason != null)
                                                        {
                                                            Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {newUserWarns}/{maxWarns}!\n💬Причина: <i>{reason}</i>\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                        }

                                                    }

                                                }

                                            }

                                        }
                                        else
                                        {
                                            string reason = null;
                                            Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                            if (Connector.Connector.message == "not found")
                                            {
                                                Connector.Connector.RegisterChatWarns(msg.Chat.Id, 3, "mute");
                                                Connector.Connector.CheckUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                if (Connector.Connector.message == "not found")
                                                {
                                                    var dateNow = DateTime.Now;
                                                    var date = DateTime.Now.AddDays(3);

                                                    Connector.Connector.RegisterUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, 1, date);
                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                    var maxWarns = Connector.Connector.maxWarns;
                                                    if (reason == null)
                                                    {
                                                        Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                    }
                                                    else if (reason != null)
                                                    {
                                                        Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n💬Причина: <i>{reason}</i>\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                    }
                                                }
                                                else if (Connector.Connector.message == "has warn data")
                                                {
                                                    var dateNow = DateTime.Now;
                                                    var date = DateTime.Now.AddDays(3);

                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                    Connector.Connector.CheckUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                    Connector.Connector.GetDeadLine(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                    Connector.Connector.GetPunishment(msg.Chat.Id);

                                                    var maxWarns = Connector.Connector.maxWarns;
                                                    var userWarns = Connector.Connector.UserWarns;
                                                    var punish = Connector.Connector.punishment;
                                                    var deadLine = Connector.Connector.deadline;

                                                    var newUserWarns = userWarns + 1;

                                                    if (maxWarns <= newUserWarns)
                                                    {
                                                        if (deadLine <= dateNow)
                                                        {
                                                            Connector.Connector.UpdateUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, 1, dateNow, date);
                                                            if (Connector.Connector.message == "deadline lost")
                                                            {
                                                                if (reason == null)
                                                                {
                                                                    Connector.Connector.DeleteReasonsOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                                    Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                                }
                                                                else if (reason != null)
                                                                {
                                                                    Connector.Connector.DeleteReasonsOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                                    Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n💬Причина: <i>{reason}</i>\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                                }
                                                            }
                                                            else if (Connector.Connector.message == "deadline updated")
                                                            {
                                                                Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                                await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                            }
                                                        }
                                                        else if (punish == "mute")
                                                        {
                                                            await client.RestrictChatMemberAsync(msg.Chat.Id, msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(45), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"⛔️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> превысил лимит равному {maxWarns}!\nТеперь он заглушен на 45 минут🔇!", ParseMode.Html);
                                                            Connector.Connector.DeleteWarnUser(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                        }
                                                        else if (punish == "ban")
                                                        {
                                                            await client.KickChatMemberAsync(msg.Chat.Id, msg.ReplyToMessage.From.Id);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"⛔️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> превысил лимит равному {maxWarns}!\nТеперь он исключён из чата🙅🏻‍♀️!", ParseMode.Html);
                                                            Connector.Connector.DeleteWarnUser(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                        }
                                                    }
                                                    else if (maxWarns > newUserWarns)
                                                    {
                                                        if (reason == null)
                                                        {
                                                            Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                        }
                                                        else if (reason != null)
                                                        {
                                                            Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n💬Причина: <i>{reason}</i>\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                        }
                                                    }

                                                }
                                            }
                                            else if (Connector.Connector.message == "has max warns")
                                            {
                                                Connector.Connector.CheckUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                if (Connector.Connector.message == "not found")
                                                {
                                                    var dateNow = DateTime.Now;
                                                    var date = DateTime.Now.AddDays(3);
                                                    Connector.Connector.RegisterUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, 1, date);
                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                    Connector.Connector.CheckUserWarns(msg.Chat.Id, msg.ReplyToMessage.From.Id);
                                                    Connector.Connector.GetPunishment(msg.Chat.Id);

                                                    var maxWarns = Connector.Connector.maxWarns;
                                                    if (reason == null)
                                                    {
                                                        Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                    }
                                                    else if (reason != null)
                                                    {
                                                        Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n💬Причина: <i>{reason}</i>\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                    }
                                                }
                                                else if (Connector.Connector.message == "has warn data")
                                                {
                                                    var dateNow = DateTime.Now;
                                                    var date = DateTime.Now.AddDays(3);

                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                    Connector.Connector.CheckUserWarns(msg.Chat.Id, msg.From.Id);
                                                    Connector.Connector.GetPunishment(msg.Chat.Id);
                                                    Connector.Connector.GetDeadLine(msg.ReplyToMessage.From.Id, msg.Chat.Id);

                                                    var maxWarns = Connector.Connector.maxWarns;
                                                    var userWarns = Connector.Connector.UserWarns;
                                                    var punish = Connector.Connector.punishment;
                                                    var deadLine = Connector.Connector.deadline;

                                                    var newUserWarns = userWarns + 1;
                                                    if (maxWarns <= newUserWarns)
                                                    {
                                                        if (deadLine <= dateNow)
                                                        {
                                                            Connector.Connector.UpdateUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, 1, dateNow, date);
                                                            if (Connector.Connector.message == "deadline lost")
                                                            {
                                                                if (reason == null)
                                                                {
                                                                    Connector.Connector.DeleteReasonsOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                                    Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                                }
                                                                else if (reason != null)
                                                                {
                                                                    Connector.Connector.DeleteReasonsOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                                    Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                                }
                                                            }
                                                            else if (Connector.Connector.message == "deadline updated")
                                                            {
                                                                Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                                await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение {1}/{maxWarns}!\n<code>Сброс предупреждений в {Connector.Connector.deadline}</code>", ParseMode.Html);
                                                            }
                                                        }
                                                        else if (punish == "mute")
                                                        {
                                                            await client.RestrictChatMemberAsync(msg.Chat.Id, msg.ReplyToMessage.From.Id, untilDate: DateTime.Now.AddMinutes(45), permissions: new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false });
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"⛔️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> превысил лимит равному {maxWarns}!\nТеперь он заглушен на 45 минут🔇!", ParseMode.Html);
                                                            Connector.Connector.DeleteWarnUser(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                        }
                                                        else if (punish == "ban")
                                                        {
                                                            await client.KickChatMemberAsync(msg.Chat.Id, msg.ReplyToMessage.From.Id);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"⛔️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> превысил лимит равному {maxWarns}!\nТеперь он исключён из чата🙅🏻‍♀️!", ParseMode.Html);
                                                            Connector.Connector.DeleteWarnUser(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                                        }
                                                    }
                                                    else if (maxWarns > newUserWarns)
                                                    {
                                                        if (reason == null)
                                                        {
                                                            Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, "without", DateTime.Now);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                        }
                                                        else if (reason != null)
                                                        {
                                                            Connector.Connector.CreateReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id, reason, DateTime.Now);
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"❗️<a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> получил предупреждение 1/{maxWarns}!\n💬Причина: <i>{reason}</i>\n<code>Сброс предупреждений в {date}</code>", ParseMode.Html);
                                                        }

                                                    }

                                                }

                                            }

                                        }
                                    }
                                }
                            }

                            if (msg.Text.StartsWith("/setmaxwarns ") | msg.Text.StartsWith("/setmaxwarns@Laura_cm_bot "))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            var newMaxWarns = Convert.ToInt32(msg.Text.Split(' ')[1]);
                                            if (newMaxWarns == null)
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, "⚠️Вы не указали число!");
                                            }
                                            else if (newMaxWarns > 20)
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, "⚠️Вы не можете указать число предупреждений, превышающее 20!");
                                            }
                                            else if (newMaxWarns < 3)
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, "⚠️Вы не можете указать меньше 3 предупреждений!");
                                            }
                                            else
                                            {
                                                Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                if (Connector.Connector.message == "not found")
                                                {
                                                    Connector.Connector.RegisterChatWarns(msg.Chat.Id, 3, "mute");
                                                    Connector.Connector.GetPunishment(msg.Chat.Id);
                                                    var punish = Connector.Connector.punishment;

                                                    Connector.Connector.UpdateMaxWarns(msg.Chat.Id, newMaxWarns, punish);
                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"✅Вы успешно изменили количество предупреждений!\nНовый максимум предупреждений: {newMaxWarns}");
                                                }
                                                else if (Connector.Connector.message == "has max warns")
                                                {
                                                    Connector.Connector.GetPunishment(msg.Chat.Id);
                                                    var punish = Connector.Connector.punishment;

                                                    Connector.Connector.UpdateMaxWarns(msg.Chat.Id, newMaxWarns, punish);
                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"✅Вы успешно изменили количество предупреждений!\nНовый максимум предупреждений: {newMaxWarns}");
                                                }
                                            }
                                        }
                                        catch (FormatException)
                                        {
                                            await client.SendTextMessageAsync(msg.Chat.Id, "⚠️Вы не указали число!");
                                        }
                                    }
                                }
                            }

                            if (msg.Text.StartsWith("/setpunish ") | msg.Text.StartsWith("/setpunish@Laura_cm_bot "))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            var newPunish = msg.Text.Split(' ')[1].ToLower();
                                            if (newPunish == null)
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, "⚠️Вы не указали тип наказания");
                                            }
                                            else
                                            {
                                                Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                if (Connector.Connector.message == "not found")
                                                {
                                                    Connector.Connector.RegisterChatWarns(msg.Chat.Id, 3, "mute");
                                                    var maxWarns = Connector.Connector.maxWarns;

                                                    if (newPunish == "mute")
                                                    {
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"✅Вы успешно изменили наказание!\nТип наказания на данный момент: мут");
                                                        Connector.Connector.UpdateMaxWarns(msg.Chat.Id, maxWarns, newPunish);
                                                    }
                                                    else if (newPunish == "ban")
                                                    {
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"✅Вы успешно изменили наказание!\nТип наказания на данный момент: бан");
                                                        Connector.Connector.UpdateMaxWarns(msg.Chat.Id, maxWarns, newPunish);
                                                    }
                                                    else
                                                    {
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"⚠️Тип \"{newPunish}\" не существует!");
                                                    }
                                                }
                                                else if (Connector.Connector.message == "has max warns")
                                                {
                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                    var maxWarns = Connector.Connector.maxWarns;

                                                    if (newPunish == "mute")
                                                    {
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"✅Вы успешно изменили наказание!\nТип наказания на данный момент: мут");
                                                        Connector.Connector.UpdateMaxWarns(msg.Chat.Id, maxWarns, newPunish);
                                                    }
                                                    else if (newPunish == "ban")
                                                    {
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"✅Вы успешно изменили наказание!\nТип наказания на данный момент: бан");
                                                        Connector.Connector.UpdateMaxWarns(msg.Chat.Id, maxWarns, newPunish);
                                                    }
                                                    else
                                                    {
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"⚠️Тип \"{newPunish}\" не существует!");
                                                    }
                                                }
                                            }
                                        }
                                        catch (FormatException)
                                        {
                                            await client.SendTextMessageAsync(msg.Chat.Id, "⚠️Вы не указали тип данных!");
                                        }
                                    }
                                }
                            }

                            if (msg.Text.ToLower() == "варны")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    var reasons = "<b>Список причин:</b>\n";
                                    Connector.Connector.GetReasonOfWarns(msg.From.Id, msg.Chat.Id);
                                    if (Connector.Connector.message == "not found")
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "У вас нет ни причины предупреждений🎉!");
                                    }
                                    else if (Connector.Connector.message == "has reasons")
                                    {
                                        var part2 = "";
                                        int i = 1;
                                        for (int k = 0; k < Connector.Connector.reasons.Count; k++)
                                        {
                                            if (Connector.Connector.reasons[k] == "without")
                                            {
                                                part2 += i + ". " + "Причина не была указана. " + "Дата получения: " + Connector.Connector.time_stamp[k] + "\n";
                                                i++;
                                            }
                                            else
                                            {
                                                part2 += i + ". Причина:\n- " + Connector.Connector.reasons[k] + ". Дата получения: " + Connector.Connector.time_stamp[k] + "\n";
                                                i++;
                                            }
                                        }
                                        Connector.Connector.CheckUserWarns(msg.From.Id, msg.Chat.Id);
                                        Connector.Connector.GetDeadLine(msg.From.Id, msg.Chat.Id);
                                        Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                        var textPart1 = $"<b>📬Количество предупреждений <a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a>: {Connector.Connector.UserWarns}/{Connector.Connector.maxWarns}</b>\n{reasons}";
                                        var finalText = $"{textPart1}\n<i>{part2}</i>\n⏱<code>Сброс предупреждений в {Connector.Connector.deadline}</code>";
                                        await client.SendTextMessageAsync(msg.Chat.Id, finalText, ParseMode.Html, disableWebPagePreview: true);
                                    }
                                }
                                else if (msg.ReplyToMessage.From.IsBot)
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, $"@{msg.ReplyToMessage.From.FirstName} является ботом🤖");
                                }
                                else
                                {
                                    var reasons = "<b>Список причин:</b>\n";
                                    Connector.Connector.GetReasonOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                    if (Connector.Connector.message == "not found")
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, $"У <a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a> нет ни причины предупреждений🎉!", ParseMode.Html, disableWebPagePreview: true);
                                    }
                                    else if (Connector.Connector.message == "has reasons")
                                    {
                                        var part2 = "";
                                        int i = 1;
                                        for (int k = 0; k < Connector.Connector.reasons.Count; k++)
                                        {
                                            if (Connector.Connector.reasons[k] == "without")
                                            {
                                                part2 += i + ". " + "Причина не была указана. " + "Дата получения: " + Connector.Connector.time_stamp[k] + "\n";
                                                i++;
                                            }
                                            else
                                            {
                                                part2 += i + ". Причина:\n- " + Connector.Connector.reasons[k] + ". Дата получения: " + Connector.Connector.time_stamp[k] + "\n";
                                                i++;
                                            }
                                        }
                                        Connector.Connector.CheckUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                        Connector.Connector.GetDeadLine(msg.ReplyToMessage.From.Id, msg.Chat.Id); ;
                                        Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                        var textPart1 = $"<b>📬Количество предупреждений <a href=\"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>: {Connector.Connector.UserWarns}/{Connector.Connector.maxWarns}</b>\n{reasons}";
                                        var finalText = $"{textPart1}\n<i>{part2}</i>\n⏱<code>Сброс предупреждений в {Connector.Connector.deadline}</code>";
                                        await client.SendTextMessageAsync(msg.Chat.Id, finalText, ParseMode.Html, disableWebPagePreview: true);
                                    }
                                }
                            }

                            if (msg.Text.ToLower().StartsWith("/setlyriccontrol ") | msg.Text.ToLower().StartsWith("/setlyriccontrol@laura_cm_bot "))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    var text = msg.Text.ToLower();
                                    var isActiveController = text.Split(' ')[1];
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "Я не являюсь администратором чата🙅🏻‍♀️!");
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "⛔️Вы не являетесь администратором!");
                                    }
                                    else
                                    {
                                        Connector.Connector.GetNoBadLyrics(msg.Chat.Id);
                                        if (Connector.Connector.message == "has settings")
                                        {
                                            if (isActiveController == "off")
                                            {
                                                Connector.Connector.SetNewParametrBL(msg.Chat.Id, isActiveController);
                                                await client.SendTextMessageAsync(msg.Chat.Id, $"<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> отключил слежку за общением🙅🏻‍♀️!\nБудьте аккуратны в общении.", ParseMode.Html, disableWebPagePreview: true);
                                            }
                                            else if (isActiveController == "on")
                                            {
                                                Connector.Connector.SetNewParametrBL(msg.Chat.Id, isActiveController);
                                                await client.SendTextMessageAsync(msg.Chat.Id, $"<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> включил слежку за общением в этом чате✅!", ParseMode.Html, disableWebPagePreview: true);
                                            }
                                            else
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, $"Параметр контроля \"{isActiveController}\" не найден⚠️");
                                            }
                                        }
                                        else
                                        {
                                            if (isActiveController == "off")
                                            {
                                                Connector.Connector.RegisterNoBadLyrics(msg.Chat.Id, isActiveController);
                                                await client.SendTextMessageAsync(msg.Chat.Id, $"<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> отключил слежку за общением🙅🏻‍♀️!\nБудьте аккуратны в общении.", ParseMode.Html, disableWebPagePreview: true);
                                            }
                                            else if (isActiveController == "on")
                                            {
                                                Connector.Connector.RegisterNoBadLyrics(msg.Chat.Id, isActiveController);
                                                await client.SendTextMessageAsync(msg.Chat.Id, $"<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> включил слежку за общением в этом чате✅!", ParseMode.Html, disableWebPagePreview: true);
                                            }
                                            else
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, $"Параметр контроля \"{isActiveController}\" не найден⚠️");
                                            }
                                        }
                                    }
                                }
                            }

                            if (msg.Text.ToLower().StartsWith(".config_laura\n"))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "Я не являюсь администратором чата🙅🏻‍♀️!");
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "⛔️Вы не являетесь администратором!");
                                    }
                                    else
                                    {
                                        var text = msg.Text.ToLower();
                                        var configuration = text.Split(".config_laura\n")[1];
                                        await client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                        var process = await client.SendTextMessageAsync(msg.Chat.Id, "⚙️Обнаружена конфигурация чата! Начался процесс обработки конфигураций.");
                                        var configParse = configuration.Split('\n');
                                        await Task.Delay(1500);
                                        var result = "<b>Список настроек (BETA):</b>\n";
                                        for (int i = 0; i < configParse.Length; i++)
                                        {
                                            try
                                            {
                                                if (configParse[i].Contains(' '))
                                                {
                                                    var newConfParse = configParse[i].Split(' ');
                                                    var finConf = "";
                                                    for (int j = 0; j < newConfParse.Length; j++)
                                                    {
                                                        finConf += newConfParse[j];
                                                    }
                                                    Console.WriteLine(finConf);
                                                    var inp = finConf.Split(':')[0];
                                                    var value = finConf.Split(':')[1];
                                                    switch (inp)
                                                    {
                                                        case "nobad_words":
                                                            {
                                                                if (value == "on")
                                                                {
                                                                    Connector.Connector.GetNoBadLyrics(msg.Chat.Id);
                                                                    if (Connector.Connector.message == "not found")
                                                                    {
                                                                        Connector.Connector.RegisterNoBadLyrics(msg.Chat.Id, value);
                                                                    }
                                                                    else
                                                                    {
                                                                        Connector.Connector.SetNewParametrBL(msg.Chat.Id, value);
                                                                    }
                                                                    result += "<code>✅Параметр nobad_lyrics: on</code>\n";
                                                                }
                                                                else if (value == "off")
                                                                {
                                                                    Connector.Connector.GetNoBadLyrics(msg.Chat.Id);
                                                                    if (Connector.Connector.message == "not found")
                                                                    {
                                                                        Connector.Connector.RegisterNoBadLyrics(msg.Chat.Id, value);
                                                                    }
                                                                    else
                                                                    {
                                                                        Connector.Connector.SetNewParametrBL(msg.Chat.Id, value);
                                                                    }
                                                                    result += "<code>✅Параметр nobad_lyrics: off</code>\n";
                                                                }
                                                                else
                                                                {
                                                                    result += $"<code>⚠️Параметр nobad_lyrics \"{value}\" не существует</code>\n";
                                                                }
                                                                break;
                                                            }
                                                        case "max_warns":
                                                            {
                                                                try
                                                                {
                                                                    var value_int = Convert.ToInt32(value);
                                                                    if (value_int > 20)
                                                                    {
                                                                        result += "<code>⚠️Значение параметра max_warns превышает 20\n</code>";
                                                                    }
                                                                    else if (value_int < 3)
                                                                    {
                                                                        result += "<code>⚠️Значение параметра max_warns меньше 3\n</code>";
                                                                    }
                                                                    else
                                                                    {
                                                                        Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                                        if (Connector.Connector.message == "not found")
                                                                        {
                                                                            Connector.Connector.RegisterChatWarns(msg.Chat.Id, value_int, "mute");
                                                                            result += $"<code>✅Параметр max_warns: {value_int}</code>\n";
                                                                        }
                                                                        else
                                                                        {
                                                                            Connector.Connector.GetPunishment(msg.Chat.Id);
                                                                            var pun = Connector.Connector.punishment;
                                                                            Connector.Connector.UpdateMaxWarns(msg.Chat.Id, value_int, pun);
                                                                            result += $"<code>✅Параметр max_warns: {value_int}</code>\n";
                                                                        }
                                                                    }
                                                                    break;
                                                                }
                                                                catch (FormatException format)
                                                                {
                                                                    result += "<code>👾Error occured: value isn't int/long</code>\n";
                                                                    break;
                                                                }
                                                            }
                                                        case "punish":
                                                            {
                                                                if (value == "mute")
                                                                {
                                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                                    if (Connector.Connector.message == "not found")
                                                                    {
                                                                        Connector.Connector.RegisterChatWarns(msg.Chat.Id, 3, value);
                                                                    }
                                                                    else
                                                                    {
                                                                        var max = Connector.Connector.maxWarns;
                                                                        Connector.Connector.UpdateMaxWarns(msg.Chat.Id, max, value);
                                                                    }
                                                                    result += $"<code>✅Параметр punish: {value}</code>\n";
                                                                }
                                                                else if (value == "ban")
                                                                {
                                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                                    if (Connector.Connector.message == "not found")
                                                                    {
                                                                        Connector.Connector.RegisterChatWarns(msg.Chat.Id, 3, value);
                                                                    }
                                                                    else
                                                                    {
                                                                        var max = Connector.Connector.maxWarns;
                                                                        Connector.Connector.UpdateMaxWarns(msg.Chat.Id, max, value);
                                                                    }
                                                                    result += $"<code>✅Параметр punish: {value}</code>\n";
                                                                }
                                                                else
                                                                {
                                                                    result += $"<code>⚠️Параметр punish \"{value}\" не существует</code>\n";
                                                                }
                                                                break;
                                                            }
                                                        case "max_msg":
                                                            {
                                                                try
                                                                {
                                                                    var value_int = Convert.ToInt32(value);
                                                                    Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                    if (Connector.Connector.message == "not found")
                                                                    {
                                                                        if (value_int < 4)
                                                                        {
                                                                            result += $"<code>⚠️Warning max_msg: Максимальное количество сообщений в секунду не может быть ниже 4</code>\n";
                                                                        }
                                                                        else if (value_int > 10)
                                                                        {
                                                                            result += $"<code>⚠️Warning max_msg: Максимальное количество сообщений в секунду не должно превышать 10</code>\n";
                                                                        }
                                                                        else
                                                                        {
                                                                            Connector.Connector.CreateSpamConfig(msg.Chat.Id, value_int, "off");
                                                                            result += $"<code>✅Параметр max_msg: {value_int}</code>\n";
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (value_int < 4)
                                                                        {
                                                                            result += $"<code>⚠️Warning max_msg: Максимальное количество сообщений в секунду не может быть ниже 4</code>\n";
                                                                        }
                                                                        else if (value_int > 10)
                                                                        {
                                                                            result += $"<code>⚠️Warning max_msg: Максимальное количество сообщений в секунду не должно превышать 10</code>\n";
                                                                        }
                                                                        else
                                                                        {
                                                                            Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                            Connector.Connector.UpdateSpamConfig(msg.Chat.Id, value_int, Connector.Connector.as_active);
                                                                            result += $"<code>✅Параметр max_msg: {value_int}</code>\n";
                                                                        }
                                                                    }
                                                                }
                                                                catch (FormatException exc)
                                                                {
                                                                    result += "<code>👾Error occured: value isn't int/long</code>\n";
                                                                    break;
                                                                }
                                                                break;
                                                            }
                                                        case "as_active":
                                                            {
                                                                switch (value)
                                                                {
                                                                    case "on":
                                                                        {
                                                                            Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                            if (Connector.Connector.message == "not found")
                                                                            {
                                                                                Connector.Connector.CreateSpamConfig(msg.Chat.Id, 4, value);
                                                                                result += $"<code>✅Параметр as_active: {value}</code>\n";
                                                                            }
                                                                            else
                                                                            {
                                                                                Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                                var max = Connector.Connector.maxMess;
                                                                                Connector.Connector.UpdateSpamConfig(msg.Chat.Id, max, value);
                                                                                result += $"<code>✅Параметр as_active: {value}</code>\n";
                                                                            }
                                                                            break;
                                                                        }
                                                                    case "off":
                                                                        {
                                                                            Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                            if (Connector.Connector.message == "not found")
                                                                            {
                                                                                Connector.Connector.CreateSpamConfig(msg.Chat.Id, 4, value);
                                                                                result += $"<code>✅Параметр as_active: {value}</code>\n";
                                                                            }
                                                                            else
                                                                            {
                                                                                Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                                var max = Connector.Connector.maxMess;
                                                                                Connector.Connector.UpdateSpamConfig(msg.Chat.Id, max, value);
                                                                                result += $"<code>✅Параметр as_active: {value}</code>\n";
                                                                            }
                                                                            break;
                                                                        }
                                                                    default:
                                                                        {
                                                                            result += $"<code>⚠️Параметр as_active \"{value}\" не существует</code>\n";
                                                                            break;
                                                                        }

                                                                }
                                                                break;
                                                            }
                                                        case "nightmode":
                                                            {
                                                                try
                                                                {
                                                                    var night = value.Split('|')[1].Split('-');
                                                                    var state = value.Split('|')[0].Split('-');
                                                                    Connector.Connector.GetNightMode(msg.Chat.Id);
                                                                    var test = Convert.ToInt32(night[0]);
                                                                    var test1 = Convert.ToInt32(night[1]);
                                                                    var test2 = Convert.ToInt32(state[0]);
                                                                    var test3 = Convert.ToInt32(state[1]);
                                                                    if (test >= 24 | test < 0)
                                                                    {
                                                                        result += "<code>⚠️Параметр nightmode не соответствует формату времени</code>\n";
                                                                    }
                                                                    if (test1 >= 60 | test1 < 0)
                                                                    {
                                                                        result += "<code>⚠️Параметр nightmode не соответствует формату времени</code>\n";
                                                                    }
                                                                    if (test2 >= 24 | test2 < 0)
                                                                    {
                                                                        result += "<code>⚠️Параметр statemode не соответствует формату времени</code>\n";
                                                                    }
                                                                    if (test3 >= 60 | test3 < 0)
                                                                    {
                                                                        result += "<code>⚠️Параметр statemode не соответствует формату времени</code>\n";
                                                                    }
                                                                    else if (test == test2)
                                                                    {
                                                                        result += "<code>📛Указанное время для параметра \"nightmode\" нарушает формат ночного времени!</code>";
                                                                    }
                                                                    else
                                                                    {
                                                                        if (Connector.Connector.message == "not found")
                                                                        {
                                                                            Connector.Connector.CreateNightMode(msg.Chat.Id, night[0] + ":" + night[1], state[0] + ":" + state[1]);
                                                                        }
                                                                        else
                                                                        {
                                                                            Connector.Connector.UpdateNightMode(msg.Chat.Id, night[0] + ":" + night[1], state[0] + ":" + state[1]);
                                                                        }
                                                                        result += $"<code>✅Параметр nightmode: {night[0]}:{night[1]}-{state[0]}:{state[1]}\n</code>";
                                                                    }
                                                                }
                                                                catch (FormatException eEEEEEEEE)
                                                                {
                                                                    result += "<code>👾Error occured: value is string\n</code>";
                                                                }
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                result += $"<code>❌Параметр \"{inp}\" не найден\n</code>";
                                                                break;
                                                            }
                                                    }

                                                    if (result == "<b>Список настроек (BETA):</b>\n")
                                                        await client.DeleteMessageAsync(msg.Chat.Id, process.MessageId);
                                                    else
                                                        await client.EditMessageTextAsync(msg.Chat.Id, process.MessageId, result, ParseMode.Html);
                                                    Thread.Sleep(750);
                                                }
                                                else
                                                {
                                                    var inp = configParse[i].Split(':')[0];
                                                    var value = configParse[i].Split(':')[1];
                                                    switch (inp)
                                                    {
                                                        case "nobad_words":
                                                            {
                                                                if (value == "on")
                                                                {
                                                                    Connector.Connector.GetNoBadLyrics(msg.Chat.Id);
                                                                    if (Connector.Connector.message == "not found")
                                                                    {
                                                                        Connector.Connector.RegisterNoBadLyrics(msg.Chat.Id, value);
                                                                    }
                                                                    else
                                                                    {
                                                                        Connector.Connector.SetNewParametrBL(msg.Chat.Id, value);
                                                                    }
                                                                    result += "<code>✅Параметр nobad_lyrics: on</code>\n";
                                                                }
                                                                else if (value == "off")
                                                                {
                                                                    Connector.Connector.GetNoBadLyrics(msg.Chat.Id);
                                                                    if (Connector.Connector.message == "not found")
                                                                    {
                                                                        Connector.Connector.RegisterNoBadLyrics(msg.Chat.Id, value);
                                                                    }
                                                                    else
                                                                    {
                                                                        Connector.Connector.SetNewParametrBL(msg.Chat.Id, value);
                                                                    }
                                                                    result += "<code>✅Параметр nobad_lyrics: off</code>\n";
                                                                }
                                                                else
                                                                {
                                                                    result += $"<code>⚠️Параметр nobad_lyrics \"{value}\" не существует</code>\n";
                                                                }
                                                                break;
                                                            }

                                                        case "max_warns":
                                                            {
                                                                try
                                                                {
                                                                    var value_int = Convert.ToInt32(value);
                                                                    if (value_int > 20)
                                                                    {
                                                                        result += "<code>⚠️Значение параметра max_warns превышает 20\n</code>";
                                                                    }
                                                                    else if (value_int < 3)
                                                                    {
                                                                        result += "<code>⚠️Значение параметра max_warns меньше 3\n</code>";
                                                                    }
                                                                    else
                                                                    {
                                                                        Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                                        if (Connector.Connector.message == "not found")
                                                                        {
                                                                            Connector.Connector.RegisterChatWarns(msg.Chat.Id, value_int, "mute");
                                                                            result += $"<code>✅Параметр max_warns: {value_int}</code>\n";
                                                                        }
                                                                        else
                                                                        {
                                                                            Connector.Connector.GetPunishment(msg.Chat.Id);
                                                                            var pun = Connector.Connector.punishment;
                                                                            Connector.Connector.UpdateMaxWarns(msg.Chat.Id, value_int, pun);
                                                                            result += $"<code>✅Параметр max_warns: {value_int}</code>\n";
                                                                        }
                                                                    }
                                                                    break;
                                                                }
                                                                catch (FormatException format)
                                                                {
                                                                    result += "<code>👾Error occured: value isn't int/long</code>\n";
                                                                    break;
                                                                }
                                                            }
                                                        case "punish":
                                                            {
                                                                if (value == "mute")
                                                                {
                                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                                    if (Connector.Connector.message == "not found")
                                                                    {
                                                                        Connector.Connector.RegisterChatWarns(msg.Chat.Id, 3, value);
                                                                    }
                                                                    else
                                                                    {
                                                                        var max = Connector.Connector.maxWarns;
                                                                        Connector.Connector.UpdateMaxWarns(msg.Chat.Id, max, value);
                                                                    }
                                                                    result += $"<code>✅Параметр punish: {value}</code>\n";
                                                                }
                                                                else if (value == "ban")
                                                                {
                                                                    Connector.Connector.CheckChatWarns(msg.Chat.Id);
                                                                    if (Connector.Connector.message == "not found")
                                                                    {
                                                                        Connector.Connector.RegisterChatWarns(msg.Chat.Id, 3, value);
                                                                    }
                                                                    else
                                                                    {
                                                                        var max = Connector.Connector.maxWarns;
                                                                        Connector.Connector.UpdateMaxWarns(msg.Chat.Id, max, value);
                                                                    }
                                                                    result += $"<code>✅Параметр punish: {value}</code>\n";
                                                                }
                                                                else
                                                                {
                                                                    result += $"<code>⚠️Параметр punish \"{value}\" не существует</code>\n";
                                                                }
                                                                break;
                                                            }
                                                        case "max_msg":
                                                            {
                                                                try
                                                                {
                                                                    var value_int = Convert.ToInt32(value);
                                                                    Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                    if (Connector.Connector.message == "not found")
                                                                    {
                                                                        if (value_int < 4)
                                                                        {
                                                                            result += $"<code>⚠️Warning max_msg: Максимальное количество сообщений в секунду не может быть ниже 4</code>\n";
                                                                        }
                                                                        else if (value_int > 10)
                                                                        {
                                                                            result += $"<code>⚠️Warning max_msg: Максимальное количество сообщений в секунду не должно превышать 10</code>\n";
                                                                        }
                                                                        else
                                                                        {
                                                                            Connector.Connector.CreateSpamConfig(msg.Chat.Id, value_int, "off");
                                                                            result += $"<code>✅Параметр max_msg: {value_int}</code>\n";
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (value_int < 4)
                                                                        {
                                                                            result += $"<code>⚠️Warning max_msg: Максимальное количество сообщений в секунду не может быть ниже 4</code>\n";
                                                                        }
                                                                        else if (value_int > 10)
                                                                        {
                                                                            result += $"<code>⚠️Warning max_msg: Максимальное количество сообщений в секунду не должно превышать 10</code>\n";
                                                                        }
                                                                        else
                                                                        {
                                                                            Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                            Connector.Connector.UpdateSpamConfig(msg.Chat.Id, value_int, Connector.Connector.as_active);
                                                                            result += $"<code>✅Параметр max_msg: {value_int}</code>\n";
                                                                        }
                                                                    }
                                                                }
                                                                catch (FormatException exc)
                                                                {
                                                                    result += "<code>👾Error occured: value isn't int/long</code>\n";
                                                                    break;
                                                                }
                                                                break;
                                                            }
                                                        case "as_active":
                                                            {
                                                                switch (value)
                                                                {
                                                                    case "on":
                                                                        {
                                                                            Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                            if (Connector.Connector.message == "not found")
                                                                            {
                                                                                Connector.Connector.CreateSpamConfig(msg.Chat.Id, 4, value);
                                                                                result += $"<code>✅Параметр as_active: {value}</code>\n";
                                                                            }
                                                                            else
                                                                            {
                                                                                Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                                var max = Connector.Connector.maxMess;
                                                                                Connector.Connector.UpdateSpamConfig(msg.Chat.Id, max, value);
                                                                                result += $"<code>✅Параметр as_active: {value}</code>\n";
                                                                            }
                                                                            break;
                                                                        }
                                                                    case "off":
                                                                        {
                                                                            Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                            if (Connector.Connector.message == "not found")
                                                                            {
                                                                                Connector.Connector.CreateSpamConfig(msg.Chat.Id, 4, value);
                                                                                result += $"<code>✅Параметр as_active: {value}</code>\n";
                                                                            }
                                                                            else
                                                                            {
                                                                                Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                                                                var max = Connector.Connector.maxMess;
                                                                                Connector.Connector.UpdateSpamConfig(msg.Chat.Id, max, value);
                                                                                result += $"<code>✅Параметр as_active: {value}</code>\n";
                                                                            }
                                                                            break;
                                                                        }
                                                                    default:
                                                                        {
                                                                            result += $"<code>⚠️Параметр as_active \"{value}\" не существует</code>\n";
                                                                            break;
                                                                        }

                                                                }
                                                                break;
                                                            }
                                                        case "nightmode":
                                                            {
                                                                try
                                                                {
                                                                    var night = value.Split('|')[1].Split('-');
                                                                    var state = value.Split('|')[0].Split('-');
                                                                    Connector.Connector.GetNightMode(msg.Chat.Id);
                                                                    var test = Convert.ToInt32(night[0]);
                                                                    var test1 = Convert.ToInt32(night[1]);
                                                                    var test2 = Convert.ToInt32(state[0]);
                                                                    var test3 = Convert.ToInt32(state[1]);
                                                                    if (test >= 24 | test < 0)
                                                                    {
                                                                        result += "<code>⚠️Параметр nightmode не соответствует формату времени</code>\n";
                                                                    }
                                                                    if (test1 >= 60 | test1 < 0)
                                                                    {
                                                                        result += "<code>⚠️Параметр nightmode не соответствует формату времени</code>\n";
                                                                    }
                                                                    if (test2 >= 24 | test2 < 0)
                                                                    {
                                                                        result += "<code>⚠️Параметр statemode не соответствует формату времени</code>\n";
                                                                    }
                                                                    if (test3 >= 60 | test3 < 0)
                                                                    {
                                                                        result += "<code>⚠️Параметр statemode не соответствует формату времени</code>\n";
                                                                    }
                                                                    else if (test == test2)
                                                                    {
                                                                        result += "<code>📛Указанное время для параметра \"nightmode\" нарушает формат ночного времени!</code>";
                                                                    }
                                                                    else
                                                                    {
                                                                        if (Connector.Connector.message == "not found")
                                                                        {
                                                                            Connector.Connector.CreateNightMode(msg.Chat.Id, night[0] + ":" + night[1], state[0] + ":" + state[1]);
                                                                        }
                                                                        else
                                                                        {
                                                                            Connector.Connector.UpdateNightMode(msg.Chat.Id, night[0] + ":" + night[1], state[0] + ":" + state[1]);
                                                                        }
                                                                        result += $"<code>✅Параметр nightmode: {night[0]}:{night[1]}-{state[0]}:{state[1]}\n</code>";
                                                                    }
                                                                }
                                                                catch (FormatException eEEEEEEEE)
                                                                {
                                                                    result += "<code>👾Error occured: value is string\n</code>";
                                                                }
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                result += $"<code>❌Параметр \"{inp}\" не найден\n</code>";
                                                                break;
                                                            }
                                                    }

                                                    if (result == "<b>Список настроек (BETA):</b>\n")
                                                        await client.DeleteMessageAsync(msg.Chat.Id, process.MessageId);
                                                    else
                                                        await client.EditMessageTextAsync(msg.Chat.Id, process.MessageId, result, ParseMode.Html);
                                                    Thread.Sleep(750);
                                                }
                                            }
                                            catch (IndexOutOfRangeException aut)
                                            {
                                                result += $"<code>👾Error occured: value is null</code>\n";
                                                await client.EditMessageTextAsync(msg.Chat.Id, process.MessageId, result, ParseMode.Html);
                                                Thread.Sleep(750);
                                            }

                                        }
                                    }
                                }
                            }

                            if (msg.Text.StartsWith("+правила\n"))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    try
                                    {
                                        var rule = msg.Text.Split("+правила\n")[1];
                                        ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                        if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == myId; }) == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            return;
                                        }
                                        else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                            return;
                                        }
                                        else if (rule.Length > 1024)
                                        {
                                            await client.SendTextMessageAsync(msg.Chat.Id, "⚠️Правила превышают больше 1024 символов!");
                                        }
                                        else
                                        {
                                            Connector.Connector.GetChatRules(msg.Chat.Id);
                                            if (Connector.Connector.message == "not setted")
                                            {
                                                Connector.Connector.CreateChatRules(msg.Chat.Id, rule);
                                                await client.SendTextMessageAsync(msg.Chat.Id, "✅Правила успешно установлены!");
                                            }
                                            else
                                            {
                                                Connector.Connector.UpdateChatRules(msg.Chat.Id, rule);
                                                await client.SendTextMessageAsync(msg.Chat.Id, "✅Правила успешно обновлены!");
                                            }
                                        }
                                    }

                                    catch (IndexOutOfRangeException eeeee)
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "Вы не можете поставить пустые правила👾");
                                    }
                                }
                            }

                            if (msg.Text.ToLower() == "правила чата")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    Connector.Connector.GetChatRules(msg.Chat.Id);
                                    if (Connector.Connector.message == "not setted")
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "🚫Правила не установлены");
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, $"📜Правила чата <b>\"{msg.Chat.Title}\"</b>", ParseMode.Html);
                                        await client.SendTextMessageAsync(msg.Chat.Id, Connector.Connector.rule);
                                    }
                                }
                            }

                            if (msg.Text.ToLower() == "-правила")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    Connector.Connector.GetChatRules(msg.Chat.Id);
                                    if (Connector.Connector.message == "not setted")
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "🚫Правила не установлены");
                                    }
                                    else
                                    {
                                        Connector.Connector.DeleteChatRules(msg.Chat.Id);
                                        await client.SendTextMessageAsync(msg.Chat.Id, "✅Правила успешно удалены!");
                                    }
                                }
                            }

                            if (msg.Text.StartsWith("/sub_weather "))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    var city = msg.Text.Split("/sub_weather ")[1];
                                    WeatherApi.Weather(city);
                                    if (WeatherApi.ResponseIsNormal == false)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<b>Город не был найден⚠!</b>\nГород не существует, либо написан не в именительном падеже.\n\n<i>Пример: Санкт-Петербург</i>", parseMode: ParseMode.Html);
                                    }
                                    else
                                    {
                                        Connector.Connector.GetSubData(msg.Chat.Id);
                                        if (Connector.Connector.message == "not sub")
                                        {
                                            Connector.Connector.CreateSubData(msg.Chat.Id, city, DateTime.Now.AddHours(3));
                                            await client.SendTextMessageAsync(msg.Chat.Id, $"Вы подписались на ежечасную рассылку прогноза погоды города \"{city}\"🔔!");
                                        }
                                        else
                                        {
                                            Connector.Connector.UpdateSubData(msg.Chat.Id, city, DateTime.Now.AddHours(3));
                                            await client.SendTextMessageAsync(msg.Chat.Id, $"Вы переподписались на ежечасную рассылку прогноза погоды города \"{city}\"🔄!");
                                        }
                                    }
                                }
                            }

                            if (msg.Text.ToLower() == "/unsub_weather")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    Connector.Connector.GetSubData(msg.Chat.Id);
                                    if (Connector.Connector.message == "not sub")
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, "Вы не были подписаны на рассылку ежечасной погоды🔕!");
                                    }
                                    else
                                    {
                                        Connector.Connector.DeleteSubData(msg.Chat.Id);
                                        await client.SendTextMessageAsync(msg.Chat.Id, "✅Теперь вы отписались от рассылки ежечасной погоды!");
                                    }
                                }
                            }

                            if (msg.Text.ToLower() == "-ночной мод")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else
                                    {
                                        Connector.Connector.GetNightMode(msg.Chat.Id);
                                        if (Connector.Connector.message == "not found")
                                        {
                                            await client.SendTextMessageAsync(msg.Chat.Id, "🚫Ночной режим не был установлен!");
                                        }
                                        else
                                        {
                                            Connector.Connector.DeleteNightMode(msg.Chat.Id);
                                            await client.SendTextMessageAsync(msg.Chat.Id, "✅Ночной режим теперь удален!");
                                        }
                                    }
                                }
                            }

                            if (msg.Text.ToLower().StartsWith("+ночной мод\n"))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            Connector.Connector.GetNightMode(msg.Chat.Id);
                                            var time1 = msg.Text.Split('\n')[1].Split('-')[0];
                                            var time2 = msg.Text.Split('\n')[1].Split('-')[1];
                                            var night = time1.Split(':');
                                            var state = time2.Split(':');
                                            var test = Convert.ToInt32(night[0]);
                                            var test1 = Convert.ToInt32(night[1]);
                                            var test2 = Convert.ToInt32(state[0]);
                                            var test3 = Convert.ToInt32(state[1]);
                                            if (test >= 24 | test < 0)
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, "📛Указанное время нарушает формат времени!");
                                                return;
                                            }
                                            if (test1 >= 60 | test1 < 0)
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, "📛Указанное время нарушает формат времени!");
                                                return;
                                            }
                                            if (test2 >= 24 | test2 < 0)
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, "📛Указанное время нарушает формат времени!");
                                                return;
                                            }
                                            if (test3 >= 60 | test3 < 0)
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, "📛Указанное время нарушает формат времени!");
                                                return;
                                            }
                                            else if (test == test2)
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, "📛Указанное время нарушает формат ночного времени!");
                                                return;
                                            }
                                            else
                                            {
                                                if (Connector.Connector.message == "not found")
                                                {
                                                    Connector.Connector.CreateNightMode(msg.Chat.Id, time1, time2);
                                                    await client.SendTextMessageAsync(msg.Chat.Id, "✅Ночной режим успешно установлен!");
                                                }
                                                else
                                                {
                                                    Connector.Connector.UpdateNightMode(msg.Chat.Id, time1, time2);
                                                    await client.SendTextMessageAsync(msg.Chat.Id, "✅Ночной режим успешно обновлен!");
                                                }
                                            }
                                        }
                                        catch (IndexOutOfRangeException eeeeeeeeeeeee)
                                        {
                                            await client.SendTextMessageAsync(msg.Chat.Id, "<code>👾Error occured: value is null</code>", ParseMode.Html);
                                        }
                                    }
                                }
                            }

                            if (msg.Text.ToLower() == "-варн")
                            {
                                if (msg.From.Id == msg.Chat.Id)
                                {
                                    return;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, "Вы не указали пользователя!");
                                }
                                else if (msg.ReplyToMessage.From.IsBot)
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, $"@{msg.ReplyToMessage.From.FirstName} является ботом🤖");
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else
                                    {
                                        Connector.Connector.CheckUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                        if (Connector.Connector.message == "not found")
                                        {
                                            await client.SendTextMessageAsync(msg.Chat.Id, $@"<b>📛Пользователь не получал предупреждений</b>", ParseMode.Html);
                                        }
                                        else
                                        {
                                            Connector.Connector.DeleteOneWarn(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                            if (Connector.Connector.message == "user warns is 1")
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, "У пользователя теперь нет ни предупреждений✅!");
                                            }
                                            else if (Connector.Connector.message == "one warn deleted")
                                            {
                                                await client.SendTextMessageAsync(msg.Chat.Id, "Успешно удален первое предупреждение✅!");
                                            }
                                        }
                                    }
                                }

                            }

                            if (msg.Text.ToLower() == "-варны")
                            {
                                if (msg.From.Id == msg.Chat.Id)
                                {
                                    return;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, "Вы не указали пользователя!");
                                }
                                else if (msg.ReplyToMessage.From.IsBot)
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, $"@{msg.ReplyToMessage.From.FirstName} является ботом🤖");
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else
                                    {
                                        Connector.Connector.CheckUserWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                        if (Connector.Connector.message == "not found")
                                        {
                                            await client.SendTextMessageAsync(msg.Chat.Id, $@"<b>📛Пользователь не получал предупреждений</b>", ParseMode.Html);
                                        }
                                        else
                                        {
                                            Connector.Connector.DeleteReasonsOfWarns(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                            Connector.Connector.DeleteWarnUser(msg.ReplyToMessage.From.Id, msg.Chat.Id);
                                            await client.SendTextMessageAsync(msg.Chat.Id, "У пользователя теперь нет ни предупреждений✅!");
                                        }
                                    }
                                }

                            }

                            if (msg.Text.ToLower().StartsWith("/fba_news "))
                            {
                                var split = msg.Text.ToLower().Split(' ');
                                if (Connector.Connector.GetFBANews(msg.Chat.Id) == false)
                                {
                                    switch (split[1])
                                    {
                                        case "on":
                                            Connector.Connector.CreateFBANews(msg.Chat.Id, "on");
                                            await client.SendTextMessageAsync(msg.Chat.Id, $"✅Вы включили рассылку новостей от FBA Studio!");
                                            break;
                                        case "off":
                                            Connector.Connector.CreateFBANews(msg.Chat.Id, "off");
                                            await client.SendTextMessageAsync(msg.Chat.Id, $"✅Вы отключили рассылку новостей от FBA Studio!");
                                            break;
                                        default:
                                            await client.SendTextMessageAsync(msg.Chat.Id, $"⚠️{split[1]} не существует!");
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (split[1])
                                    {
                                        case "on":
                                            Connector.Connector.UpdateFBANews(msg.Chat.Id, "on");
                                            await client.SendTextMessageAsync(msg.Chat.Id, $"✅Вы включили рассылку новостей от FBA Studio!");
                                            break;
                                        case "off":
                                            Connector.Connector.UpdateFBANews(msg.Chat.Id, "off");
                                            await client.SendTextMessageAsync(msg.Chat.Id, $"✅Вы отключили рассылку новостей от FBA Studio!");
                                            break;
                                        default:
                                            await client.SendTextMessageAsync(msg.Chat.Id, $"⚠️{split[1]} не существует!");
                                            break;
                                    }
                                }
                            }

                            if (msg.Text.ToLower().StartsWith("/set_capcha") || msg.Text.ToLower().StartsWith("/set_capcha@laura_cm_bot"))
                            {
                                if(msg.From.Id == msg.Chat.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else
                                    {
                                        Connector.Connector.GetCapchaSetting(msg.Chat.Id);
                                        var active = msg.Text.ToLower().Split(' ')[1];
                                        if(Connector.Connector.message == "not found")
                                        {
                                            switch(active)
                                            {
                                                case "on":
                                                    Connector.Connector.CreateCapchaSetting(msg.Chat.Id, active, 5);
                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"<code>✅Капча в чате установлена!</code>", ParseMode.Html);
                                                    break;
                                                case "off":
                                                    Connector.Connector.CreateCapchaSetting(msg.Chat.Id, active, 5);
                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"<code>⛔️Капча в чате отключена!</code>", ParseMode.Html);
                                                    break;
                                                default:
                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"<code>⚠️Значение '{active}' не существует</code>", ParseMode.Html);
                                                    break;
                                            }   
                                        }
                                        else
                                        {
                                            switch (active)
                                            {
                                                case "on":
                                                    Connector.Connector.UpdateCapchaSetting(msg.Chat.Id, active, 5);
                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"<code>✅Капча в чате установлена!</code>", ParseMode.Html);
                                                    break;
                                                case "off":
                                                    Connector.Connector.UpdateCapchaSetting(msg.Chat.Id, active, 5);
                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"<code>⛔️Капча в чате отключена!</code>", ParseMode.Html);
                                                    break;
                                                default:
                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"<code>⚠️Значение '{active}' не существует</code>", ParseMode.Html);
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (msg.Text.ToLower().StartsWith("+нотиф коммент\n"))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else
                                    {
                                        var inputNotifComment = msg.Text.Split(msg.Text.Split('\n')[0] + '\n')[1];
                                        var check = await client.SendTextMessageAsync(msg.Chat.Id, "<i>Идёт проверка на правильное оформление HTML-тегов...</i>", ParseMode.Html);
                                        Thread.Sleep(350);
                                        try
                                        {
                                            await client.EditMessageTextAsync(msg.Chat.Id, check.MessageId, inputNotifComment, ParseMode.Html);
                                            await client.SendTextMessageAsync(msg.Chat.Id, "✅HTML-теги введены правильно или вовсе не использованы, выше вы видите результат комментария для уведомления в комментариях поста канала");
                                            if (Connector.Connector.GetNotifComment(msg.Chat.Id) == null)
                                            {
                                                Connector.Connector.CreateNotifComment(msg.Chat.Id, inputNotifComment);
                                                await client.SendTextMessageAsync(msg.Chat.Id, "✅Комментарий уведомлении записан");
                                            }
                                            else
                                            {
                                                Connector.Connector.UpdateNotifComment(msg.Chat.Id, inputNotifComment);
                                                await client.SendTextMessageAsync(msg.Chat.Id, "✅Комментарий уведомлении записан");
                                            }
                                        }
                                        catch (ApiRequestException html)
                                        {
                                            await client.EditMessageTextAsync(msg.Chat.Id, check.MessageId, $"<code>👾Error occured: {html.Message}</code>\n<b>Рекомендуем посмотреть доступные теги и их правильное применение здесь: <a href=\"https://telegra.ph/Rabota-s-HTML-tegami-dlya-oformleniya-kommentariya-uvedomlenii-11-02\">Как оформить уведомление комментария</a></b>", ParseMode.Html, disableWebPagePreview: true);
                                        }
                                    }
                                }

                                //RP commands
                                if (msg.Text.StartsWith("+"))
                                {
                                    {
                                        if (msg.Chat.Id == msg.From.Id)
                                        {
                                            return;
                                        }
                                        else if (msg.ReplyToMessage == null)
                                        {
                                            return;
                                        }
                                        else if (msg.ReplyToMessage != null)
                                        {
                                            if (msg.ReplyToMessage.From.Id == msg.From.Id)
                                            {
                                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, а у вас, как я вижу, высокая самооценка😏", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                return;
                                            }
                                            else
                                            {
                                                Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                if (Connector.Connector.message == "don't rate")
                                                {
                                                    Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                    if (Connector.Connector.message == "not rated")
                                                    {
                                                        Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, 1);
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: 1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                    else
                                                    {
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        var rating = Connector.Connector.user_rating + 1;
                                                        Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                }
                                                else
                                                {
                                                    Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                    if (Connector.Connector.message == "not rated")
                                                    {
                                                        Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, 1);
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: 1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                    else
                                                    {
                                                        Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                        if (Connector.Connector.count_control >= 4)
                                                        {
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"🚫Вы уже исчерпали лимит оценивания <b>{msg.ReplyToMessage.From.FirstName}</b>\n⏱Дата сброса лимита: {Connector.Connector.soc_control}", ParseMode.Html);
                                                        }
                                                        else
                                                        {
                                                            var control = Connector.Connector.count_control + 1;
                                                            Connector.Connector.UpdateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, control, DateTime.Now.AddDays(1));
                                                            var rating = Connector.Connector.user_rating + 1;
                                                            Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (msg.Text.StartsWith("-"))
                                {
                                    {
                                        if (msg.Chat.Id == msg.From.Id)
                                        {
                                            return;
                                        }
                                        else if (msg.ReplyToMessage == null)
                                        {
                                            return;
                                        }
                                        else if (msg.ReplyToMessage != null)
                                        {
                                            if (msg.Text.Contains("_") || msg.Text.ToLower().StartsWith("-варн"))
                                            {
                                                return;
                                            }
                                            else if (msg.ReplyToMessage.From.Id == msg.From.Id)
                                            {
                                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, зачем себя так унижать😕", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                return;
                                            }
                                            else
                                            {
                                                Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                if (Connector.Connector.message == "don't rate")
                                                {
                                                    Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                    if (Connector.Connector.message == "not rated")
                                                    {
                                                        Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, -1);
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: -1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                    else
                                                    {
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        var rating = Connector.Connector.user_rating - 1;
                                                        Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                }
                                                else
                                                {
                                                    Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                    if (Connector.Connector.message == "not rated")
                                                    {
                                                        Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, -1);
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: -1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                    else
                                                    {
                                                        Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                        if (Connector.Connector.count_control >= 4)
                                                        {
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"🚫Вы уже исчерпали лимит оценивания <b>{msg.ReplyToMessage.From.FirstName}</b>\n⏱Дата сброса лимита: {Connector.Connector.soc_control}", ParseMode.Html);
                                                        }
                                                        else
                                                        {
                                                            var control = Connector.Connector.count_control + 1;
                                                            Connector.Connector.UpdateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, control, DateTime.Now.AddDays(1));
                                                            var rating = Connector.Connector.user_rating - 1;
                                                            Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        return;
                                    }
                                }


                                if (msg.Text.ToUpper() == "ОБНЯТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> обнял себя🤗", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id{msg.From.Id}\">{msg.From.FirstName}</a> обнял <a href = \"tg://user?id{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🤗", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }

                                }

                                if (msg.Text.ToUpper() == "УДАРИТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> ударил со всей силой в пустоту😶", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> ударил со всей силой в <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🤕", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                }

                                if (msg.Text.ToUpper() == "УБИТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> покончил свою жизнь самоубийством🤡🔪", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> убивает <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🔪😢", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }

                                }

                                if (msg.Text.ToUpper() == "УКУСИТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> укусил себя🤡", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> делает укус <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🐺", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }

                                }

                                if (msg.Text.ToUpper() == "ПОКАЗАТЬ ЯЗЫК")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> просто показал язык👅", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?sid={msg.From.Id}\">{msg.From.FirstName}</a> показал язык <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>😜", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }

                                }

                                if (msg.Text.ToUpper() == "НАКОРМИТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> вкусно покормил себя😋", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> вкусно покормил участника <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🍔🍟🌭", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }

                                if (msg.Text.ToUpper() == "КАСТРИРОВАТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> себя кастрировал🫡", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> кастрировал <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>✂️", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }

                                //Buttons/easters
                                switch (msg.Text)
                                {

                                    //Just button
                                    case "VPN":
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "<b>FBA Studio x LibreNet</b>\n\n<b><i>Доступные сервера:</i></b>\n<i>LanceVPN_Netherlands:</i>\n¬Скорость скачивания: 250 МБит/с\n¬Страна: Нидерланды\n¬Цена: 50 Руб/мес\nПротокол: WireGuard", parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Прайслист", "https://t.me/LanceVPN")));
                                            break;
                                        }

                                    //Buttons from "GetButtons"
                                    case "Инструкция📚":
                                        if (msg.Chat.Id != msg.From.Id)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Инструкция для работы с телеграм ботом:\ntelegra.ph/Rabota-s-botom-Laura-10-18", parseMode: ParseMode.Html, disableWebPagePreview: false);
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
                                                "<b href=\"t.me/FBA_Studio\">FBA Studio</b> - это команда разработчиков, которые делают Телеграм ботов с нужным функционалом без обязательной рекламы, они настроены больше всего на мнение комьюнити, думаю, тебе стоит посетить их канал🤗",
                                                replyMarkup: InfoKeyboard, parseMode: ParseMode.Html);
                                            break;
                                        }


                                    case "Добавить бота в чат➕":
                                        if (msg.Chat.Id != msg.From.Id)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, text: $"<b>❗Права, необходимые для модерировани бота:</b>\n\n<i>-измененния прав участников чата</i>\n<i>-удаление участников из чата</i>\n<i>-удаление чужих сообщений</i>\n<i>Изменения разрешений чата</i>", parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Добавить бота в группу", "http://t.me/Laura_cm_bot?startgroup=start&admin=change_info+restrict_members+delete_messages+pin_messages+invite_users")));
                                            break;
                                        }

                                    //Sponsors of the bot
                                    case "Партнёры бота":
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, text: "<b>Наши партнёры бота🤝:</b>\n<i>-@FlushaStudio(Использование бота)</i>\n<i>-@RiceTeamStudio(Пиар проекта)</i>\n<i>-@banan4ikmoder(Пиар)</i>\n<i>@TheShadow_hk(Dev, разработал проверку статуса участника для корректного бана в чате)</i>\n<i>Maxim Bysh(Помог с разработкой мута по заданному времени)</i>\n<i>Libreto(Предоставления сервера)</i>\n<i>Список обновляется</i>", parseMode: ParseMode.Html);
                                        break;
                                }

                                //Get Chat Id (For admins)
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

                                //Get Uptime
                                if (msg.Text.ToLower().StartsWith("/uptime"))
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, $"Мой Uptime:\n{timeOfStart}");
                                }

                                //Get your id 
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

                                //Get our id
                                if (msg.Text.StartsWith("/getourid"))
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        if (msg.ReplyToMessage == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Вы не указали пользователя!");
                                        }
                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"ID пользователя {msg.ReplyToMessage.From.FirstName}:\n<code>{msg.ReplyToMessage.From.Id}</code>", parseMode: ParseMode.Html);
                                        }

                                    }
                                }

                                //Random rating
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

                                //Weather request from WeatherClassApi.cs
                                if (msg.Text.ToLower().StartsWith("погода в"))
                                {
                                    string[] strings = msg.Text.Split(' ');
                                    String[] InpResponse = msg.Text.Split(strings[0] + ' ' + strings[1] + ' ');
                                    var inputCityName = InpResponse[1];
                                    WeatherApi.Weather(inputCityName);
                                    WeatherApi.Celsius(celsius: WeatherApi.temperatureCity);
                                    WeatherApi.WindCourse(WeatherApi.windDegCity);

                                    if (WeatherApi.ResponseIsNormal == true)
                                    {
                                        var MainWeather = await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<b>{WeatherApi.answer}</b>\n\n<b>Информация города <code>{WeatherApi.nameCity}</code>:</b>\nИндекс Air Pollution💨: <code>{WeatherApi.aqi}</code>\nТемпература города🌡: <code>{Math.Round(WeatherApi.temperatureCity)}°C</code>\nОщущается как: <code>{Math.Round(WeatherApi.tempFeelsLikeCity)}°C</code>\nПогода⛅: <code>{WeatherApi.weatherCity}</code>\nДавление⬇:<code>{WeatherApi.pressureCity} гПа</code>\nВидимость👁: <code>{WeatherApi.visibilityCity} км</code>\nВлажность💧: <code>{WeatherApi.humidityCity}%</code>\nСкорость ветра🌫: <code>{WeatherApi.windSpeedCity} м/c</code>\nНаправление ветра: <code>{WeatherApi.windDegCity}° ({WeatherApi.WindAnswer})</code>", parseMode: ParseMode.Html);
                                        await client.SendTextMessageAsync(
                                            msg.Chat.Id,
                                            replyToMessageId: MainWeather.MessageId,
                                            text:
    @$"<b>AirPollution - Компоненты воздуха:</b>
CO: <i>{WeatherApi.co} мкг/м3</i>
NO: <i>{WeatherApi.no} мкг/м3</i>
NO2: <i>{WeatherApi.no2} мкг/м3</i>
O3: <i>{WeatherApi.o3} мкг/м3</i>
SO2: <i>{WeatherApi.so2} мкг/м3</i>
PM2.5: <i>{WeatherApi.pm2_5} мкг/м3</i>
PM10: <i>{WeatherApi.pm10} мкг/м3</i>
NH3: <i>{WeatherApi.nh3} мкг/м3</i>
",
                                            parseMode: ParseMode.Html);
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<b>Ошибка запроса погоды⚠!</b>\nГород не существует, либо написан не в именительном падеже.\n\n<i>Пример: Погода в Санкт-Петербург</i>", parseMode: ParseMode.Html);
                                    }
                                }

                                //Random % of question
                                if (msg.Text.ToLower().StartsWith("лаура инфа"))
                                {
                                    Random rndm_count = new Random();
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"🤔Я думаю, что это возможно на {rndm_count.Next(0, 100)}%");
                                }

                                if ((msg.Text.StartsWith("Лаура") | msg.Text.StartsWith("лаура")) & (msg.Text.Contains("Скажи") | msg.Text.Contains("скажи")))
                                {
                                    String[] answers = { "Нет, не хочу", "Пошёл ты!", "Я понимаю, что я бот, но я не собираюсь это говорить!", "ДА ИДИ ТЫ НАФИГ!!!", "Хорошо, я это скажу..." };
                                    Random random = new Random();

                                    var myAnswerResult = await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"{answers[random.Next(answers.Length)]}");
                                    var SplitSymbol = msg.Text.Split("кажи");
                                    var phrase = SplitSymbol[+1];
                                    if (myAnswerResult.Text == "Хорошо, я это скажу...")
                                    {
                                        await Task.Delay(2250);
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"{phrase}");
                                        return;
                                    }
                                    else
                                    {
                                        return;
                                    }

                                }

                                //AI Activation
                                if (msg.ReplyToMessage != null && msg.ReplyToMessage.From.Id == myId)
                                {
                                    AIHandler.AIAnswer(msg.Text, msg.From.FirstName);
                                    if (AIHandler.Status != true)
                                    {
                                        return;
                                    }
                                    else if (AIHandler.HaveSticker == true)
                                    {
                                        await client.SendStickerAsync(chatId: msg.Chat.Id, sticker: AIHandler.StickerUrl);
                                        await Task.Delay(1500);
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, AIHandler.AIMessage);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, AIHandler.AIMessage);
                                        return;
                                    }
                                }

                                //Random user (in Dev)
                                if (msg.Text.ToLower().StartsWith("лаура кто сегодня "))
                                {
                                    var splitter = msg.Text.Split(' ');
                                    var SplitPredictPhrase = msg.Text.Split($"{splitter[0]} {splitter[1]} {splitter[2]} ");
                                    var PredictPhrase = SplitPredictPhrase[1];

                                    String[] botText = { "🔮Шар ясно показывает, что", "🌌Древние боги гласят, что" };
                                    Random random_botText = new Random();
                                    var user = await MessageParser.GetRandomMemberAsync(msg.Chat.Id, token, msg.Chat.Type, msg.MessageId);

                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"{botText[random_botText.Next(botText.Length)]} <a href = \"tg://user?id={user[1]}\">{user[0]}</a> сегодня {PredictPhrase}", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                }

                                if ((msg.Text.Contains("чат") | msg.Text.Contains("Чат")) & (msg.Text.Contains("умер") | msg.Text.Contains("Умер") | msg.Text.Contains("сдох") | msg.Text.Contains("Сдох")))
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"F, {msg.Chat.Title}");
                                        return;
                                    }
                                }

                                else if (msg.Chat.Id != msg.From.Id)
                                {
                                    if (msg.From.Id == channel || msg.From.Id == group || msg.From.Id == anonim)
                                    {
                                        return;
                                    }
                                    Connector.Connector.GetAntiSpam(msg.From.Id, msg.Chat.Id);
                                    if (Connector.Connector.message == "not found")
                                    {
                                        Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                        if (Connector.Connector.message == "not found")
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            if (Connector.Connector.as_active == "off")
                                            {
                                                return;
                                            }
                                            else
                                            {
                                                Connector.Connector.CreateAntiSpam(msg.From.Id, msg.Chat.Id, 1);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                        if (Connector.Connector.message == "not found")
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            if (Connector.Connector.as_active == "off")
                                            {
                                                return;
                                            }
                                            else
                                            {
                                                if (Connector.Connector.countMess >= Connector.Connector.maxMess)
                                                {
                                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                                    if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                                    {
                                                        return;
                                                    }
                                                    else if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == msg.From.Id; }) != null)
                                                    {
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        Connector.Connector.DeleteAntiSpam(msg.Chat.Id, msg.From.Id);
                                                        await client.RestrictChatMemberAsync(msg.Chat.Id, msg.From.Id, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false }, DateTime.Now.AddMinutes(30));
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> флудит, я заглушила его на 30 минут🔇", ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                }
                                                else
                                                {
                                                    var max = Connector.Connector.countMess + 1;
                                                    Connector.Connector.UpdateAntiSpam(msg.Chat.Id, msg.From.Id, max);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (msg.Text.ToLower() == "-нотиф коммент")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(chatId: msg.Chat.Id);
                                    if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == myId; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"Я не являюсь администратором чата🙅🏻‍♀️!", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else if (admins.FirstOrDefault(a => { return a.User != null && a.User.Id == msg.From.Id; }) == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, вы не являетесь администратором/создателем чата!»", disableWebPagePreview: true, parseMode: ParseMode.Html);
                                        return;
                                    }
                                    else
                                    {
                                        if(Connector.Connector.GetNotifComment(msg.Chat.Id) == null)
                                        {
                                            await client.SendTextMessageAsync(msg.Chat.Id, "⛔Комментарий уведомлений не был задан!");
                                        }
                                        else
                                        {
                                            Connector.Connector.DeleteNotifComment(msg.Chat.Id);
                                            await client.SendTextMessageAsync(msg.Chat.Id, "✅Комментарий уведомлений успешно удален!");
                                        }
                                    }
                                }

                                //RP commands
                                if (msg.Text.StartsWith("+"))
                                {
                                    {
                                        if (msg.Chat.Id == msg.From.Id)
                                        {
                                            return;
                                        }
                                        else if (msg.ReplyToMessage == null)
                                        {
                                            return;
                                        }
                                        else if (msg.ReplyToMessage != null)
                                        {
                                            if (msg.ReplyToMessage.From.Id == msg.From.Id)
                                            {
                                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, а у вас, как я вижу, высокая самооценка😏", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                return;
                                            }
                                            else
                                            {
                                                Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                if (Connector.Connector.message == "don't rate")
                                                {
                                                    Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                    if (Connector.Connector.message == "not rated")
                                                    {
                                                        Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, 1);
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: 1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                    else
                                                    {
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        var rating = Connector.Connector.user_rating + 1;
                                                        Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                }
                                                else
                                                {
                                                    Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                    if (Connector.Connector.message == "not rated")
                                                    {
                                                        Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, 1);
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: 1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                    else
                                                    {
                                                        Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                        if (Connector.Connector.count_control >= 4)
                                                        {
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"🚫Вы уже исчерпали лимит оценивания <b>{msg.ReplyToMessage.From.FirstName}</b>\n⏱Дата сброса лимита: {Connector.Connector.soc_control}", ParseMode.Html);
                                                        }
                                                        else
                                                        {
                                                            var control = Connector.Connector.count_control + 1;
                                                            Connector.Connector.UpdateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, control, DateTime.Now.AddDays(1));
                                                            var rating = Connector.Connector.user_rating + 1;
                                                            Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (msg.Text.StartsWith("-"))
                                {
                                    {
                                        if (msg.Chat.Id == msg.From.Id)
                                        {
                                            return;
                                        }
                                        else if (msg.ReplyToMessage == null)
                                        {
                                            return;
                                        }
                                        else if (msg.ReplyToMessage != null)
                                        {
                                            if (msg.Text.Contains("_") || msg.Text.ToLower().StartsWith("-варн"))
                                            {
                                                return;
                                            }
                                            else if (msg.ReplyToMessage.From.Id == msg.From.Id)
                                            {
                                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, зачем себя так унижать😕", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                return;
                                            }
                                            else
                                            {
                                                Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                if (Connector.Connector.message == "don't rate")
                                                {
                                                    Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                    if (Connector.Connector.message == "not rated")
                                                    {
                                                        Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, -1);
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: -1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                    else
                                                    {
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        var rating = Connector.Connector.user_rating - 1;
                                                        Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                }
                                                else
                                                {
                                                    Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                    if (Connector.Connector.message == "not rated")
                                                    {
                                                        Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, -1);
                                                        Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: -1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                    else
                                                    {
                                                        Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                        if (Connector.Connector.count_control >= 4)
                                                        {
                                                            await client.SendTextMessageAsync(msg.Chat.Id, $"🚫Вы уже исчерпали лимит оценивания <b>{msg.ReplyToMessage.From.FirstName}</b>\n⏱Дата сброса лимита: {Connector.Connector.soc_control}", ParseMode.Html);
                                                        }
                                                        else
                                                        {
                                                            var control = Connector.Connector.count_control + 1;
                                                            Connector.Connector.UpdateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, control, DateTime.Now.AddDays(1));
                                                            var rating = Connector.Connector.user_rating - 1;
                                                            Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        return;
                                    }
                                }


                                if (msg.Text.ToUpper() == "ОБНЯТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> обнял себя🤗", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id{msg.From.Id}\">{msg.From.FirstName}</a> обнял <a href = \"tg://user?id{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🤗", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }

                                }

                                if (msg.Text.ToUpper() == "УДАРИТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> ударил со всей силой в пустоту😶", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> ударил со всей силой в <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🤕", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                }

                                if (msg.Text.ToUpper() == "УБИТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> покончил свою жизнь самоубийством🤡🔪", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> убивает <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🔪😢", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }

                                }

                                if (msg.Text.ToUpper() == "УКУСИТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> укусил себя🤡", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> делает укус <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🐺", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }

                                }

                                if (msg.Text.ToUpper() == "ПОКАЗАТЬ ЯЗЫК")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> просто показал язык👅", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?sid={msg.From.Id}\">{msg.From.FirstName}</a> показал язык <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>😜", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }

                                }

                                if (msg.Text.ToUpper() == "НАКОРМИТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> вкусно покормил себя😋", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> вкусно покормил участника <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🍔🍟🌭", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }

                                if (msg.Text.ToUpper() == "КАСТРИРОВАТЬ")
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> себя кастрировал🫡", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }
                                    else
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> кастрировал <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>✂️", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }

                                //Buttons/easters
                                switch (msg.Text)
                                {

                                    //Just button
                                    case "VPN":
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "<b>FBA Studio x LibreNet</b>\n\n<b><i>Доступные сервера:</i></b>\n<i>LanceVPN_Netherlands:</i>\n¬Скорость скачивания: 250 МБит/с\n¬Страна: Нидерланды\n¬Цена: 50 Руб/мес\nПротокол: WireGuard", parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Прайслист", "https://t.me/LanceVPN")));
                                            break;
                                        }

                                    //Buttons from "GetButtons"
                                    case "Инструкция📚":
                                        if (msg.Chat.Id != msg.From.Id)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Инструкция для работы с телеграм ботом:\ntelegra.ph/Rabota-s-botom-Laura-10-18", parseMode: ParseMode.Html, disableWebPagePreview: false);
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
                                                "<b href=\"t.me/FBA_Studio\">FBA Studio</b> - это команда разработчиков, которые делают Телеграм ботов с нужным функционалом без обязательной рекламы, они настроены больше всего на мнение комьюнити, думаю, тебе стоит посетить их канал🤗",
                                                replyMarkup: InfoKeyboard, parseMode: ParseMode.Html);
                                            break;
                                        }


                                    case "Добавить бота в чат➕":
                                        if (msg.Chat.Id != msg.From.Id)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, text: $"<b>❗Права, необходимые для модерировани бота:</b>\n\n<i>-измененния прав участников чата</i>\n<i>-удаление участников из чата</i>\n<i>-удаление чужих сообщений</i>\n<i>Изменения разрешений чата</i>", parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Добавить бота в группу", "http://t.me/Laura_cm_bot?startgroup=start&admin=change_info+restrict_members+delete_messages+pin_messages+invite_users")));
                                            break;
                                        }

                                    //Sponsors of the bot
                                    case "Партнёры бота":
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, text: "<b>Наши партнёры бота🤝:</b>\n<i>-@FlushaStudio(Использование бота)</i>\n<i>-@RiceTeamStudio(Пиар проекта)</i>\n<i>-@banan4ikmoder(Пиар)</i>\n<i>@TheShadow_hk(Dev, разработал проверку статуса участника для корректного бана в чате)</i>\n<i>Maxim Bysh(Помог с разработкой мута по заданному времени)</i>\n<i>Libreto(Предоставления сервера)</i>\n<i>Список обновляется</i>", parseMode: ParseMode.Html);
                                        break;
                                }

                                //Get Chat Id (For admins)
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

                                //Get Uptime
                                if (msg.Text.ToLower().StartsWith("/uptime"))
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, $"Мой Uptime:\n{timeOfStart}");
                                }

                                //Get your id 
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

                                //Get our id
                                if (msg.Text.StartsWith("/getourid"))
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        if (msg.ReplyToMessage == null)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Вы не указали пользователя!");
                                        }
                                        else
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"ID пользователя {msg.ReplyToMessage.From.FirstName}:\n<code>{msg.ReplyToMessage.From.Id}</code>", parseMode: ParseMode.Html);
                                        }

                                    }
                                }

                                //Random rating
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

                                //Weather request from WeatherClassApi.cs
                                if (msg.Text.ToLower().StartsWith("погода в"))
                                {
                                    string[] strings = msg.Text.Split(' ');
                                    String[] InpResponse = msg.Text.Split(strings[0] + ' ' + strings[1] + ' ');
                                    var inputCityName = InpResponse[1];
                                    WeatherApi.Weather(inputCityName);
                                    WeatherApi.Celsius(celsius: WeatherApi.temperatureCity);
                                    WeatherApi.WindCourse(WeatherApi.windDegCity);

                                    if (WeatherApi.ResponseIsNormal == true)
                                    {
                                        var MainWeather = await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<b>{WeatherApi.answer}</b>\n\n<b>Информация города <code>{WeatherApi.nameCity}</code>:</b>\nИндекс Air Pollution💨: <code>{WeatherApi.aqi}</code>\nТемпература города🌡: <code>{Math.Round(WeatherApi.temperatureCity)}°C</code>\nОщущается как: <code>{Math.Round(WeatherApi.tempFeelsLikeCity)}°C</code>\nПогода⛅: <code>{WeatherApi.weatherCity}</code>\nДавление⬇:<code>{WeatherApi.pressureCity} гПа</code>\nВидимость👁: <code>{WeatherApi.visibilityCity} км</code>\nВлажность💧: <code>{WeatherApi.humidityCity}%</code>\nСкорость ветра🌫: <code>{WeatherApi.windSpeedCity} м/c</code>\nНаправление ветра: <code>{WeatherApi.windDegCity}° ({WeatherApi.WindAnswer})</code>", parseMode: ParseMode.Html);
                                        await client.SendTextMessageAsync(
                                            msg.Chat.Id,
                                            replyToMessageId: MainWeather.MessageId,
                                            text:
    @$"<b>AirPollution - Компоненты воздуха:</b>
CO: <i>{WeatherApi.co} мкг/м3</i>
NO: <i>{WeatherApi.no} мкг/м3</i>
NO2: <i>{WeatherApi.no2} мкг/м3</i>
O3: <i>{WeatherApi.o3} мкг/м3</i>
SO2: <i>{WeatherApi.so2} мкг/м3</i>
PM2.5: <i>{WeatherApi.pm2_5} мкг/м3</i>
PM10: <i>{WeatherApi.pm10} мкг/м3</i>
NH3: <i>{WeatherApi.nh3} мкг/м3</i>
",
                                            parseMode: ParseMode.Html);
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<b>Ошибка запроса погоды⚠!</b>\nГород не существует, либо написан не в именительном падеже.\n\n<i>Пример: Погода в Санкт-Петербург</i>", parseMode: ParseMode.Html);
                                    }
                                }

                                //Random % of question
                                if (msg.Text.ToLower().StartsWith("лаура инфа"))
                                {
                                    Random rndm_count = new Random();
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"🤔Я думаю, что это возможно на {rndm_count.Next(0, 100)}%");
                                }

                                if ((msg.Text.StartsWith("Лаура") | msg.Text.StartsWith("лаура")) & (msg.Text.Contains("Скажи") | msg.Text.Contains("скажи")))
                                {
                                    String[] answers = { "Нет, не хочу", "Пошёл ты!", "Я понимаю, что я бот, но я не собираюсь это говорить!", "ДА ИДИ ТЫ НАФИГ!!!", "Хорошо, я это скажу..." };
                                    Random random = new Random();

                                    var myAnswerResult = await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"{answers[random.Next(answers.Length)]}");
                                    var SplitSymbol = msg.Text.Split("кажи");
                                    var phrase = SplitSymbol[+1];
                                    if (myAnswerResult.Text == "Хорошо, я это скажу...")
                                    {
                                        await Task.Delay(2250);
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"{phrase}");
                                        return;
                                    }
                                    else
                                    {
                                        return;
                                    }

                                }

                                //AI Activation
                                if (msg.ReplyToMessage != null && msg.ReplyToMessage.From.Id == myId)
                                {
                                    AIHandler.AIAnswer(msg.Text, msg.From.FirstName);
                                    if (AIHandler.Status != true)
                                    {
                                        return;
                                    }
                                    else if (AIHandler.HaveSticker == true)
                                    {
                                        await client.SendStickerAsync(chatId: msg.Chat.Id, sticker: AIHandler.StickerUrl);
                                        await Task.Delay(1500);
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, AIHandler.AIMessage);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, AIHandler.AIMessage);
                                        return;
                                    }
                                }

                                //Random user (in Dev)
                                if (msg.Text.ToLower().StartsWith("лаура кто сегодня "))
                                {
                                    var splitter = msg.Text.Split(' ');
                                    var SplitPredictPhrase = msg.Text.Split($"{splitter[0]} {splitter[1]} {splitter[2]} ");
                                    var PredictPhrase = SplitPredictPhrase[1];

                                    String[] botText = { "🔮Шар ясно показывает, что", "🌌Древние боги гласят, что" };
                                    Random random_botText = new Random();
                                    var user = await MessageParser.GetRandomMemberAsync(msg.Chat.Id, token, msg.Chat.Type, msg.MessageId);

                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"{botText[random_botText.Next(botText.Length)]} <a href = \"tg://user?id={user[1]}\">{user[0]}</a> сегодня {PredictPhrase}", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                }

                                if ((msg.Text.Contains("чат") | msg.Text.Contains("Чат")) & (msg.Text.Contains("умер") | msg.Text.Contains("Умер") | msg.Text.Contains("сдох") | msg.Text.Contains("Сдох")))
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"F, {msg.Chat.Title}");
                                        return;
                                    }
                                }

                                else if (msg.Chat.Id != msg.From.Id)
                                {
                                    if (msg.From.Id == channel || msg.From.Id == group || msg.From.Id == anonim)
                                    {
                                        return;
                                    }
                                    Connector.Connector.GetAntiSpam(msg.From.Id, msg.Chat.Id);
                                    if (Connector.Connector.message == "not found")
                                    {
                                        Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                        if (Connector.Connector.message == "not found")
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            if (Connector.Connector.as_active == "off")
                                            {
                                                return;
                                            }
                                            else
                                            {
                                                Connector.Connector.CreateAntiSpam(msg.From.Id, msg.Chat.Id, 1);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                        if (Connector.Connector.message == "not found")
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            if (Connector.Connector.as_active == "off")
                                            {
                                                return;
                                            }
                                            else
                                            {
                                                if (Connector.Connector.countMess >= Connector.Connector.maxMess)
                                                {
                                                    ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                                    if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                                    {
                                                        return;
                                                    }
                                                    else if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == msg.From.Id; }) != null)
                                                    {
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        Connector.Connector.DeleteAntiSpam(msg.Chat.Id, msg.From.Id);
                                                        await client.RestrictChatMemberAsync(msg.Chat.Id, msg.From.Id, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false }, DateTime.Now.AddMinutes(30));
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> флудит, я заглушила его на 30 минут🔇", ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                }
                                                else
                                                {
                                                    var max = Connector.Connector.countMess + 1;
                                                    Connector.Connector.UpdateAntiSpam(msg.Chat.Id, msg.From.Id, max);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //RP commands
                            if (msg.Text.StartsWith("+"))
                            {
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage != null)
                                    {
                                        if (msg.ReplyToMessage.From.Id == msg.From.Id)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, а у вас, как я вижу, высокая самооценка😏", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                            return;
                                        }
                                        else
                                        {
                                            Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                            if (Connector.Connector.message == "don't rate")
                                            {
                                                Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                if (Connector.Connector.message == "not rated")
                                                {
                                                    Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, 1);
                                                    Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: 1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                }
                                                else
                                                {
                                                    Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                    var rating = Connector.Connector.user_rating + 1;
                                                    Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                }
                                            }
                                            else
                                            {
                                                Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                if (Connector.Connector.message == "not rated")
                                                {
                                                    Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, 1);
                                                    Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: 1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                }
                                                else
                                                {
                                                    Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                    if (Connector.Connector.count_control >= 4)
                                                    {
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"🚫Вы уже исчерпали лимит оценивания <b>{msg.ReplyToMessage.From.FirstName}</b>\n⏱Дата сброса лимита: {Connector.Connector.soc_control}", ParseMode.Html);
                                                    }
                                                    else
                                                    {
                                                        var control = Connector.Connector.count_control + 1;
                                                        Connector.Connector.UpdateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, control, DateTime.Now.AddDays(1));
                                                        var rating = Connector.Connector.user_rating + 1;
                                                        Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>✅\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (msg.Text.StartsWith("-"))
                            {
                                {
                                    if (msg.Chat.Id == msg.From.Id)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage == null)
                                    {
                                        return;
                                    }
                                    else if (msg.ReplyToMessage != null)
                                    {
                                        if (msg.Text.Contains("_") || msg.Text.ToLower().StartsWith("-варн"))
                                        {
                                            return;
                                        }
                                        else if (msg.ReplyToMessage.From.Id == msg.From.Id)
                                        {
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a>, зачем себя так унижать😕", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                            return;
                                        }
                                        else
                                        {
                                            Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                            if (Connector.Connector.message == "don't rate")
                                            {
                                                Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                if (Connector.Connector.message == "not rated")
                                                {
                                                    Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, -1);
                                                    Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: -1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                }
                                                else
                                                {
                                                    Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                    var rating = Connector.Connector.user_rating - 1;
                                                    Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                }
                                            }
                                            else
                                            {
                                                Connector.Connector.GetSocialData(msg.ReplyToMessage.From.Id);
                                                if (Connector.Connector.message == "not rated")
                                                {
                                                    Connector.Connector.CreateSocialData(msg.ReplyToMessage.From.Id, -1);
                                                    Connector.Connector.CreateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, 1, DateTime.Now.AddDays(1));
                                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: -1", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                }
                                                else
                                                {
                                                    Connector.Connector.GetControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id);
                                                    if (Connector.Connector.count_control >= 4)
                                                    {
                                                        await client.SendTextMessageAsync(msg.Chat.Id, $"🚫Вы уже исчерпали лимит оценивания <b>{msg.ReplyToMessage.From.FirstName}</b>\n⏱Дата сброса лимита: {Connector.Connector.soc_control}", ParseMode.Html);
                                                    }
                                                    else
                                                    {
                                                        var control = Connector.Connector.count_control + 1;
                                                        Connector.Connector.UpdateControlSocData(msg.From.Id, msg.ReplyToMessage.From.Id, control, DateTime.Now.AddDays(1));
                                                        var rating = Connector.Connector.user_rating - 1;
                                                        Connector.Connector.UpdateSocialData(msg.ReplyToMessage.From.Id, rating);
                                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName} {msg.From.LastName}</a> не соглашается с участником <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName} {msg.ReplyToMessage.From.LastName}</a>💢\n\nСоциальный рейтинг: {rating}📊", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    return;
                                }
                            }


                            if (msg.Text.ToUpper() == "ОБНЯТЬ")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> обнял себя🤗", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id{msg.From.Id}\">{msg.From.FirstName}</a> обнял <a href = \"tg://user?id{msg.ReplyToMessage.From.Username}\">{msg.ReplyToMessage.From.FirstName}</a>🤗", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }

                            }

                            if (msg.Text.ToUpper() == "УДАРИТЬ")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> ударил со всей силой в пустоту😶", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> ударил со всей силой в <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🤕", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }
                            }

                            if (msg.Text.ToUpper() == "УБИТЬ")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> покончил свою жизнь самоубийством🤡🔪", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> убивает <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🔪😢", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }

                            }

                            if (msg.Text.ToUpper() == "УКУСИТЬ")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> укусил себя🤡", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> делает укус <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🐺", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }

                            }

                            if (msg.Text.ToUpper() == "ПОКАЗАТЬ ЯЗЫК")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> просто показал язык👅", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?sid={msg.From.Id}\">{msg.From.FirstName}</a> показал язык <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>😜", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }

                            }

                            if (msg.Text.ToUpper() == "НАКОРМИТЬ")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> вкусно покормил себя😋", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }
                                else
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> вкусно покормил участника <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>🍔🍟🌭", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                return;
                            }

                            if (msg.Text.ToUpper() == "КАСТРИРОВАТЬ")
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else if (msg.ReplyToMessage == null)
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> себя кастрировал🫡", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                    return;
                                }
                                else
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<a href = \"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> кастрировал <a href = \"tg://user?id={msg.ReplyToMessage.From.Id}\">{msg.ReplyToMessage.From.FirstName}</a>✂️", parseMode: ParseMode.Html, disableWebPagePreview: true);
                                return;
                            }

                            //Buttons/easters
                            switch (msg.Text)
                            {

                                //Just button
                                case "VPN":
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, "<b>FBA Studio x LibreNet</b>\n\n<b><i>Доступные сервера:</i></b>\n<i>LanceVPN_Netherlands:</i>\n¬Скорость скачивания: 250 МБит/с\n¬Страна: Нидерланды\n¬Цена: 50 Руб/мес\nПротокол: WireGuard", parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Прайслист", "https://t.me/LanceVPN")));
                                        break;
                                    }

                                //Buttons from "GetButtons"
                                case "Инструкция📚":
                                    if (msg.Chat.Id != msg.From.Id)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Инструкция для работы с телеграм ботом:\ntelegra.ph/Rabota-s-botom-Laura-10-18", parseMode: ParseMode.Html, disableWebPagePreview: false);
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
                                            "<b href=\"t.me/FBA_Studio\">FBA Studio</b> - это команда разработчиков, которые делают Телеграм ботов с нужным функционалом без обязательной рекламы, они настроены больше всего на мнение комьюнити, думаю, тебе стоит посетить их канал🤗",
                                            replyMarkup: InfoKeyboard, parseMode: ParseMode.Html);
                                        break;
                                    }


                                case "Добавить бота в чат➕":
                                    if (msg.Chat.Id != msg.From.Id)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, text: $"<b>❗Права, необходимые для модерировани бота:</b>\n\n<i>-измененния прав участников чата</i>\n<i>-удаление участников из чата</i>\n<i>-удаление чужих сообщений</i>\n<i>Изменения разрешений чата</i>", parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Добавить бота в группу", "http://t.me/Laura_cm_bot?startgroup=Laura&admin=change_info+restrict_members+delete_messages+pin_messages+invite_users")));
                                        break;
                                    }

                                //Sponsors of the bot
                                case "Партнёры бота":
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, text: "<b>Наши партнёры бота🤝:</b>\n<i>-@FlushaStudio(Использование бота)</i>\n<i>-@RiceTeamStudio(Пиар проекта)</i>\n<i>-@banan4ikmoder(Пиар)</i>\n<i>@TheShadow_hk(Dev, разработал проверку статуса участника для корректного бана в чате)</i>\n<i>Maxim Bysh(Помог с разработкой мута по заданному времени)</i>\n<i>Libreto(Предоставления сервера)</i>\n<i>Список обновляется</i>", parseMode: ParseMode.Html);
                                    break;
                            }

                            //Get Chat Id (For admins)
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
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"⛔<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a>, вы не являетесь администратором/создателем чата!", ParseMode.Html, disableWebPagePreview: true);
                                        return;
                                    }

                                    else
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"ID данного чата🆔: <code>{msg.Chat.Id}</code>", parseMode: ParseMode.Html); ;
                                    return;
                                }
                            }

                            //Get Uptime
                            if (msg.Text.ToLower().StartsWith("/uptime"))
                            {
                                await client.SendTextMessageAsync(msg.Chat.Id, $"Мой Uptime:\n{timeOfStart}");
                            }

                            //Get your id 
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

                            //Get our id
                            if (msg.Text.StartsWith("/getourid"))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    if (msg.ReplyToMessage == null)
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, "Вы не указали пользователя!");
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"ID пользователя {msg.ReplyToMessage.From.FirstName}:\n<code>{msg.ReplyToMessage.From.Id}</code>", parseMode: ParseMode.Html);
                                    }

                                }
                            }

                            //Random rating
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

                            //Weather request from WeatherClassApi.cs
                            if (msg.Text.ToLower().StartsWith("погода в"))
                            {
                                string[] strings = msg.Text.Split(' ');
                                String[] InpResponse = msg.Text.Split(strings[0] + ' ' + strings[1] + ' ');
                                var inputCityName = InpResponse[1];
                                WeatherApi.Weather(inputCityName);
                                WeatherApi.Celsius(celsius: WeatherApi.temperatureCity);
                                WeatherApi.WindCourse(WeatherApi.windDegCity);

                                if (WeatherApi.ResponseIsNormal == true)
                                {
                                    var MainWeather = await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<b>{WeatherApi.answer}</b>\n\n<b>Информация города <code>{WeatherApi.nameCity}</code>:</b>\nИндекс Air Pollution💨: <code>{WeatherApi.aqi}</code>\nТемпература города🌡: <code>{Math.Round(WeatherApi.temperatureCity)}°C</code>\nОщущается как: <code>{Math.Round(WeatherApi.tempFeelsLikeCity)}°C</code>\nПогода⛅: <code>{WeatherApi.weatherCity}</code>\nДавление⬇:<code>{WeatherApi.pressureCity} гПа</code>\nВидимость👁: <code>{WeatherApi.visibilityCity} км</code>\nВлажность💧: <code>{WeatherApi.humidityCity}%</code>\nСкорость ветра🌫: <code>{WeatherApi.windSpeedCity} м/c</code>\nНаправление ветра: <code>{WeatherApi.windDegCity}° ({WeatherApi.WindAnswer})</code>", parseMode: ParseMode.Html);
                                    await client.SendTextMessageAsync(
                                        msg.Chat.Id,
                                        replyToMessageId: MainWeather.MessageId,
                                        text:
@$"<b>AirPollution - Компоненты воздуха:</b>
CO: <i>{WeatherApi.co} мкг/м3</i>
NO: <i>{WeatherApi.no} мкг/м3</i>
NO2: <i>{WeatherApi.no2} мкг/м3</i>
O3: <i>{WeatherApi.o3} мкг/м3</i>
SO2: <i>{WeatherApi.so2} мкг/м3</i>
PM2.5: <i>{WeatherApi.pm2_5} мкг/м3</i>
PM10: <i>{WeatherApi.pm10} мкг/м3</i>
NH3: <i>{WeatherApi.nh3} мкг/м3</i>
",
                                        parseMode: ParseMode.Html);
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"<b>Ошибка запроса погоды⚠!</b>\nГород не существует, либо написан не в именительном падеже.\n\n<i>Пример: Погода в Санкт-Петербург</i>", parseMode: ParseMode.Html);
                                }
                            }

                            //Random % of question
                            if (msg.Text.ToLower().StartsWith("лаура инфа"))
                            {
                                Random rndm_count = new Random();
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"🤔Я думаю, что это возможно на {rndm_count.Next(0, 100)}%");
                            }

                            if ((msg.Text.StartsWith("Лаура") | msg.Text.StartsWith("лаура")) & (msg.Text.Contains("Скажи") | msg.Text.Contains("скажи")))
                            {
                                String[] answers = { "Нет, не хочу", "Пошёл ты!", "Я понимаю, что я бот, но я не собираюсь это говорить!", "ДА ИДИ ТЫ НАФИГ!!!", "Хорошо, я это скажу..." };
                                Random random = new Random();

                                var myAnswerResult = await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"{answers[random.Next(answers.Length)]}");
                                var SplitSymbol = msg.Text.Split("кажи");
                                var phrase = SplitSymbol[+1];
                                if (myAnswerResult.Text == "Хорошо, я это скажу...")
                                {
                                    await Task.Delay(2250);
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"{phrase}");
                                    return;
                                }
                                else
                                {
                                    return;
                                }

                            }

                            //AI Activation
                            if (msg.ReplyToMessage != null && msg.ReplyToMessage.From.Id == myId)
                            {
                                AIHandler.AIAnswer(msg.Text, msg.From.FirstName);
                                if (AIHandler.Status != true)
                                {
                                    return;
                                }
                                else if (AIHandler.HaveSticker == true)
                                {
                                    await client.SendStickerAsync(chatId: msg.Chat.Id, sticker: AIHandler.StickerUrl);
                                    await Task.Delay(1500);
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, AIHandler.AIMessage);
                                    return;
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, AIHandler.AIMessage);
                                    return;
                                }
                            }

                            //Random user (in Dev)
                            //if (msg.Text.ToLower().StartsWith("лаура кто сегодня "))
                            //{
                            //    var splitter = msg.Text.Split(' ');
                            //    var SplitPredictPhrase = msg.Text.Split($"{splitter[0]} {splitter[1]} {splitter[2]} ");
                            //    var PredictPhrase = SplitPredictPhrase[1];

                            //    String[] botText = { "🔮Шар ясно показывает, что", "🌌Легенды гласят, что", "💡Один мудрец гласит, что", "👨‍💻Один из сотрудников FBA говорит, что" };
                            //    Random random_botText = new Random();
                            //    var user = await MessageParser.GetRandomMemberAsync(msg.Chat.Id, token, msg.Chat.Type, msg.MessageId);

                            //    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"{botText[random_botText.Next(botText.Length)]} <a href = \"tg://user?id={user[1]}\">{user[0]}</a> сегодня {PredictPhrase}", parseMode: ParseMode.Html, disableWebPagePreview: true);
                            //}

                            if ((msg.Text.Contains("чат") | msg.Text.Contains("Чат")) & (msg.Text.Contains("умер") | msg.Text.Contains("Умер") | msg.Text.Contains("сдох") | msg.Text.Contains("Сдох")))
                            {
                                if (msg.Chat.Id == msg.From.Id)
                                {
                                    return;
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id, $"F, {msg.Chat.Title}");
                                    return;
                                }
                            }

                            else if (msg.Chat.Id != msg.From.Id)
                            {
                                if(msg.From.Id == channel || msg.From.Id == group || msg.From.Id == anonim)
                                {
                                    return;
                                }
                                Connector.Connector.GetAntiSpam(msg.From.Id, msg.Chat.Id);
                                if (Connector.Connector.message == "not found")
                                {
                                    Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                    if (Connector.Connector.message == "not found")
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        if (Connector.Connector.as_active == "off")
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            Connector.Connector.CreateAntiSpam(msg.From.Id, msg.Chat.Id, 1);
                                        }
                                    }
                                }
                                else
                                {
                                    Connector.Connector.GetSpamConfig(msg.Chat.Id);
                                    if (Connector.Connector.message == "not found")
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        if (Connector.Connector.as_active == "off")
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            if (Connector.Connector.countMess >= Connector.Connector.maxMess)
                                            {
                                                ChatMember[] admins = await client.GetChatAdministratorsAsync(msg.Chat.Id);
                                                if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == myId; }) == null)
                                                {
                                                    return;
                                                }
                                                else if (admins.FirstOrDefault(a => { return a.User.Id != null && a.User.Id == msg.From.Id; }) != null)
                                                {
                                                    return;
                                                }
                                                else
                                                {
                                                    Connector.Connector.DeleteAntiSpam(msg.Chat.Id, msg.From.Id);
                                                    await client.RestrictChatMemberAsync(msg.Chat.Id, msg.From.Id, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false, CanSendOtherMessages = false }, DateTime.Now.AddMinutes(30));
                                                    await client.SendTextMessageAsync(msg.Chat.Id, $"<a href=\"tg://user?id={msg.From.Id}\">{msg.From.FirstName}</a> флудит, я заглушила его на 30 минут🔇", ParseMode.Html, disableWebPagePreview: true);
                                                }
                                            }
                                            else
                                            {
                                                var max = Connector.Connector.countMess + 1;
                                                Connector.Connector.UpdateAntiSpam(msg.Chat.Id, msg.From.Id, max);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                    catch (ApiRequestException exc)
                    {
                        Console.WriteLine(exc.ToString());
                        return;
                    }
                    catch (FormatException exc2)
                    {
                        Console.WriteLine(exc2.ToString());
                        return;
                    }
                    catch (MySqlException exc3)
                    {
                        Console.WriteLine($"Error occured: {exc3.Message}");
                        return;
                    }
                }

            }


        }

        private static IReplyMarkup GetButtons() => new ReplyKeyboardMarkup
        {
            ResizeKeyboard = true,
            Keyboard = new List<List<KeyboardButton>>
            {
                    new List<KeyboardButton>{new KeyboardButton{ Text = "Инструкция📚" }, new KeyboardButton { Text = "О нас📒"}, },
                    new List<KeyboardButton>{new KeyboardButton{ Text = "Добавить бота в чат➕"}, new KeyboardButton { Text = "VPN" } }
            }
        };
    }
}
