using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastTelecom.AvaloniaUI.Services;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly INavigationService _nav;
        private readonly CredentialStore _credentials;
        public UpdateViewModel Update { get; }
        public string AppVersion { get; } =
            typeof(MainWindowViewModel).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
                .Split('+')[0]
                ?? "0.0.0";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDashboardPage))]
        [NotifyPropertyChangedFor(nameof(IsBundlesPage))]
        [NotifyPropertyChangedFor(nameof(IsActiveBundlesPage))]
        [NotifyPropertyChangedFor(nameof(CurrentPageSubtitle))]
        private ViewModelBase _currentView = null!;
        [ObservableProperty] private bool _showShell;
        [ObservableProperty] private string _currentPageTitle = string.Empty;
        public bool IsDashboardPage     => CurrentView is DashboardViewModel;
        public bool IsBundlesPage       => CurrentView is BundlesViewModel;
        public bool IsActiveBundlesPage => CurrentView is ActiveBundlesViewModel;
        public string CurrentPageSubtitle => CurrentView switch
        {
            BundlesViewModel => "Choose the plan that fits your needs",
            _ => string.Empty,
        };
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOverlayVisible))]
        private bool _isPageLoading;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOverlayVisible))]
        private bool _isPageFailed;

        [ObservableProperty] private string _pageLoadingMessage = string.Empty;

        public bool IsOverlayVisible => IsPageLoading || IsPageFailed || Update.ShowUpdatePrompt;

        private PageLoadViewModelBase? _trackedPage;

        public MainWindowViewModel(INavigationService nav, CredentialStore credentials)
        {
            _nav = nav;
            _credentials = credentials;
            _nav.StateChanged += OnNavigationChanged;

            Update = new UpdateViewModel();
            Update.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(UpdateViewModel.ShowUpdatePrompt))
                    OnPropertyChanged(nameof(IsOverlayVisible));
            };
        }
        public void NavigateToLogin() => _nav.NavigateToLogin();
        public async Task TryAutoLoginAsync(string username, string password)
        {
            _nav.NavigateToLogin();
            if (_nav.CurrentView is LoginViewModel loginVm)
                await loginVm.TryAutoLoginAsync(username, password);
        }

        private void OnNavigationChanged()
        {
            if (_trackedPage is not null)
                _trackedPage.PropertyChanged -= OnTrackedPagePropertyChanged;

            CurrentView = _nav.CurrentView;
            ShowShell = _nav.ShowShell;

            _trackedPage = CurrentView as PageLoadViewModelBase;

            if (_trackedPage is not null)
            {
                _trackedPage.PropertyChanged += OnTrackedPagePropertyChanged;
                SyncOverlayState();
            }
            else
            {
                IsPageLoading = false;
                IsPageFailed = false;
                PageLoadingMessage = string.Empty;
            }

            CurrentPageTitle = CurrentView switch
            {
                DashboardViewModel => "Dashboard",
                BundlesViewModel => "Bundles",
                ActiveBundlesViewModel => "My Bundles",
                LoginViewModel => "Sign In",
                _ => string.Empty,
            };

            if (CurrentView is DashboardViewModel && !Update.IsUpdateAvailable)
                _ = Update.CheckForUpdatesCommand.ExecuteAsync(null);
        }

        private void OnTrackedPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is
                nameof(PageLoadViewModelBase.IsLoading) or
                nameof(PageLoadViewModelBase.HasFailed) or
                nameof(PageLoadViewModelBase.LoadingMessage))
            {
                SyncOverlayState();
            }
        }

        private void SyncOverlayState()
        {
            if (_trackedPage is null) return;
            IsPageLoading = _trackedPage.IsLoading;
            IsPageFailed = _trackedPage.HasFailed;
            PageLoadingMessage = _trackedPage.LoadingMessage;
        }

        [RelayCommand]
        private void NavigateToDashboard() => _nav.NavigateTo<DashboardViewModel>();

        [RelayCommand]
        private void NavigateToBundles() =>
            _nav.NavigateTo<BundlesViewModel>(vm => _ = vm.LoadAsync());

        [RelayCommand]
        private void NavigateToActiveBundles() =>
            _nav.NavigateTo<ActiveBundlesViewModel>(vm => _ = vm.LoadAsync());

        [RelayCommand]
        private void Logout()
        {
            _credentials.Clear();
            _nav.NavigateToLogin();
        }

        [RelayCommand]
        private void RetryPage() => _trackedPage?.RetryLoad();
    }
}
