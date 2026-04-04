using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastTelecom.Application.DTOs;
using FastTelecom.Application.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public partial class BundlesViewModel : PageLoadViewModelBase
    {
        private readonly BundleService _bundleService;
        private long _basic;

        [ObservableProperty]
        private ObservableCollection<BundleDto> _bundles = [];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PurchaseCommand))]
        private bool _isPurchasing;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private string? _statusMessage;

        [ObservableProperty]
        private bool _showEmptyState;

        public BundlesViewModel(BundleService bundleService)
        {
            _bundleService = bundleService;
        }

        protected override async Task<bool> FetchAsync(CancellationToken ct)
        {
            ShowEmptyState = false;
            ErrorMessage   = null;
            StatusMessage  = null;
            Bundles.Clear();

            var result = await _bundleService.GetBundlesAsync(ct);

            if (!result.Success)
                return false;  

            _basic = result.Basic;

            foreach (var bundle in result.Bundles)
                Bundles.Add(bundle);

            ShowEmptyState = Bundles.Count == 0;
            return true;
        }


        private bool CanPurchase(BundleDto? bundle) =>
            !IsPurchasing && (bundle?.IsAvailable ?? false);

        [RelayCommand(CanExecute = nameof(CanPurchase))]
        private async Task PurchaseAsync(BundleDto bundle, CancellationToken ct)
        {
            IsPurchasing  = true;
            ErrorMessage  = null;
            StatusMessage = null;

            try
            {
                var result = await _bundleService.PurchaseBundleAsync(bundle.Id, _basic, ct);

                if (result.Success)
                    StatusMessage = result.Message;
                else
                    ErrorMessage = result.Error;
            }
            catch (OperationCanceledException) { }
            finally
            {
                IsPurchasing = false;
            }
        }
    }
}
