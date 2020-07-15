FROM golang:latest-alpine

WORKDIR /livesino

COPY .token . \
COPY go.mod . \
COPY go.sum . \
COPY main.go .

RUN go build

VOLUME ["./titles.txt"]

CMD ["./livesino"]