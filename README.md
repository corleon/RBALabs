# RBA Labs Website

Новый сайт Umbraco-агентства RBA Labs с нуля.

## Технологии

- Umbraco CMS `17.3.2`
- .NET SDK `10.0.200`
- ASP.NET Core + Razor Views

## Структура

- `RBALabs.Website.slnx` — solution
- `src/RBALabs.Website` — основной Umbraco-проект
- `rba_labs_version_c_html (1).html` — исходный HTML-макет (референс)

## Локальный запуск

```bash
dotnet restore src/RBALabs.Website/RBALabs.Website.csproj
dotnet run --project src/RBALabs.Website/RBALabs.Website.csproj
```

После запуска:

- Сайт: `https://localhost:44350` или `http://localhost:5000` (точные порты в выводе)
- Backoffice: `/umbraco`

При первом запуске Umbraco откроет установщик, где создается админ-аккаунт и БД.

## Ближайшие шаги

1. Перенести базовый layout/стили из HTML-макета в Umbraco templates.
2. Создать Document Types: Home, Service Page, Case Study, Contact.
3. Настроить контентные блоки (Block Grid) для гибкой сборки страниц.
4. Добавить SEO-базу (meta, OpenGraph, sitemap, robots).
