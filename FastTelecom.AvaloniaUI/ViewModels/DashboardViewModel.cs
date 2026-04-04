using CommunityToolkit.Mvvm.ComponentModel;
using FastTelecom.Application.DTOs;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _subscriberId = "—";

        [ObservableProperty]
        private string _userProfile = "—";

        [ObservableProperty]
        private string _ipAddress = "—";

        [ObservableProperty]
        private string _contactPhone = "—";

        [ObservableProperty]
        private string _chargingType = "—";

        [ObservableProperty]
        private string _effectTime = "—";

        [ObservableProperty]
        private string _expireTime = "—";

        [ObservableProperty]
        private string _firstUseTime = "—";

        [ObservableProperty]
        private string _lastUseEndTime = "—";

        [ObservableProperty]
        private int _status;

        [ObservableProperty]
        private int _maxSessions;

        [ObservableProperty]
        private bool _isActive;

        public void Load(SubscriberDto subscriber)
        {
            SubscriberId = subscriber.SubscriberID ?? "—";
            UserProfile = subscriber.UserProfileID ?? "—";
            IpAddress = subscriber.IPAddress ?? "—";
            ContactPhone = subscriber.ContactTeleNo ?? "—";
            ChargingType = subscriber.ChargingType ?? "—";
            EffectTime = subscriber.EffectTime ?? "—";
            ExpireTime = subscriber.ExpireTime ?? "—";
            FirstUseTime = subscriber.FirstUseTime ?? "—";
            LastUseEndTime = subscriber.LastUseEndTime ?? "—";
            Status = subscriber.Status;
            MaxSessions = subscriber.MaxSessNumber;
            IsActive = subscriber.Status == 0;
        }
    }
}