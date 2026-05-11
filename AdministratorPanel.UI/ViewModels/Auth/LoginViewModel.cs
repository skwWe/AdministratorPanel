using AdministratorPanel.Infrastructure.Security;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AdministratorPanel.UI.ViewModels.Auth
{
    public partial class LoginViewModel
        : ObservableObject
    {
        private readonly ISshSessionService _session;

        [ObservableProperty]
        private string _userName = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public Action? OnLoginSuccess { get; set; }

        public LoginViewModel(
            ISshSessionService session)
        {
            _session = session;
        }

        [RelayCommand]
        private void Login()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(UserName))
            {
                ErrorMessage =
                    "Введите SSH пользователя.";

                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage =
                    "Введите пароль.";

                return;
            }

            _session.Login(
                UserName,
                Password);

            OnLoginSuccess?.Invoke();
        }
    }
}