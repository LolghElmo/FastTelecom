using CommunityToolkit.Mvvm.ComponentModel;
using FastTelecom.Application.DTOs;
using FastTelecom.Application.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public partial class ActiveBundlesViewModel : PageLoadViewModelBase
    {
        private readonly BundleService _bundleService;

        [ObservableProperty]
        private ObservableCollection<ActiveBundleDto> _bundles = [];

        [ObservableProperty]
        private bool _showEmptyState;

        [ObservableProperty]
        private ActiveBundleDto? _pinnedBundle;

        [ObservableProperty]
        private bool _hasPinnedBundle;

        public ActiveBundlesViewModel(BundleService bundleService)
        {
            _bundleService = bundleService;
        }


        protected override async Task<bool> FetchAsync(CancellationToken ct)
        {
            ShowEmptyState  = false;
            PinnedBundle    = null;
            HasPinnedBundle = false;
            Bundles.Clear();

            var result = await _bundleService.GetActiveBundlesAsync(ct);

            if (!result.Success)
                return false;  

            foreach (var bundle in result.Bundles)
                Bundles.Add(bundle);

            PinnedBundle    = result.Bundles.FirstOrDefault(b => b.IsOnline);
            HasPinnedBundle = PinnedBundle is not null;
            ShowEmptyState  = Bundles.Count == 0;
            return true;
        }
    }
}
