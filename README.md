# Renga Bri4ka

Набор утилит и плагинов для Renga "Bri4ka"

![](icons/Bri4kaIcon_256x256.png)

План разработки см. в [отдельном файле](TODO.md).

# Установка и использование

См. [отдельный файл](docs/src/1.1_INSTALL.md)

## Возможные обновления

Среда .NET Framework 4.8 -- https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-offline-installer

# Dev

Внешние зависимости (файлы из Renga SDK, `Renga.NET.PluginUtility.dll`, `Renga.NET8.PluginUtility.dll`, `RengaCOMAPI.tlb`) размещаются по относительному пути в папке `external` в корне репозитория. Для корректности, они исключены на уровне `.gitignore`.

Проекты имеют TargetFramework = net48.

Иконки создаются в приложении IcoFX, лежат в \external и в репозиторий не включаются.

Справка собирает через [mdbook](https://github.com/rust-lang/mdBook/releases) (перед сборкой установите его себе и добавьте папку с ним в переменную Path).

Для автосборки имеется скрипт в корне репозитория `RengaBri4ka_Build.bat`.
