# Система «Антиплагиат»

## Содержание

1. Общее описание и цели
2. Архитектура системы (компоненты, границы ответственности)
3. Схема данных (БД / таблицы)
4. Подробное API (эндпойнты каждого сервиса + примеры запросов/ответов)
5. Алгоритм определения плагиата (описание + тонкие моменты)
6. Визуализация --- облако слов (QuickChart)
7. Docker / docker-compose --- как собрать и запустить (пошагово)
8. Swagger (как использовать)
9. Обработка ошибок, устойчивость и нюансы взаимодействия микросервисов
10. Тестирование (curl-примеры)

## 1. Общее описание и цели

Система предназначена для приёма контрольных работ от студентов, хранения присланных файлов и выполнения автоматического анализа на заимствования (антиплагиат). Система состоит из независимых микросервисов и API Gateway, реализована с использованием .NET 8 и упакована в Docker-контейнеры. Основные требования:

- Приём и хранение файлов (кто/когда/по какому заданию).
- Запуск анализа файла и генерация отчёта (хранение отчётов).
- API для получения отчётов по контрольной работе (work).
- Swagger + Postman для демонстрации API.
- Визуализация текста файла в виде облака слов (QuickChart) - бонус для 10 баллов.

## 2. Архитектура системы

**Компоненты**

- **ApiGateway** - единственная точка входа для клиента. Маршрутизирует запросы к FileStoringService и FileAnalysisService, реализует агрегирующие и вспомогательные эндпойнты (включая word cloud).

- **FileStoringService** - отвечает за приём файлов, сохранение метаданных в БД (SQLite), хранение файла на диске (в контейнере --- /app/uploads), отдачу файла и базовой информации.

- **FileAnalysisService** - скачивает файл у FileStoringService, извлекает текст (UTF-8 plain text; в рамках MVP предполагаем .txt или уже распарсенные тексты), выполняет проверку на заимствование (алгоритм см. ниже), сохраняет текст и отчёт в свою БД (SQLite) и отдаёт отчёты.

- **Docker / docker-compose** - развёртывание всех трёх сервисов (каждый в собственном контейнере).

- **QuickChart (внешний)** - сторонний сервис для генерации облака слов по тексту (используется ApiGateway).

**Разделение ответственности (KISS)**

- FileStoring - только файлы / метаданные.
- FileAnalysis - только анализ, отчёты, исторические тексты.
- ApiGateway - маршрутизация, агрегация, визуализация (word cloud), публичный API.

**Сеть**

Docker Compose создаёт общую сеть kpo_hw3_default. Сервисы общаются друг с другом по именам: filestoring:5001, fileanalysis:5002. Gateway доступен с хоста по localhost:8080.

## 3. Схема данных (SQLite)

**FileStoringService (filestore.db) --- таблица FileEntries**

- Id TEXT PRIMARY KEY (GUID)
- StudentName TEXT
- TaskId INTEGER
- FileName TEXT
- FilePath TEXT --- путь на диске, например /app/uploads/{fileId}_{fileName}
- FileSize INTEGER
- UploadedDate TEXT

**FileAnalysisService (analysis.db)**

- FileContents:
    - Id TEXT PRIMARY KEY (GUID) --- тот же fileId
    - Content TEXT --- извлечённый (и/или raw) текст файла (utf-8)
    - StudentName TEXT
    - TaskId TEXT
    - UploadedDate TEXT
    - ReportId TEXT NULL --- FK в AnalysisReports

- AnalysisReports:
    - Id TEXT PRIMARY KEY (GUID)
    - FileId TEXT NOT NULL --- FK на FileContents
    - HasPlagiarism INTEGER (0/1)
    - SimilarityPercent REAL
    - Status TEXT (pending / completed / failed)
    - AnalyzedDate TEXT

## 4. Подробное API

Общая рекомендация: использовать ApiGateway как единую точку входа. Ниже перечислены эндпойнты ApiGateway, а также внутренние эндпойнты микросервисов (для отладки/Swagger).

**ApiGateway (порт 8080)**

