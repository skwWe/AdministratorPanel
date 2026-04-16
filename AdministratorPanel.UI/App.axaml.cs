using AdministratorPanel.Infrastructure.Extensions;
using AdministratorPanel.Infrastructure.Services;
using AdministratorPanel.UI.ViewModels;
using AdministratorPanel.UI.ViewModels.LogCollector;
using AdministratorPanel.UI.Views;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace AdministratorPanel.UI;

public partial class App : Avalonia.Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();

        var bootstrapper = _serviceProvider.GetRequiredService<ModuleBootstrapper>();
        bootstrapper.RegisterModules();

        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow(
                _serviceProvider.GetRequiredService<MainWindowViewModel>(),
                _serviceProvider.GetRequiredService<LogCollectorViewModel>());

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddAdministratorPanelInfrastructure();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<LogCollectorViewModel>();
    }
}