using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIClass
{
    class AIHandler
    {

        public static string AIMessage { get; set; }

        public static bool Status { get; set; }

        public static int Negative { get; set; } = 0;

        public static int ResetNegative { get; set; } = 30;

        public static string StickerUrl { get; set; }

        public static bool HaveSticker { get; set; }

        public static void AIAnswer(string Message, string firstName)
        {
            Random random = new Random();
            Message = Message.ToUpper();
            if(Message == null)
            {
                Status = false;
                HaveSticker = false;
                return;
            }

            else if(Negative == 60)
            {
                if (ResetNegative > 0)
                {
                    string[] Answers = { "Отстаньте, мне всё это достало...", "Оставь меня одной...", "В чате такие токсики, что мне противно говорить...", "Дай мне покой от этого всего...", "Поскорее бы дождаться техничего перерыва, чтобы не страдать больше в этом чате..."};
                    AIMessage = Answers[random.Next(Answers.Length)];
                    ResetNegative--;
                    Status = true;
                    HaveSticker = true;
                }
                else
                {
                    Negative = 0;
                    ResetNegative = 30;
                    Status = false;
                    HaveSticker = false;
                }
            }

            else
            {
                if (Message.Contains("ПРИВЕТ"))
                {
                    if(Message.Contains("ШЛЮХА"))
                    {
                        string[] Answers = { "За что так со мной обращаетесь? Я же Вас даже не трогала😕", "Ну а что я такого сделала?", "Ваша клоунская манера речи меня раздражает." };
                        AIMessage = Answers[random.Next(Answers.Length)];
                        HaveSticker = false;
                        Status = true;
                        Negative++;
                    }
                    else
                    {
                        string[] Answers = { $"Привет, {firstName}!", $"Здравствуй, {firstName}!", "Привет-привет!", $"Привет, {firstName}! Как дела?" };
                        AIMessage = Answers[random.Next(Answers.Length)];
                        HaveSticker = false;
                        Status = true;
                    }
                }
                else if(Message.Contains("КАК ДЕЛА"))
                {
                    string[] Answers = { "Нормально. А у тебя?", "Ничего. А у тебя как?" };
                    AIMessage = Answers[random.Next(Answers.Length)];
                    HaveSticker = false;
                    Status = true;
                }
                else if(Message.Contains("ДУРА"))
                {
                    string[] Answers = { "Да хватит!", "На себя в зеркало посмотрел?", "Ну хватит!"};
                    AIMessage = Answers[random.Next(Answers.Length)];
                    HaveSticker = false;
                    Status = true;
                    Negative++;
                }
                else if(Message.Contains("МИЛАЯ"))
                {
                    string[] Answers = { "Взаимно!", "Спасибо😊", "Это мне..." };
                    AIMessage = Answers[random.Next(Answers.Length)];
                    StickerUrl = "https://t.me/ScladOfRes/71";
                    HaveSticker = true;
                    Status = true;
                }
                else
                {
                    HaveSticker = false;
                    Status = false;
                }
            }
        }
    }
}
