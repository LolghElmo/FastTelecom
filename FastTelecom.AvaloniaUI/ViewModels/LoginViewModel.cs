using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastTelecom.Application.DTOs;
using FastTelecom.Application.Services;
using FastTelecom.AvaloniaUI.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly AuthenticationService _authService;
        private readonly INavigationService    _nav;
        private readonly CredentialStore       _credentials;

        private const int MaxAttempts  = 5;
        private const int RetryDelayMs = 1500;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _username = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password = string.Empty;

        [ObservableProperty] private bool    _rememberMe;
        [ObservableProperty] private string? _errorMessage;
        [ObservableProperty] private string  _attemptMessage = string.Empty;
        [ObservableProperty] private bool    _isLoading;

        public bool ShowAttemptMessage => !string.IsNullOrEmpty(AttemptMessage);

        partial void OnAttemptMessageChanged(string value) =>
            OnPropertyChanged(nameof(ShowAttemptMessage));

        public LoginViewModel(
            AuthenticationService authService,
            INavigationService    nav,
            CredentialStore       credentials)
        {
            _authService = authService;
            _nav         = nav;
            _credentials = credentials;
        }

        public async Task TryAutoLoginAsync(string username, string password)
        {
            Username   = username;
            Password   = password;
            RememberMe = true;
            await RunLoginLoopAsync(CancellationToken.None);
        }

        private bool CanLogin() =>
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !IsLoading;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync(CancellationToken ct) =>
            await RunLoginLoopAsync(ct);


        private async Task RunLoginLoopAsync(CancellationToken ct)
        {
            ErrorMessage   = null;
            AttemptMessage = string.Empty;
            IsLoading      = true;
            LoginCommand.NotifyCanExecuteChanged();

            try
            {
                for (int attempt = 1; attempt <= MaxAttempts; attempt++)
                {
                    ct.ThrowIfCancellationRequested();

                    AttemptMessage = attempt == 1
                        ? $"1 / {MaxAttempts} — Connecting…"
                        : $"{attempt} / {MaxAttempts} — Retrying…";

                    if (attempt > 1)
                        await Task.Delay(RetryDelayMs, ct);

                    LoginResultDto result;
                    try
                    {
                        result = await _authService.LoginAsync(
                            new LoginRequestDto { Username = Username, Password = Password }, ct);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch
                    {
                        if (attempt == MaxAttempts)
                            ErrorMessage = "Couldn't reach the server after several attempts. " +
                                           "Please check your connection and try again.";
                        continue;
                    }

                    if (!result.Success)
                    {
                        if (result.IsCredentialError)
                        {
                            ErrorMessage   = "Incorrect username or password. Please try again.";
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
            catch (OperationCanceledException) { }
            finally
            {
                IsLoading      = false;
                AttemptMessage = string.Empty;
                LoginCommand.NotifyCanExecuteChanged();
            }
        }
    }
}
