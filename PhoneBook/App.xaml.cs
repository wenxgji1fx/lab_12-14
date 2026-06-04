using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhoneBook.Data;
using PhoneBook.ViewModels;

namespace PhoneBook;

public partial class App : Application
{
    public const string DatabaseFileName = "PhoneBookDB_Веселков_2407sa1.db";

    private readonly IHost _host;
    private IServiceScope? _applicationScope;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                var databasePath = Path.Combine(AppContext.BaseDirectory, DatabaseFileName);

                services.AddDbContextFactory<PhoneBookDbContext>(options =>
                    options.UseSqlite($"Data Source={databasePath}"));

                services.AddTransient<ContactEditViewModel>();
                services.AddTransient<ContactsListViewModel>();
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        _applicationScope = _host.Services.CreateScope();
        var mainWindow = _applicationScope.ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _applicationScope?.Dispose();
        await _host.StopAsync(TimeSpan.FromSeconds(5));
        _host.Dispose();

        base.OnExit(e);
    }
}
