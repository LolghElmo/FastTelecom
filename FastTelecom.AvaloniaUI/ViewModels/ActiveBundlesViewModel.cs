using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastTelecom.Application.DTOs;
using FastTelecom.Application.Services;
using FastTelecom.AvaloniaUI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public enum BundleSortMode { DateAscending, DateDescending, BySize }

    public partial class ActiveBundlesViewModel : PageLoadViewModelBase
    {
        private readonly BundleService _bundleService;
        private readonly INavigationService _nav;
        private readonly UserPreferencesService _prefs;

        private ActiveBundleDto[] _allBundles = [];
        [ObservableProperty] private ObservableCollection<ActiveBundleDto> _bundles = [];
        [ObservableProperty] private ObservableCollection<BundleGroupViewModel> _groupedBundles = [];
        [ObservableProperty] private ActiveBundleDto? _pinnedBundle;
        [ObservableProperty] private bool _hasPinnedBundle;
        [ObservableProperty] private bool _showEmptyState;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGroupedView))]
        [NotifyPropertyChangedFor(nameof(IsDateAscending))]
        [NotifyPropertyChangedFor(nameof(IsDateDescending))]
        private BundleSortMode _sortMode = BundleSortMode.DateAscending;

        public bool IsGroupedView => SortMode == BundleSortMode.BySize;
        public bool IsDateAscending => SortMode == BundleSortMode.DateAscending;
        public bool IsDateDescending => SortMode == BundleSortMode.DateDescending;

        public ActiveBundlesViewModel(BundleService bundleService, INavigationService nav, UserPreferencesService prefs)
        {
            _bundleService = bundleService;
            _nav = nav;
            _prefs = prefs;
            _sortMode = _prefs.BundleSortMode;
        }

        protected override async Task<bool> FetchAsync(CancellationToken ct)
        {
            ShowEmptyState = false;
            PinnedBundle = null;
            HasPinnedBundle = false;
            Bundles.Clear();
            GroupedBundles.Clear();

            var result = await _bundleService.GetActiveBundlesAsync(ct);
            if (!result.Success) return false;

            _allBundles = result.Bundles;

            PinnedBundle = _allBundles.FirstOrDefault(b => b.IsOnline);
            HasPinnedBundle = PinnedBundle is not null;
            ShowEmptyState = _allBundles.Length == 0;

            ApplySort();
            return true;
        }

        private void ApplySort()
        {
            Bundles.Clear();
            GroupedBundles.Clear();
            var all = _allBundles.ToList();
            var sticky = all.FirstOrDefault(b => b.Name.Contains("الافتراضية"));
            var others = all.Where(b => b != sticky);

            switch (SortMode)
            {
                case BundleSortMode.DateAscending:
                    if (sticky is not null) Bundles.Add(sticky);
                    foreach (var b in others.OrderBy(b => b.EffectiveDateValue ?? DateTime.MaxValue))
                        Bundles.Add(b);
                    break;

                case BundleSortMode.DateDescending:
                    if (sticky is not null) Bundles.Add(sticky);
                    foreach (var b in others.OrderByDescending(b => b.EffectiveDateValue ?? DateTime.MinValue))
                        Bundles.Add(b);
                    break;

                case BundleSortMode.BySize:
                    var groups = all
                        .GroupBy(b => b.TotalDisplay)
                        .OrderBy(g =>
                        {
                            var vol = g.First().VolumeMb;
                            return vol <= 0 ? long.MaxValue : vol;
                        })
                        .Select(g =>
                        {
                            bool hasLive = PinnedBundle is not null && g.Any(b => b == PinnedBundle);
                            return new BundleGroupViewModel(g.Key, g, isExpanded: hasLive);
                        });

                    foreach (var g in groups)
                        GroupedBundles.Add(g);
                    break;
            }
        }

        [RelayCommand]
        private void SetSortDateAscending()
        {
            SortMode = BundleSortMode.DateAscending;
            _prefs.BundleSortMode = SortMode;
            ApplySort();
        }

        [RelayCommand]
        private void SetSortDateDescending()
        {
            SortMode = BundleSortMode.DateDescending;
            _prefs.BundleSortMode = SortMode;
            ApplySort();
        }

        [RelayCommand]
        private void SetSortBySize()
        {
            SortMode = BundleSortMode.BySize;
            _prefs.BundleSortMode = SortMode;
            ApplySort();
        }

        [RelayCommand]
        private void GoToStore() =>
            _nav.NavigateTo<BundlesViewModel>(vm => _ = vm.LoadAsync());
    }
}
