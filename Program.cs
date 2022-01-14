using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.PollOption
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;

namespace Quiz_Bot
{
    class Program
    {

        private static TelegramBotClient? Bot;


        static Random rng = new Random();
        static int result = 0;






        public static async Task Main()
        {
            Bot = new TelegramBotClient("5092708636:AAHLr5HeTCIUER0rYWCujyWxg72VYCxe2ik");

            User me = await Bot.GetMeAsync();
            Console.Title = me.Username ?? "Quiz Bot";
            using var cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Bot.StartReceiving(HandleUpdateAsync,
                               HandleErrorAsync,
                               receiverOptions,
                               cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }



        


        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }


        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }



        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            


            var action = message.Text switch
            {
                "/Ask_me" => Ask(botClient, message),
                "/help" or "/start" => help(botClient, message),
                _ => help(botClient, message)
            };
            Message sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");


            static async Task<Message> help(ITelegramBotClient botClient, Message message)
            {

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text:
                                                                  "/help - Get help\n" +
                                                                  "/Ask_me - Ask a new question"
                                                            );


            }

                static async Task<Message> Ask(ITelegramBotClient botClient, Message message)
                {

                var json = new WebClient().DownloadString("https://opentdb.com/api.php?amount=1");
                var jsonData = JsonConvert.DeserializeObject<Root>(json);
                string correct_answer ="";
                string question_category = "";
                List<string> answers_list = new List<string>();
                string question = "";

                if (jsonData.response_code == 0)
                {
                    foreach (var i in jsonData.results)
                    {
                        question = i.question;
                        correct_answer = i.correct_answer;
                        answers_list = i.incorrect_answers;
                        question_category = i.category;
                    }

           

                }
                answers_list.Add(correct_answer);
                answers_list = Shuffle(answers_list);
                string O1 = answers_list[0];
                string O2 = answers_list[1];
                string O3 = answers_list[2];
                string O4 = answers_list[3];
                int correct_answers_index = answers_list.IndexOf(correct_answer);

                Message pollMessage = await botClient.SendPollAsync(
                        chatId: message.Chat.Id,
                        type: PollType.Quiz,
                        question: question,
                        explanation: "Question Category: " + question_category,
                        options: new[]
                        {O1,O1,O2,O3},
                        allowsMultipleAnswers: false,
                        correctOptionId: correct_answers_index
                        );

                int x = (int)pollMessage.Poll.CorrectOptionId;

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text:
                                                                  "/help - Get help\n" +
                                                                  "/Ask_me - Ask a new question"
                                                            );


            }



    }
        public static List<string> Shuffle(List<string> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }


    }


        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Result
        {
            public string category { get; set; }
            public string type { get; set; }
            public string difficulty { get; set; }
            public string question { get; set; }
            public string correct_answer { get; set; }
            public List<string> incorrect_answers { get; set; }
        }

        public class Root
        {
            public int response_code { get; set; }
            public List<Result> results { get; set; }
        }


    
}
