using Avalonia.Controls;
using Avalonia.Input;
using FastTelecom.AvaloniaUI.ViewModels;

namespace FastTelecom.AvaloniaUI.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        // ── Keyboard shortcuts ────────────────────────────────────────────────

        /// <summary>Enter in the username field → move focus to the password field.</summary>
        private void UsernameBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter or Key.Tab)
            {
                PasswordBox.Focus();
                e.Handled = true;
            }
        }

        /// <summary>Enter in the password field → trigger the login command.</summary>
        private void PasswordBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is LoginViewModel vm)
            {
                if (vm.LoginCommand.CanExecute(null))
                    vm.LoginCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
