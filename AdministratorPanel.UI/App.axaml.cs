using AdministratorPanel.Infrastructure.Extensions;
using AdministratorPanel.Infrastructure.Services;

using AdministratorPanel.UI.Services;

using AdministratorPanel.UI.ViewModels;
using AdministratorPanel.UI.ViewModels.Auth;
using AdministratorPanel.UI.ViewModels.LogCollector;
using AdministratorPanel.UI.ViewModels.ServerManagement;
using AdministratorPanel.UI.ViewModels.ServerMonitoring;

using AdministratorPanel.UI.Views;
using AdministratorPanel.UI.Views.Auth;

using Avalonia;
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

        _serviceProvider =
            services.BuildServiceProvider();

        var bootstrapper =
            _serviceProvider
                .GetRequiredService<ModuleBootstrapper>();

        bootstrapper.RegisterModules();


        var loginViewModel =
            _serviceProvider
                .GetRequiredService<LoginViewModel>();

        var loginWindow =
            new LoginView
            {
                DataContext = loginViewModel
            };

        loginViewModel.OnLoginSuccess = () =>
        {
            var mainWindow =
                new MainWindow(
                    _serviceProvider.GetRequiredService<MainWindowViewModel>(),
                    _serviceProvider.GetRequiredService<LogCollectorViewModel>(),
                    _serviceProvider.GetRequiredService<ServerManagementViewModel>(),
                    _serviceProvider.GetRequiredService<ServerMonitoringViewModel>());

            mainWindow.Show();

            loginWindow.Close();
        };

        if (ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = loginWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(
        IServiceCollection services)
    {
        services.AddAdministratorPanelInfrastructure();

        /*
            VIEW MODELS
        */

        services.AddSingleton<MainWindowViewModel>();

        services.AddSingleton<LogCollectorViewModel>();

        services.AddSingleton<ServerManagementViewModel>();

        services.AddSingleton<ServerMonitoringViewModel>();

        services.AddSingleton<LoginViewModel>();

        services.AddSingleton<IFileDialogService,
            FileDialogService>();
    }
}