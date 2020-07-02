using System.Threading;
using System;
using System.IO;
using CodeHollow.FeedReader;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace livesino_feed_bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start job");
            const string tokenFile = ".token";
            var token = File.ReadAllText(tokenFile);

            const string url = "https://livesino.net/feed";
            const string titlesName = "titles-livesino.txt";
            var titlesPath = Environment.CurrentDirectory;

            var botClient = new TelegramBotClient(token);


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

                    var ivLink = $"https://t.me/iv?url={item.Link}&rhash=e0127ad0c39cac";

                    botClient.SendTextMessageAsync("@livesino",
                        $"<b>{item.Title}</b>\n{ivLink}",
                        ParseMode.Html,
                        replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl(
                            "原文链接",
                            $"{item.Link}"
                        ))
                    );

                    Console.WriteLine($"{item.Title}\n{ivLink}");
                }

                Console.WriteLine($"yet another try at {DateTime.Now}");
                Thread.Sleep(600000);
            }
        }
    }
}