# Client Utility

Консольная утилита-клиент для [ZIP-Backend сервиса](https://github.com/iosif-cpp/ZIP-Backend)

## Функционал
- получение списка файлов
- создание задачи на архивирование
- проверку статуса задачи
- скачивание архива
- режим "одна команда": создать → дождаться готовности → скачать
а
## Требования

- .NET SDK 9 (можно поменять TargetFramework)
- Запущенный backend, по умолчанию: `http://localhost:5278`

## Как собрать?

```powershell
dotnet build .\Kaspersky-Task2\Kaspersky-Task2.csproj
```

После сборки исполняемый файл будет в:

`Kaspersky-Task2\bin\Debug\net9.0\util.dll`

## Настройка адреса backend

По умолчанию используется `http://localhost:5278`.

Переопределить можно:

- переменной окружения `KASPERSKY_BACKEND_URL` (или `BACKEND_URL`);
- флагом `--base-url <url>` в POSIX-режиме (см. ниже).

## Режим 1: интерактивный

Запуск из папки `bin`:

```powershell
cd .\Kaspersky-Task2\bin\Debug\net9.0
dotnet util.dll
```

Дальше:

```powershell
> client
Client was started.
Press <Enter> to exit...
> list
...
```

Основные команды
```powershell
list - список файлов
create-archive file1 file2... - запрос на создание архива
status <processId> - проверить статус задачи
download <processId> <path> - скачать по айди в директорию
```



В utility-режиме поддерживается команда:

- `client` — вход в клиентский интерактивный режим.

## Режим 2: POSIX

```powershell
cd .\Kaspersky-Task2\bin\Debug\net9.0
dotnet util.dll list
```

### Команды

Список файлов:

```powershell
dotnet util.dll list
```

Создать задачу на архив:

```powershell
dotnet util.dll create-archive file1.txt file2.txt
```

Проверить статус:

```powershell
dotnet util.dll status <processId>
```

Скачать архив:

```powershell
dotnet util.dll download <processId> <path>
```

Режим "одна команда" (создать → дождаться → скачать):

```powershell
dotnet util.dll create-and-download-archive file1.txt file2.txt --out "C:\Temp" --poll-interval 2s --timeout 2m
```

### Флаги

`--base-url <url>`

- Адрес backend, например: `--base-url http://localhost:5278`
- Флаг может находиться до команды.

Пример:

```powershell
dotnet util.dll --base-url "http://localhost:5278" list
```

`--out <path>`

- Папка для сохранения архива.
- Используется в `create-and-download-archive`.
- Если флаг не указан, используется текущая директория.

`--poll-interval <time>`

- Интервал опроса статуса задачи в режиме `create-and-download-archive`.
- Не обязателен. Значение по умолчанию: `2s`.
- Поддерживаемые форматы: `100ms`, `2s`, `10m`, `1h`, а также форматы `TimeSpan`.

`--timeout <time>`

- Максимальное время ожидания готовности архива в режиме `create-and-download-archive`.
- Не обязателен. Значение по умолчанию: `10m`.
- Поддерживаемые форматы как у `--poll-interval`.

## Ошибки backend

Утилита выводит ошибки backend в формате:

`<statusCode>: <message>`

Например:

- `400: File 'x.txt' does not exist.`
- `404: Process not found.`
- `409: Archive is not ready yet.`

## Запуск unit-тестов

```powershell
dotnet test .\Kaspersky-Task2.Tests\Kaspersky-Task2.Tests.csproj
```

