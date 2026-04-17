using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public partial class UpdateViewModel : ViewModelBase
    {
        private readonly UpdateManager _updateManager = new(
            new GithubSource("https://github.com/LolghElmo/FastTelecom", null, false));
        private UpdateInfo? _pendingUpdate;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CheckForUpdatesCommand))]
        private bool _isCheckingForUpdates;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplyUpdateCommand))]
        private bool _isApplyingUpdate;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplyUpdateCommand))]
        private bool _isUpdateAvailable;
        [ObservableProperty] private bool _showUpdatePrompt;
        [ObservableProperty] private string _availableVersion = string.Empty;

        [RelayCommand(CanExecute = nameof(CanCheck))]
        private async Task CheckForUpdatesAsync()
        {
            IsCheckingForUpdates = true;
            try
            {
                _pendingUpdate = await _updateManager.CheckForUpdatesAsync();

                if (_pendingUpdate is not null)
                {
                    IsUpdateAvailable = true;
                    AvailableVersion = _pendingUpdate.TargetFullRelease.Version.ToString();
                    ShowUpdatePrompt = true;
                }
                else
                {
                    IsUpdateAvailable = false;
                    AvailableVersion = string.Empty;
                }
            }
            catch (Exception)
            {
                // nothing happens
            }
            finally
            {
                IsCheckingForUpdates = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanApply))]
        private async Task ApplyUpdateAsync()
        {
            if (_pendingUpdate is null) return;

            IsApplyingUpdate = true;
            try
            {
                await _updateManager.DownloadUpdatesAsync(_pendingUpdate);
                _updateManager.ApplyUpdatesAndRestart(_pendingUpdate);
            }
            catch (Exception)
            {
                IsApplyingUpdate = false;
            }
        }

        [RelayCommand]
        private void DismissUpdate()
        {
            ShowUpdatePrompt = false;
        }

        [RelayCommand]
        private void ShowPrompt()
        {
            ShowUpdatePrompt = true;
        }

        private bool CanCheck() => !IsCheckingForUpdates;
        private bool CanApply() => !IsApplyingUpdate && IsUpdateAvailable;
    }
}
