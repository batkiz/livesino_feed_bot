using System;
using System.IO;
using System.Threading;
using CodeHollow.FeedReader;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace LivesinoIVBot
{
    class Program
    {
        static string Token = "";
        const string url = "https://livesino.net/feed";
        const string titlesName = "titles-livesino.txt";
        static string titlesPath = Environment.CurrentDirectory;

        static void Main(string[] args)
        {
            var botClient = new TelegramBotClient(Token);

            for (; ; )
            {
                var allTitles = File.ReadAllText(Path.Combine(titlesPath, titlesName));

                var urlsTask = FeedReader.GetFeedUrlsFromUrlAsync(url);
                urlsTask.ConfigureAwait(false);

                var readerTask = FeedReader.ReadAsync(url);
                readerTask.ConfigureAwait(false);

                foreach (var item in readerTask.Result.Items)
                {
                    if (allTitles.Contains(item.Title))
                    {
                        continue;
                    }
                    using (var outputFile = new StreamWriter(Path.Combine(titlesPath, titlesName), true))
                    {
                        outputFile.WriteLine(item.Title);
                    }

                    Console.WriteLine($"{item.Title}\n{item.Link}\n");

                    var ivLink = $"https://t.me/iv?url={item.Link}&rhash=e0127ad0c39cac";

                    botClient.SendTextMessageAsync(chatId: "@livesino", text: $"<b>{item.Title}</b>\n{ivLink}", parseMode: ParseMode.Html);
                    System.Console.WriteLine(ivLink);
                }

                Console.WriteLine($"yet another try at {DateTime.Now}");
                Thread.Sleep(600000);
            }
        }
    }
}
