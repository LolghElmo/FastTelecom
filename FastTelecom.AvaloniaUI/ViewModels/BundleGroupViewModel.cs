using CommunityToolkit.Mvvm.ComponentModel;
using FastTelecom.Application.DTOs;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public sealed partial class BundleGroupViewModel : ObservableObject
    {
        public string TypeName { get; }
        public string Header => $"{TypeName}  ({Bundles.Count})";
        public ObservableCollection<ActiveBundleDto> Bundles { get; }
        [ObservableProperty]
        private bool _isExpanded;
        public BundleGroupViewModel(string typeName, IEnumerable<ActiveBundleDto> bundles,
                                    bool isExpanded = false)
        {
            TypeName = typeName;
            Bundles = new ObservableCollection<ActiveBundleDto>(bundles);
            _isExpanded = isExpanded;
        }
    }
}
