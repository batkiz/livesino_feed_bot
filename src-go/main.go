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
	tokenStr, err := ioutil.ReadFile(".token")
	if err != nil {
		log.Println(err)
	}

	token := strings.TrimRight(string(tokenStr), "\n")

	bot, err := tgbotapi.NewBotAPI(token)
	if err != nil {
		log.Println(err)
	}

	for true {
		titles, err := ioutil.ReadFile("titles.txt")
		if err != nil {
			log.Println(err)
		}

		feed, err := gofeed.NewParser().ParseURL("https://livesino.net/feed")
		if err != nil {
			log.Println(err)
		}

		log.Println(feed.Title)

		items := feed.Items

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

				m, err := bot.Send(msg)

				log.Println(m)

				if err != nil {
					log.Println(err)
				}

				f, err := os.OpenFile("titles.txt", os.O_APPEND|os.O_WRONLY, 0600)
				if err != nil {
					log.Println(err)
				}

				_, err = f.WriteString(item.Title + "\n")
				if err != nil {
					log.Println(err)
				}

				err = f.Close()
				if err != nil {
					log.Println(err)
				}
			}
		}

		log.Println()
		time.Sleep(time.Duration(10) * time.Minute)
	}
}
