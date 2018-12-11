# PullentiServer

Простая обёртка HTTP-сервер для библиотеки PullEnti для .NET Core 2.0. Запросы обрабатываются последовательно, нет ограничений на число запросов, объём текста в одном запросе, время обработки. Предполагается, что сервер используется локально в связке с [pullenti-client](https://github.com/pullenti/pullenti-client). 

## Разработка

Скачать PullEnti SDK

```bash
cd ..
git clone https://github.com/pullenti/PullentiNetCore.git
```

