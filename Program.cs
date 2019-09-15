using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using CodeHollow.FeedReader;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace livesino_rss_bot
{
    class Program
    {
        static void Main(string[] args)
        {
            // your bot token here
            const string BotToken = "";

            var botClient = new TelegramBotClient(BotToken);

            const string url = "https://livesino.net/feed";
            const string titlesName = "titles-livesino.txt";
            var titlesPath = Environment.CurrentDirectory;

            for (; ; )
            {
                // 保存文章标题作为数据库
                var allTitles = File.ReadAllText(Path.Combine(titlesPath, titlesName));

                var urlsTask = FeedReader.GetFeedUrlsFromUrlAsync(url);
                urlsTask.ConfigureAwait(false);

                var feedUrl = url;

                var readerTask = FeedReader.ReadAsync(feedUrl);
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

                    var content = RmUselessInfo(RmUselessTag(item.Content));

                    Console.WriteLine($"{item.Title}\n{content}\n{item.Link}");


                    // 未验证
                    // 如果 content 里包含不支持的标签，则仅发送标题与链接
                    try
                    {
                        botClient.SendTextMessageAsync(chatId: "@livesino",
                                                       text: $"<b>{item.Title}</b>\n\n{content}\n{item.Link}",
                                                       parseMode: ParseMode.Html);
                        // ATTENTION: html tags are NOT ALL supported
                        // checkout https://core.telegram.org/bots/api#html-style
                    }
                    catch (System.Exception)
                    {
                        botClient.SendTextMessageAsync(chatId: "@livesino",
                                                       text: $"<b>{item.Title}</b>\n\n{item.Link}",
                                                       parseMode: ParseMode.Html);
                    }
                }

                Console.WriteLine($"yet another try at {DateTime.Now}");
                // refresh every 10 minutes
                Thread.Sleep(600000);
            }
        }

        // 去除广告及阅读原文信息
        static Func<string, string> RmUselessInfo = content =>
            Regex.Replace(content, "<a.*?pvxt.*?促销.*?阅读原文.*?>", "");
        // 去除不受支持的标签
        static Func<string, string> RmUselessTag = content =>
            Regex.Replace(content, "<br />|<.p?>|<img.*?/>|<.strong?>|<.li?>|<.ul?>", "");
        // static Func<string, string> ReplaceImg = content => Regex.Replace(content, "<img.*?(https?://.*.(?:png|jpg))*?/>", "(https?://.*.(?:png|jpg))");
    }
}
