using FastTelecom.AvaloniaUI.ViewModels;
using System;

namespace FastTelecom.AvaloniaUI.Services

{
    public interface INavigationService
    {
        ViewModelBase CurrentView { get; }
        bool ShowShell { get; }

        void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
        void NavigateTo<TViewModel>(Action<TViewModel> configure) where TViewModel : ViewModelBase;
        void NavigateToLogin();

        event Action? StateChanged;
    }
}