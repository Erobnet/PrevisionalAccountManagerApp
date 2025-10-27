using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Models.DataBaseEntities;
using PrevisionalAccountManager.Views;
using PrevisionalAccountManager.Services;
using PrevisionalAccountManager.Utils;

namespace PrevisionalAccountManager.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly ILoginService _loginService;
        private DataTemplate? _currentViewTemplate;
        private bool _isLoggedIn = false;

        private readonly LoginViewModel _loginViewModel;
        private readonly AccountBalanceViewModel _accountBalanceViewModel;

        public IViewModel? CurrentViewModel {
            get;
            set {
                field = value;
                field?.Restart();
                OnPropertyChanged();
            }
        }

        public DataTemplate? CurrentViewTemplate {
            get => _currentViewTemplate;
            set {
                _currentViewTemplate = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoggedIn {
            get => _isLoggedIn;
            set {
                _isLoggedIn = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowAccountBalanceCommand { get; }
        public ICommand LogoutCommand { get; }
        public bool IsShowingAccountBalance => CurrentViewModel is AccountBalanceViewModel;

        public MainWindowViewModel() : this(GetRequiredInstance<ILoginService>())
        { }

        public MainWindowViewModel(ILoginService loginService)
        {
            _loginService = loginService;
            _loginViewModel = new();
            _loginViewModel.LoginAttempted += OnLoginAttempted;
            _accountBalanceViewModel = new();

            ShowAccountBalanceCommand = new RelayCommand(ShowAccountBalance, GetIsLoggedIn);
            LogoutCommand = new RelayCommand(Logout, GetIsLoggedIn);

            // Start with Login view
            ShowLoginView();
        }

        private bool GetIsLoggedIn()
        {
            return IsLoggedIn;
        }

        private void OnLoginAttempted(object? sender, bool success)
        {
            if ( success )
            {
                IsLoggedIn = true;
                ShowAccountBalance();
            }
        }

        private void ShowLoginView()
        {
            CurrentViewModel = _loginViewModel;
            CurrentViewTemplate = Application.Current.MainWindow?.FindResource("LoginViewTemplate") as DataTemplate;
            IsLoggedIn = false;
        }

        private void ShowAccountBalance()
        {
            if ( IsLoggedIn )
            {
                CurrentViewModel = _accountBalanceViewModel;
                CurrentViewTemplate = Application.Current.MainWindow?.FindResource("AccountBalanceViewTemplate") as DataTemplate;
            }
        }

        private void Logout()
        {
            _loginService.ClearCurrentSession();
            IsLoggedIn = false;
            _loginViewModel.Username = string.Empty;
            _loginViewModel.Password = string.Empty;
            _loginViewModel.ErrorMessage = string.Empty;
            ShowLoginView();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface IViewModel
    {
        public void Restart();
    }
}