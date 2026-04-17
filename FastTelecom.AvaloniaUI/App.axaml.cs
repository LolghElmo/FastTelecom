using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FastTelecom.Application;
using FastTelecom.AvaloniaUI.Services;
using FastTelecom.AvaloniaUI.ViewModels;
using FastTelecom.AvaloniaUI.Views;
using FastTelecom.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FastTelecom.AvaloniaUI
{
    public partial class App : Avalonia.Application
    {
        public static ServiceProvider Services { get; private set; } = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();

            // Layer registrations
            services.AddInfrastructure();
            services.AddApplication();

            // Navigation
            services.AddSingleton<INavigationService, NavigationService>();

            // Credential storage (machine+user encrypted, persists across restarts)
            services.AddSingleton<CredentialStore>();

            // User preferences (sort mode etc., persisted to AppData JSON)
            services.AddSingleton<UserPreferencesService>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<BundlesViewModel>();
            services.AddSingleton<ActiveBundlesViewModel>();
            services.AddSingleton<MainWindowViewModel>();

            Services = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainVm          = Services.GetRequiredService<MainWindowViewModel>();
                var credentialStore = Services.GetRequiredService<CredentialStore>();

                var saved = credentialStore.TryLoad();
                if (saved.HasValue)
                {
                    // Saved credentials found - show the login page and attempt silent login
                    _ = mainVm.TryAutoLoginAsync(saved.Value.Username, saved.Value.Password);
                }
                else
                {
                    mainVm.NavigateToLogin();
                }

                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainVm,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
