# Лабораторные работы 12-14

Проект: WPF-приложение «Телефонная книга» на .NET 8 с Entity Framework Core и SQLite.

## Лабораторная работа 12

Тема: взаимодействие с базами данных в .NET. ADO.NET vs ORM. Обратная инженерия.

Что реализовано:

- Подключены NuGet-пакеты Entity Framework Core, EF Core SQLite, Tools/Design и `Microsoft.Extensions.Hosting`.
- Создана SQLite-база `PhoneBookDB_Храмов_2407sa1.db` с таблицей `Contacts`.
- По готовой базе выполнена обратная инженерия командой `dotnet ef dbcontext scaffold`.
- Сгенерированы сущность `Contact` и контекст `PhoneBookDbContext`.
- `PhoneBookDbContext` зарегистрирован в DI-контейнере в `App.xaml.cs`.

## Лабораторная работа 13

Тема: взаимодействие с базами данных в .NET. CRUD операции.

Цель: изучить работу `DbContext`, механизм Change Tracker и реализовать полный цикл CRUD-операций в приложении «Телефонная книга».

Что реализовано:

- `PhoneBookDbContext` внедряется через DI в `ContactsListViewModel` и `ContactEditViewModel`.
- `Read`: список контактов загружается из базы данных, добавлена фильтрация по имени и телефону.
- `Create`: новый контакт добавляется через форму и сохраняется вызовом `_context.SaveChanges()`.
- `Update`: выбранный контакт загружается в форму, изменения сохраняются в базе данных.
- `Delete`: выбранный контакт удаляется из базы данных и из интерфейса.
- После каждой операции список перечитывается из БД, а интерфейс обновляется через `ObservableCollection`.
- Операции сохранения и удаления обёрнуты в `try-catch` для отображения ошибок пользователю.

## Лабораторная работа 14

Тема: управление жизненным циклом `DbContext` в MVVM-приложениях.

Цель: исправить проблему долгоживущего `DbContext` в WPF/MVVM-приложении и перейти на `IDbContextFactory`.

Что реализовано:

- `AddDbContext<PhoneBookDbContext>` заменён на `AddDbContextFactory<PhoneBookDbContext>`.
- `ContactsListViewModel` и `ContactEditViewModel` получают `IDbContextFactory<PhoneBookDbContext>`.
- Чтение, создание, редактирование и удаление выполняются через короткоживущие контексты внутри `using`.
- Редактирование реализовано по паттерну Fetch-Modify-Save: найти контакт в новом контексте, изменить свойства, вызвать `SaveChanges()`.
- Удаление выполняется через новый контекст с поиском сущности по `Id`.

## Команды

```powershell
dotnet restore
dotnet tool restore
dotnet build PhoneBookLab12.sln
dotnet build LAB13.sln
dotnet build LAB14.sln
dotnet run --project PhoneBook\PhoneBook.csproj
```

Команда обратной инженерии, использованная для генерации модели:

```powershell
dotnet tool run dotnet-ef dbcontext scaffold "Data Source=PhoneBookDB_Веселков_2407sa1.db" Microsoft.EntityFrameworkCore.Sqlite --project PhoneBook\PhoneBook.csproj --startup-project PhoneBook\PhoneBook.csproj --context PhoneBookDbContext --context-dir Data --output-dir Models --no-onconfiguring --force
```

## Структура

- `LAB13.sln` - отдельный файл решения Visual Studio для лабораторной 13.
- `LAB13.md` - отдельный отчёт по лабораторной 13.
- `LAB14.sln` - отдельный файл решения Visual Studio для лабораторной 14.
- `LAB14.md` - отдельный отчёт по лабораторной 14.
- `PhoneBook/PhoneBookDB_Веселков_2407sa1.db` - готовая база данных SQLite.
- `PhoneBook/Models/Contact.cs` - сущность, полученная через scaffolding.
- `PhoneBook/Data/PhoneBookDbContext.cs` - контекст данных, полученный через scaffolding.
- `PhoneBook/ViewModels/ContactsListViewModel.cs` - чтение, фильтрация и удаление контактов.
- `PhoneBook/ViewModels/ContactEditViewModel.cs` - добавление и редактирование контактов.
- `PhoneBook/Commands/RelayCommand.cs` - команды для кнопок WPF.
- `PhoneBook/App.xaml.cs` - регистрация `DbContext` и ViewModel в DI.
- `Database/create_phonebook_sqlite.sql` - SQL-скрипт создания таблицы и начальных записей.

## Контрольные вопросы к лабораторной 13

1. `DbSet<TEntity>` представляет таблицу базы данных и позволяет выполнять LINQ-запросы, добавлять, изменять и удалять сущности.
2. `SaveChanges()` нужен, потому что изменения в объектах сначала фиксируются Change Tracker в памяти, а SQL-команды выполняются только при сохранении.
3. Если сущность уже отслеживается контекстом, достаточно изменить её свойства. `Update()` нужен, когда объект был получен вне текущего контекста и его нужно явно пометить как изменённый.
4. Change Tracker хранит состояния сущностей: `Added`, `Modified`, `Deleted`, `Unchanged`. При большом количестве данных отслеживание увеличивает расход памяти, поэтому для чтения можно использовать `AsNoTracking()`.
5. Если удалить неотслеживаемый объект, его нужно сначала присоединить к контексту или найти через текущий контекст. В проекте удаление выполняется через `_context.Contacts.Find(id)`.
