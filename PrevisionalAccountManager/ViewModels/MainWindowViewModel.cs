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
    public class MainWindowViewModel : ViewModel
    {
        private readonly ILoginService _loginService;
        private DataTemplate? _currentViewTemplate;
        private bool _isLoggedIn = false;

        private readonly LoginRootViewModel _loginRootViewModel;
        private readonly AccountBalanceRootViewModel _accountBalanceRootViewModel;

        public IRootViewModel? CurrentViewModel {
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
        public bool IsShowingAccountBalance => CurrentViewModel is AccountBalanceRootViewModel;

        public MainWindowViewModel() : this(GetRequiredInstance<ILoginService>())
        { }

        public MainWindowViewModel(ILoginService loginService)
        {
            _loginService = loginService;
            _loginRootViewModel = new();
            _loginRootViewModel.LoginAttempted += OnLoginRootAttempted;
            _accountBalanceRootViewModel = new();

            ShowAccountBalanceCommand = new RelayCommand(ShowAccountBalance, GetIsLoggedIn);
            LogoutCommand = new RelayCommand(Logout, GetIsLoggedIn);
            _ = CheckForApplicationUpdate();

            // Start with Login view
            ShowLoginView();
        }

        private bool GetIsLoggedIn()
        {
            return IsLoggedIn;
        }

        private void OnLoginRootAttempted(object? sender, bool success)
        {
            if ( success )
            {
                IsLoggedIn = true;
                ShowAccountBalance();
            }
        }

        private void ShowLoginView()
        {
            CurrentViewModel = _loginRootViewModel;
            CurrentViewTemplate = Application.Current.MainWindow?.FindResource("LoginViewTemplate") as DataTemplate;
            IsLoggedIn = false;
        }

        private void ShowAccountBalance()
        {
            if ( IsLoggedIn )
            {
                CurrentViewModel = _accountBalanceRootViewModel;
                CurrentViewTemplate = Application.Current.MainWindow?.FindResource("AccountBalanceViewTemplate") as DataTemplate;
            }
        }

        private void Logout()
        {
            _loginService.ClearCurrentSession();
            IsLoggedIn = false;
            _loginRootViewModel.Username = string.Empty;
            _loginRootViewModel.Password = string.Empty;
            _loginRootViewModel.ErrorMessage = string.Empty;
            ShowLoginView();
        }
    }
}