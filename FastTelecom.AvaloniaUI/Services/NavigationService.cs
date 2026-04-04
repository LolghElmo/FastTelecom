using FastTelecom.AvaloniaUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FastTelecom.AvaloniaUI.Services
{
    public sealed class NavigationService : INavigationService
    {
        private readonly IServiceProvider _services;

        public NavigationService(IServiceProvider services)
        {
            _services = services;
        }

        public ViewModelBase CurrentView { get; private set; } = null!;
        public bool ShowShell { get; private set; }

        public event Action? StateChanged;

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            CurrentView = _services.GetRequiredService<TViewModel>();
            ShowShell = true;
            StateChanged?.Invoke();
        }

        public void NavigateTo<TViewModel>(Action<TViewModel> configure) where TViewModel : ViewModelBase
        {
            var vm = _services.GetRequiredService<TViewModel>();
            configure(vm);
            CurrentView = vm;
            ShowShell = true;
            StateChanged?.Invoke();
        }

        public void NavigateToLogin()
        {
            CurrentView = _services.GetRequiredService<LoginViewModel>();
            ShowShell = false;
            StateChanged?.Invoke();
        }
    }
}