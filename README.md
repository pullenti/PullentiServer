# PullentiServer [![Build Status](https://travis-ci.org/pullenti/PullentiServer.svg?branch=master)](https://travis-ci.org/pullenti/PullentiServer)

Простая обёртка HTTP-сервер для библиотеки [PullEnti для .NET Core 2.0](https://github.com/pullenti/PullentiNetCore). Запросы обрабатываются последовательно, нет ограничений на время обработки, число запросов, объём текста в одном запросе. Предполагается, что сервер используется локально в связке с [pullenti-client](https://github.com/pullenti/pullenti-client). 

## Использование

Сервер распространяется в виде Docker-контейнера, настраивается XML-конфигурацией.

custom.xml:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<conf>
  <listen>
    <host>*</host>
    <port>8080</port>
  </listen>
  <langs>
    <lang>ru</lang>
    <lang>en</lang>
  </langs>
  <analyzers>
    <analyzer>geo</analyzer>
    <analyzer>org</analyzer>
    <analyzer>person</analyzer>
  </analyzers>
</conf>
```

Cписок доступных langs: ru, ua, by, en, it, kz.

Список доступных analyzers: money, uri, phone, date, keyword, definition, denomination, measure, bank, geo, address, org, person, mail, transport, decree, instrument, titlepage, booklink, business, named, weapon.

Запустить демон на порте 8083:

```bash
docker run -d --name pullenti -p 8083:8080 -v $PWD/custom.xml:/app/conf.xml pullenti/pullenti-server
```

Запрос:

```bash
curl -X POST http://localhost:8083/ -d 'Единственным конкурентом «Трансмаша» на этом дебильном тендере было ООО «Плассер Алека Рейл Сервис», основным владельцем которого является австрийская компания «СТЦ-Холдинг ГМБХ». До конца 2011 г. эта же фирма была совладельцем «Трансмаша» вместе с «Тако» Краснова. Зато совладельцем «Плассера», также до конца 2011 г., был тот самый Карл Контрус, который имеет четверть акций «Трансмаша».'
```
```xml
<result>
  <referent id="277458" type="GEO" />
  <slot parent="277458" value="AT" key="ALPHA2" />
  <slot parent="277458" value="АВСТРИЙСКАЯ РЕСПУБЛИКА" key="NAME" />
  <slot parent="277458" value="АВСТРИЯ" key="NAME" />
  <slot parent="277458" value="государство" key="TYPE" />
  <referent id="8046282" type="ORGANIZATION" />
  <slot parent="8046282" value="ТРАНСМАША" key="NAME" />
  <slot parent="8046282" value="ТРАНСМАШ" key="NAME" />
  <referent id="32015599" type="ORGANIZATION" />
  <slot parent="32015599" value="ООО" key="TYPE" />
  <slot parent="32015599" value="общество с ограниченной ответственностью" key="TYPE" />
  <slot parent="32015599" value="ПЛАССЕР АЛЕКА РЕЙЛ СЕРВИС" key="NAME" />
  <referent id="56037156" type="ORGANIZATION" />
  <slot parent="56037156" value="компания" key="TYPE" />
  <slot parent="56037156" referent="277458" key="GEO" />
  <slot parent="56037156" value="холдинг" key="TYPE" />
  <slot parent="56037156" value="СТЦ ХОЛДИНГ ГМБХ" key="NAME" />
  <slot parent="56037156" value="австрийская компания" key="TYPE" />
  <referent id="14464807" type="PERSON" />
  <slot parent="14464807" value="MALE" key="SEX" />
  <slot parent="14464807" value="КОНТРУС" key="LASTNAME" />
  <slot parent="14464807" value="КАРЛ" key="FIRSTNAME" />
  <match id="803548" referent="10675717" start="25" stop="36" />
  <match id="26517107" referent="16754362" start="68" stop="99" />
  <match id="2649323" referent="16023056" start="139" stop="178" />
  <match id="20318803" referent="10675717" start="228" stop="239" />
  <match id="66540731" referent="16754362" start="284" stop="294" />
  <match id="48360500" referent="58998806" start="334" stop="346" />
  <match id="52392654" referent="10675717" start="377" stop="388" />
</result>

```

Для разбора ответа сервера есть специальная библиотека [pullenti-client](https://github.com/pullenti/pullenti-client).

Логи:

```bash
docker logs pullenti

2018-12-11 06:14:56 [INFO] Init Pullenti v3.14 ...
2018-12-11 06:14:56 [INFO] Load lang: ru
2018-12-11 06:14:57 [INFO] Load lang: en
2018-12-11 06:14:57 [INFO] Load analyzer: geo
2018-12-11 06:14:57 [INFO] Load analyzer: org
2018-12-11 06:14:58 [INFO] Load analyzer: person
2018-12-11 06:14:59 [INFO] Listen prefix: http://*:8080/
2018-12-11 06:16:21 [INFO] Process: 389 chars, 1.707s, 5 refs
...
```

Остановить сервер:

```bash
docker kill pullenti
docker rm pullenti
```

## Разработка

Собрать контейнер:

```bash
rm -r EP.SdkCore
# patch version Makefile:IMAGE=...
make image
```

Тест:

```bash
make up
# wait 5 sec
make test
make down
```

Опубликовать контейнер:

```bash
make push
```

Закомитить

```bash
git status
git add .
git commit -m 'Mirror 3.19'
git push
```
