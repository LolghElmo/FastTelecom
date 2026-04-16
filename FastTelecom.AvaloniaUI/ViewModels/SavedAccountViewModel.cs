using CommunityToolkit.Mvvm.ComponentModel;

namespace FastTelecom.AvaloniaUI.ViewModels
{
    public partial class SavedAccountViewModel : ObservableObject
    {
        public string Username { get; }
        public string Password { get; }
        public string Initial => Username.Length > 0 ? Username[0].ToString().ToUpperInvariant() : "?";

        [ObservableProperty] private bool _isSelected;

        public SavedAccountViewModel(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