- GET /health  
  Response 200:
  {
  "status":"healthy",
  "service":"api-gateway",
  "timestamp":"2025-12-11T..Z"
  }

- GET /api/files/{fileId} --- получить метаданные файла (маршрутизует в FileStoringService).  
  Прокси-запрос: GET http://filestoring:5001/api/files/{fileId}.  
  **Важно:** при создании все части файла нужно именовать на английском языке, в том числе и имя пользователя.

- POST /api/files/upload --- загрузка файла через Gateway (multipart/form-data). Gateway собирает форму и пересылает в Filestoring.  
  Пример curl:
  curl -X POST "http://localhost:8080/api/files/upload" \
  -F "studentName=Ivan" \
  -F "taskId=3" \
  -F "file=@./work.txt"

  Пример ответа:
  {
  "fileId":"<guid>",
  "fileName":"work.txt",
  "studentName":"Ivan",
  "status":"uploaded",
  "taskId":3,
  "dateCreated":"2025-12-11T07:13:29Z"
  }

- GET /api/files/{fileId}/download --- скачивание файла (Gateway пробрасывает GET http://filestoring:5001/api/files/{fileId}/download и возвращает application/octet-stream).

- POST /api/analysis/{fileId}?studentName=...&taskId=... --- запустить анализ конкретного файла (Gateway пересылает POST http://fileanalysis:5002/api/analysis/{fileId}?...).  
  Пример:
  curl -X POST "http://localhost:8080/api/analysis/<fileId>?studentName=Ivan&taskId=3"

  Ответ от FileAnalysisService --- JSON с информацией об отчёте.

- GET /api/analysis/reports/{fileId} --- получить отчёт по файлу (маршрутизует fileanalysis).

- GET /api/analysis/works/{workId}/reports --- analytics по работе (все отчёты для workId).

- **Word cloud**: GET /api/visual/wordcloud/{fileId} --- Gateway скачивает файл из filestoring, извлекает текст (utf-8), отправляет в QuickChart и возвращает PNG.  
  Пример вызова:
  curl "http://localhost:8080/api/visual/wordcloud/<fileId>" --output wordcloud.png

**FileStoringService (порт контейнера 5001)**

- POST /api/files/upload (multipart/form-data)  
  Request form fields: studentName (string), taskId (int), file (IFormFile).  
  Validation: file non-empty, max 10MB. Saves file to /app/uploads/{fileId}_{filename}. Saves metadata to SQLite. Responds JSON FileUploadResponse.

- GET /api/files/{fileId} --- возвращает FileDownloadResponse (метаданные).

- GET /api/files/{fileId}/download --- возвращает файл (Results.File).

Swagger: http://localhost:5001/swagger (или root, в зависимости от RoutePrefix).

**FileAnalysisService (порт контейнера 5002)**

- POST /api/analysis/{fileId}?studentName=...&taskId=... --- запускает анализ.  
  Важно: **внутренние обращения к FileStoringService должны использовать http://filestoring:5001**, а не localhost. Запрос скачивает файла .../download, извлекает текст, сохраняет текст и report, возвращает JSON AnalysisResponse.

- GET /api/analysis/reports/{fileId} --- получить report по конкретному файлу.

- GET /api/analysis/works/{workId}/reports --- получить все отчёты для работы (workId).

Swagger: http://localhost:5002/swagger.

## 5. Алгоритм определения плагиата

Плагиат считается, если существует более ранняя сдача другим студентом по тому же заданию, чей текст совпадает с анализируемым более чем на 80%.
Шаги алгоритма:

1. Сервис получает:
- fileId,
- studentName,
- taskId,
- текущий текст документа.

2. Из БД выбираются предыдущие работы
3. Для каждой работы проводится сравнение: similarity = (число совпавших слов) / (максимальное количество слов в документе)
4. Если similarity >= 0.8 — отчёт помечается как плагиат

## 6. Визуализация - облако слов (QuickChart)

- Endpoint в ApiGateway: GET /api/visual/wordcloud/{fileId}  
  Логика: Gateway делает GET http://filestoring:5001/api/files/{fileId}/download → получает bytes → декодирует в UTF-8 → формирует JSON для QuickChart { format, width, height, text } → POST https://quickchart.io/wordcloud → получает PNG → отдаёт клиенту.

- Ограничения: файл должен быть текстовым (utf-8). Для .pdf/.docx нужна дополнительная обработка; в MVP допускается только text/plain или заранее подготовленный текст.

- Пример вызова (curl):
  curl "http://localhost:8080/api/visual/wordcloud/<fileId>" --output wordcloud.png

## 7. Docker / docker-compose - как собрать и запустить

**Структура репозитория (рекомендуемая)**

root/
docker-compose.yml
ApiGateway/
ApiGateway.csproj
Program.cs
Dockerfile
FileStoringService/
FileStoringService.csproj
Program.cs
Dockerfile
uploads/ (volume)
FileAnalisisService/
FileAnalisisService.csproj
Program.cs
Dockerfile
README.md

**Пример корректного docker-compose.yml**

services:
api-gateway:
build: ./ApiGateway
ports:
- "8080:8080"
environment:
- ASPNETCORE_URLS=http://0.0.0.0:8080
depends_on:
- filestoring
- fileanalysis

filestoring:
build: "./FileStoringService "
environment:
- ASPNETCORE_URLS=http://0.0.0.0:5001
volumes:
- ./uploads:/app/uploads
- ./data/filestoring:/app/data

fileanalysis:
build: "./FileAnalisisService"
environment:
- ASPNETCORE_URLS=http://0.0.0.0:5002
volumes:
- ./data/analysis:/app/data
depends_on:
- filestoring

**Запуск**

1. Из корня проекта:
2. docker compose down -v
3. docker compose up --build
4. Проверить контейнеры:
5. docker ps
6. Swagger:
    - ApiGateway (если включён): http://localhost:8080/swagger
    - FileStoring: http://localhost:5001/swagger
    - FileAnalisis: http://localhost:5002/swagger

## 8. Swagger

- Каждый микросервис имеет AddEndpointsApiExplorer() + AddSwaggerGen() и app.UseSwagger(); app.UseSwaggerUI(...);.
- По умолчанию Swagger UI включён (в development).


**Рекомендуемые Postman-запросы**

- Upload file (POST multipart) → http://localhost:8080/api/files/upload
- Start analysis → http://localhost:8080/api/analysis/{fileId}?studentName=...&taskId=...
- Get report → http://localhost:8080/api/analysis/reports/{fileId}
- Word cloud → GET http://localhost:8080/api/visual/wordcloud/{fileId}

## 9. Обработка ошибок, устойчивость и нюансы взаимодействия

**Обработка ошибок**

- Каждый внешний HTTP-запрос обёрнут в try/catch (HttpRequestException) в Gateway и в FileAnalysisService. В случае недоступности сервиса --- возвращаем 503 Service Unavailable с user-friendly сообщением.

- Внутри анализатора - если получение файла возвращает 404 - возвращаем понятную ошибку: Failed to get file: NotFound → Gateway перехватывает и транслирует 404 клиенту.

- При критических ошибках - сохраняем AnalysisReport со статусом failed и записью в логи.

**Очередь / асинхронность**

- синхронный вызов POST /analysis/{fileId} от Gateway к FileAnalysisService. Можно улучшить: Gateway POST → FileAnalysis записывает задачу в очередь (RabbitMQ) → worker асинхронно обрабатывает.

## 10. Тестирование (curl-примеры)

1. Загрузить файл:

curl -X POST "http://localhost:8080/api/files/upload" \
-F "studentName=TestStudent" \
-F "taskId=999" \
-F "file=@./analysis_test.txt"

Ожидаемый ответ: JSON с fileId.

2. Запустить анализ:

curl -X POST "http://localhost:8080/api/analysis/<fileId>?studentName=TestStudent&taskId=999"

Ожидаемый ответ: JSON AnalysisResponse или ошибка с подробностью.

3. Получить отчёт:

curl "http://localhost:8080/api/analysis/reports/<fileId>"

4. Получить облако слов:

curl "http://localhost:8080/api/visual/wordcloud/<fileId>" --output wordcloud.png

Открыть wordcloud.png.