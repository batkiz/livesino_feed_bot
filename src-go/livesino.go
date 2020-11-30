package livesino

import (
	"fmt"
	"log"
	"net/http"
	"os"
	"regexp"

	"github.com/deta/deta-go"

	tgbotapi "github.com/go-telegram-bot-api/telegram-bot-api"
	"github.com/mmcdole/gofeed"
)

type Article struct {
	Key   string `json:"key"`
	Url   string `json:"url"`
	Title string `json:"title"`
}

func Livesino(w http.ResponseWriter, r *http.Request) {
	// load secrets
	projectKey := os.Getenv("projectKey")
	token := os.Getenv("token")

	// init database
	d, err := deta.New(projectKey)
	if err != nil {
		log.Fatal("failed to init new Deta instance:", err)
	}

	db, err := d.NewBase("livesino")
	if err != nil {
		log.Fatal("failed to init new Base instance:", err)
	}

	// init bot
	bot, err := tgbotapi.NewBotAPI(token)
	if err != nil {
		log.Fatal(err)
	}

	feed, err := gofeed.NewParser().ParseURL("https://livesino.net/feed")
	if err != nil {
		log.Println(err)
	}
	log.Println(feed.Title)

	var articles []Article
	for _, item := range feed.Items {
		articles = append(articles, feedItemToArticle(item))
	}

	for _, article := range articles {
		if !isArticleExists(db, article.Key) {
			text := fmt.Sprintf(
				"<b>%v</b>\nhttps://t.me/iv?url=%v&rhash=e0127ad0c39cac",
				article.Title,
				article.Url)

			log.Println(text)

			// msg := tgbotapi.NewMessage(-1001169390347, text)
			msg := tgbotapi.NewMessageToChannel("@livesino", text)
			msg.ParseMode = tgbotapi.ModeHTML

			var btn = tgbotapi.NewInlineKeyboardMarkup(
				tgbotapi.NewInlineKeyboardRow(
					tgbotapi.NewInlineKeyboardButtonURL("原文链接", article.Url),
				),
			)

			msg.ReplyMarkup = btn

			_, err := bot.Send(msg)
			if err != nil {
				log.Println(err)
			}

			_, err = db.Put(article)
			if err != nil {
				msg := tgbotapi.NewMessage(367019922, err.Error())
				bot.Send(msg)
			}
		}
	}

	fmt.Fprintf(w, "200 OK")

}

func isArticleExists(db *deta.Base, key string) bool {
	var a Article

	err := db.Get(key, &a)
	if err == deta.ErrNotFound {
		return false
	}

	return true
}

func urlToKey(url string) string {
	re := regexp.MustCompile("\\d+")
	key := re.FindAllString(url, -1)[0]

	return key
}

func feedItemToArticle(item *gofeed.Item) Article {
	url := item.Link
	key := urlToKey(url)
	title := item.Title

	a := &Article{
		Key:   key,
		Url:   url,
		Title: title,
	}

	return *a
}
