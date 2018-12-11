# PullentiServer

Простая обёртка HTTP-сервер для библиотеки PullEnti для .NET Core 2.0. Запросы обрабатываются последовательно, нет ограничений на число запросов, объём текста в одном запросе, время обработки. Предполагается, что сервер используется локально в связке с [pullenti-client](https://github.com/pullenti/pullenti-client). 

## Использование

Сервер распространяется в виде Docker-контейнера, настраивается XML-конфигурацией.

conf-basic.xml
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

Запустить демон на порте 8083:

```bash
docker run -d --name pullenti -p 8083:8080 -v $PWD/conf-basic.xml:/app/conf.xml pullenti/pullenti-server
```

Запрос

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
  <match start="25" stop="35" referent="8046282" />
  <match start="68" stop="98" referent="32015599" />
  <match start="139" stop="177" referent="56037156" />
  <match start="228" stop="238" referent="8046282" />
  <match start="284" stop="293" referent="32015599" />
  <match start="334" stop="345" referent="14464807" />
  <match start="377" stop="387" referent="8046282" />
</result>

```

Для разбора ответа сервера есть специальная библиотека [pullenti-client](https://github.com/pullenti/pullenti-client).

Логи

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

Остановить

```bash
docker kill pullenti
docker rm pullenti
```

## Разработка

Скачать PullEnti SDK

```bash
cd ..
git clone https://github.com/pullenti/PullentiNetCore.git
```

Собрать и опубликовать контейнер

```bash
make image push
```
