using CommunityToolkit.Mvvm.ComponentModel;
using FastTelecom.Application.DTOs;
using System;
using System.Globalization;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        [ObservableProperty] private string _subscriberId = "—";
        [ObservableProperty] private bool _isActive;
        [ObservableProperty] private string _effectTime = "—";
        [ObservableProperty] private string _expireTime = "—";
        [ObservableProperty] private string _firstUseTime = "—";
        [ObservableProperty] private string _lastUseEndTime = "—";
        [ObservableProperty] private string _lastUseRelative = "—";

        public void Load(SubscriberDto subscriber)
        {
            SubscriberId = subscriber.SubscriberID ?? "—";
            IsActive = subscriber.Status == 0;

            EffectTime = FormatDate(subscriber.EffectTime);
            ExpireTime = FormatDate(subscriber.ExpireTime);
            FirstUseTime = FormatDate(subscriber.FirstUseTime);

            var lastUsed = ParseDate(subscriber.LastUseEndTime);
            LastUseEndTime = lastUsed.HasValue ? lastUsed.Value.ToString("MMM dd, yyyy") : "—";
            LastUseRelative = lastUsed.HasValue ? ToRelativeTime(lastUsed.Value) : "—";
        }

        private static DateTime? ParseDate(string? raw)
        {
            if (raw is null || raw.Length < 14) return null;
            return DateTime.TryParseExact(raw, "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                ? dt : null;
        }

        private static string FormatDate(string? raw)
        {
            var dt = ParseDate(raw);
            return dt.HasValue ? dt.Value.ToString("MMM dd, yyyy") : "—";
        }

        private static string ToRelativeTime(DateTime dt)
        {
            var diff = DateTime.Now - dt;

            if (diff.TotalSeconds < 60) return "just now";
            if (diff.TotalMinutes < 60)
            {
                int m = (int)diff.TotalMinutes;
                return $"{m} minute{S(m)} ago";
            }
            if (diff.TotalHours < 24)
            {
                int h = (int)diff.TotalHours;
                return $"{h} hour{S(h)} ago";
            }
            if (diff.TotalDays < 7)
            {
                int d = (int)diff.TotalDays;
                return $"{d} day{S(d)} ago";
            }
            if (diff.TotalDays < 30)
            {
                int w = (int)(diff.TotalDays / 7);
                return $"{w} week{S(w)} ago";
            }
            if (diff.TotalDays < 365)
            {
                int mo = (int)(diff.TotalDays / 30);
                return $"{mo} month{S(mo)} ago";
            }
            {
                int y = (int)(diff.TotalDays / 365);
                return $"{y} year{S(y)} ago";
            }
        }

        private static string S(int n) => n == 1 ? "" : "s";
    }
}
