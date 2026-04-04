using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public abstract partial class PageLoadViewModelBase : ViewModelBase
    {
        private const int MaxAttempts          = 5;
        private const int ShowOverlayThresholdMs = 400;   
        private const int RetryDelayMs          = 1500;

        [ObservableProperty] private bool   _isLoading;
        [ObservableProperty] private bool   _hasFailed;
        [ObservableProperty] private string _loadingMessage = string.Empty;

        private CancellationTokenSource? _loadCts;


        public async Task LoadAsync(CancellationToken externalCt = default)
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            var ct = _loadCts.Token;

            IsLoading = false;   
            HasFailed = false;
            LoadingMessage = $"1 / {MaxAttempts} — Connecting to server…";

            bool succeeded = false;
            bool cancelled = false;

            try
            {

                var firstFetch = FetchAsync(ct);
                var threshold  = Task.Delay(ShowOverlayThresholdMs, ct);

                if (await Task.WhenAny(firstFetch, threshold) == threshold)
                    IsLoading = true;   

                bool ok1;
                try   { ok1 = await firstFetch; }
                catch (OperationCanceledException) { throw; }
                catch { ok1 = false; }

                if (ok1)
                {
                    succeeded = true;   
                }
                else
                {
                    IsLoading = true;

                    for (int attempt = 2; attempt <= MaxAttempts; attempt++)
                    {
                        ct.ThrowIfCancellationRequested();

                        LoadingMessage = $"{attempt} / {MaxAttempts} — Retrying…";
                        await Task.Delay(RetryDelayMs, ct);

                        bool ok;
                        try   { ok = await FetchAsync(ct); }
                        catch (OperationCanceledException) { throw; }
                        catch { ok = false; }

                        if (ok) { succeeded = true; break; }
                    }
                }
            }
            catch (OperationCanceledException) { cancelled = true; }
            finally { IsLoading = false; }

            if (!succeeded && !cancelled)
                HasFailed = true;
        }

        public void RetryLoad() => _ = LoadAsync();
        protected abstract Task<bool> FetchAsync(CancellationToken ct);
    }
}
