using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CodeHollow.FeedReader;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace livesino_feed_bot
{
    class Program
    {
        static void Main(string[] args)
        {
            var botClient = new TelegramBotClient("BOT TOKEN HERE");

            string url = "https://livesino.net/feed";
            string titles_name = "titles-livesino.txt";
            string titles_path = Environment.CurrentDirectory;

            while (true)
            {
                string all_titles = System.IO.File.ReadAllText(Path.Combine(titles_path, titles_name));
                // titles file as the database

                var urlsTask = FeedReader.GetFeedUrlsFromUrlAsync(url);
                urlsTask.ConfigureAwait(false);
                var urls = urlsTask.Result;

                string feedUrl = url;

                var readerTask = FeedReader.ReadAsync(feedUrl);
                readerTask.ConfigureAwait(false);

                foreach (var item in readerTask.Result.Items)
                {
                    if (!all_titles.Contains(item.Title))
                    {
                        using (StreamWriter outputFile = new StreamWriter(Path.Combine(titles_path, titles_name), true))
                        {
                            outputFile.WriteLine(item.Title);
                        }

                        System.Console.WriteLine(item.Title);
                        var Content = Regex.Replace(item.Content, "<img.*?/>", "");
                        // remove the img tag
                        Content = Regex.Replace(Content, "<.p?>", "");
                        // remove <p> and <p/> tag
                        Content = Regex.Replace(Content, "<a.*?pvxt.*?促销.*?阅读原文.*?>", "");
                        // remove the ads

                        botClient.SendTextMessageAsync(
                            chatId: "@livesino",
                            text: $"<b>{item.Title}\n\n</b>{Content}\n{item.Link}",
                            parseMode: ParseMode.Html
                        );
                        // rss to telegram
                        // ATTENTION: html tags are not all supported
                        // checkout https://core.telegram.org/bots/api#html-style
                    }
                }

                System.Console.WriteLine($"yet another try at {DateTime.Now}");
                Thread.Sleep(600000);
                // refresh every 10 min
            }
        }
    }
}
