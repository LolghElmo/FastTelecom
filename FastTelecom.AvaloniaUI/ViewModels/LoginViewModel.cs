using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastTelecom.Application.DTOs;
using FastTelecom.Application.Services;
using FastTelecom.AvaloniaUI.Services;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly AuthenticationService _authService;
        private readonly INavigationService _nav;
        private readonly CredentialStore _credentials;

        private const int MaxAttempts = 5;
        private const int RetryDelayMs = 1500;

        private CancellationTokenSource? _loginCts;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _username = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password = string.Empty;

        [ObservableProperty] private bool _rememberMe;
        [ObservableProperty] private string? _errorMessage;
        [ObservableProperty] private string _attemptMessage = string.Empty;
        [ObservableProperty] private bool _isLoading;

        public bool ShowAttemptMessage => !string.IsNullOrEmpty(AttemptMessage);

        partial void OnAttemptMessageChanged(string value) =>
            OnPropertyChanged(nameof(ShowAttemptMessage));

        public ObservableCollection<SavedAccountViewModel> SavedAccounts { get; } = [];
        public bool HasSavedAccounts => SavedAccounts.Count > 0;

        public LoginViewModel(AuthenticationService authService, INavigationService nav, CredentialStore credentials)
        {
            _authService = authService;
            _nav = nav;
            _credentials = credentials;

            foreach (var a in _credentials.LoadAll())
                SavedAccounts.Add(new SavedAccountViewModel(a.Username, a.Password));

            SavedAccounts.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasSavedAccounts));
        }

        public async Task TryAutoLoginAsync(string username, string password)
        {
            Username = username;
            Password = password;
            RememberMe = true;
            await RunLoginLoopAsync();
        }

        private bool CanLogin() =>
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !IsLoading;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync() => await RunLoginLoopAsync();

        [RelayCommand]
        private void CancelLogin() => _loginCts?.Cancel();

        [RelayCommand]
        private async Task SelectAccountAsync(SavedAccountViewModel account)
        {
            if (IsLoading) return;

            foreach (var a in SavedAccounts) a.IsSelected = false;
            account.IsSelected = true;

            Username = account.Username;
            Password = account.Password;
            RememberMe = true;

            await RunLoginLoopAsync();
        }

        [RelayCommand]
        private void RemoveAccount(SavedAccountViewModel account)
        {
            _credentials.Remove(account.Username);
            SavedAccounts.Remove(account);
        }

        private async Task RunLoginLoopAsync()
        {
            _loginCts?.Cancel();
            _loginCts?.Dispose();
            _loginCts = new CancellationTokenSource();
            var ct = _loginCts.Token;

            ErrorMessage = null;
            AttemptMessage = string.Empty;
            IsLoading = true;
            LoginCommand.NotifyCanExecuteChanged();

            try
            {
                for (int attempt = 1; attempt <= MaxAttempts; attempt++)
                {
                    ct.ThrowIfCancellationRequested();

                    AttemptMessage = attempt == 1
                        ? "Connecting to server…"
                        : $"API not responding — Retrying ({attempt} of {MaxAttempts})…";

                    if (attempt > 1)
                        await Task.Delay(RetryDelayMs, ct);

                    LoginResultDto result;
                    try
                    {
                        result = await _authService.LoginAsync(
                            new LoginRequestDto { Username = Username, Password = Password }, ct);
                    }
                    catch (System.OperationCanceledException) { throw; }
                    catch
                    {
                        if (attempt == MaxAttempts)
                            ErrorMessage = "Couldn't reach the server after several attempts. Please check your connection and try again.";
                        continue;
                    }

                    if (!result.Success)
                    {
                        if (result.IsCredentialError)
                        {
                            foreach (var a in SavedAccounts) a.IsSelected = false;
                            ErrorMessage = "Incorrect username or password. Please try again.";
                            AttemptMessage = string.Empty;
                            return;
                        }

                        if (attempt == MaxAttempts)
                            ErrorMessage = "The server isn't responding. Please check your connection and try again.";
                        continue;
                    }

                    AttemptMessage = string.Empty;

                    if (RememberMe)
                        _credentials.Save(Username, Password);
                    else
                        _credentials.Clear();

                    _nav.NavigateTo<DashboardViewModel>(vm => vm.Load(result.Subscriber!));
                    return;
                }
            }
            catch (System.OperationCanceledException)
            {
                foreach (var a in SavedAccounts) a.IsSelected = false;
            }
            finally
            {
                IsLoading = false;
                AttemptMessage = string.Empty;
                LoginCommand.NotifyCanExecuteChanged();
                _loginCts?.Dispose();
                _loginCts = null;
            }
        }
    }
}
