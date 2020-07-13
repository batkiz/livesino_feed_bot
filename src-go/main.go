package main

import (
	"fmt"
	tgbotapi "github.com/go-telegram-bot-api/telegram-bot-api"
	"github.com/mmcdole/gofeed"
	"io/ioutil"
	"log"
	"os"
	"strings"
	"time"
)

func main() {
	var token, _ = ioutil.ReadFile(".token")
	var bot, _ = tgbotapi.NewBotAPI(string(token))


	for true {
		titles, _ := ioutil.ReadFile("titles.txt")

		items := fetchRss()

		for _, item := range items {
			if !strings.Contains(string(titles), item.Title) {
				text := fmt.Sprintf(
					"<b>%v</b>\nhttps://t.me/iv?url=%v&rhash=e0127ad0c39cac",
					item.Title,
					item.Link)

				log.Println(text)

				// msg := tgbotapi.NewMessage(-1001169390347, text)
				msg := tgbotapi.NewMessageToChannel("@livesino", text)

				msg.ParseMode = tgbotapi.ModeHTML

				var btn = tgbotapi.NewInlineKeyboardMarkup(
					tgbotapi.NewInlineKeyboardRow(
						tgbotapi.NewInlineKeyboardButtonURL("原文链接", item.Link),
					),
				)

				msg.ReplyMarkup = btn

				if _, err := bot.Send(msg); err != nil {
					log.Panic(err)
				}

				f, _ := os.OpenFile("titles.txt", os.O_APPEND|os.O_WRONLY, 0600)
				_, _ = f.WriteString(item.Title)

				_ = f.Close()
			}
		}

		log.Println()
		time.Sleep(time.Duration(10) * time.Minute)
	}
}

func fetchRss() []*gofeed.Item {
	feed, _ := gofeed.NewParser().ParseURL("https://livesino.net/feed")

	log.Println(feed.Title)

	return feed.Items
}
