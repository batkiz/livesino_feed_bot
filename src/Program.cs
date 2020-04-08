using System.Threading;
using System;
using System.IO;
using CodeHollow.FeedReader;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace livesino_feed_bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start job");

            string Token = args[0];
            const string url = "https://livesino.net/feed";
            const string titlesName = "titles-livesino.txt";
            string titlesPath = Environment.CurrentDirectory;

            var botClient = new TelegramBotClient(Token);

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

                var ivLink = $"https://t.me/iv?url={item.Link}&rhash=e0127ad0c39cac";

                botClient.SendTextMessageAsync(chatId: "@livesino",
                                               text: $"<b>{item.Title}</b>\n{ivLink}",
                                               parseMode: ParseMode.Html);
                Console.WriteLine($"{item.Title}\n{ivLink}");
            }

            Thread.Sleep(5000);

            Console.WriteLine("Done");
        }
    }
}
