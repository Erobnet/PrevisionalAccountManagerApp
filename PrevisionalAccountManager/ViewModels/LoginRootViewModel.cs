using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using NetEscapades.EnumGenerators;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Services;
using PrevisionalAccountManager.Utils;

namespace PrevisionalAccountManager.ViewModels;

public sealed class LoginRootViewModel : ViewModel, IRootViewModel
{
    private readonly ILoginService _loginService;
    private readonly IStyleService _styleService;

    public string Username {
        get;
        set {
            field = value;
            OnPropertyChanged();
            ClearMessages();
        }
    } = string.Empty;

    public string Password {
        get;
        set {
            field = value;
            OnPropertyChanged();
            ClearMessages();
        }
    } = string.Empty;

    public string ConfirmPassword {
        get;
        set {
            field = value;
            OnPropertyChanged();
            ClearMessages();
        }
    } = string.Empty;

    public string ErrorMessage {
        get;
        set {
            field = value;
            OnPropertyChanged();
            HasError = !string.IsNullOrEmpty(value);
        }
    } = string.Empty;

    public string SuccessMessage {
        get;
        set {
            field = value;
            OnPropertyChanged();
            HasSuccess = !string.IsNullOrEmpty(value);
        }
    } = string.Empty;

    public bool HasError {
        get;
        set {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool HasSuccess {
        get;
        set {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading {
        get;
        set {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsCreateAccountMode {
        get;
        set {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FormTitle));
            OnPropertyChanged(nameof(PrimaryButtonText));
            OnPropertyChanged(nameof(ToggleButtonText));
            ClearMessages();
            ClearFields();
        }
    }

    public bool IsPasswordVisible {
        get;
        set {
            field = value;
            OnPropertyChanged();
        }
    } = false;

    public bool IsConfirmPasswordVisible {
        get;
        set {
            field = value;
            OnPropertyChanged();
        }
    } = false;

    public ColorTheme SelectedTheme {
        get;
        set {
            field = value;
            _styleService.LoadStyleTheme(value);
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<ColorTheme> AvailableThemes => _styleService.AvailableThemes;

    public string FormTitle => IsCreateAccountMode ? "Create Account" : "Login";
    public string PrimaryButtonText => IsCreateAccountMode ? "Create Account" : "Login";
    public string ToggleButtonText => IsCreateAccountMode ? "Back to Login" : "Create New Account";

    public ICommand PrimaryCommand { get; }
    public ICommand ToggleModeCommand { get; }
    public ICommand BackupDatabaseCommand { get; }
    public ICommand ImportDatabaseCommand { get; }
    public ICommand TogglePasswordVisibilityCommand { get; }
    public ICommand ToggleConfirmPasswordVisibilityCommand { get; }

    public event EventHandler<bool>? LoginAttempted;

    public LoginRootViewModel() : this(GetRequiredInstance<ILoginService>(), GetRequiredInstance<IStyleService>())
    { }

    public LoginRootViewModel(ILoginService loginService, IStyleService styleManagerService)
    {
        _loginService = loginService;
        _styleService = styleManagerService;

        PrimaryCommand = new AsyncRelayCommand(ExecutePrimaryActionAsync, () => !IsLoading);
        ToggleModeCommand = new RelayCommand(ToggleMode, () => !IsLoading);
        TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
        ToggleConfirmPasswordVisibilityCommand = new RelayCommand(ToggleConfirmPasswordVisibility);
        BackupDatabaseCommand = new RelayCommand(BackupDatabaseToFile);
        ImportDatabaseCommand = new RelayCommand(ImportDatabase);

        // Initialize with default theme 
        SelectedTheme = _styleService.CurrentTheme;
        System.Diagnostics.Debug.WriteLine($"App started with default {SelectedTheme.ToStringFast()} ");
        // Initialize the database
        _ = InitializeAsync();
    }

    private void ImportDatabase()
    {
        var saveFileDialog = new OpenFileDialog();
        saveFileDialog.Title = "Save database";
        saveFileDialog.Filter = "Database files (*.db)|*.db";

        if ( saveFileDialog.ShowDialog() == true )
        {
            if ( MessageBox.Show("Are you sure you want to import this database ? this will remove the current one and replace it, make sure to backup first", "Import database", MessageBoxButton.YesNo) != MessageBoxResult.Yes )
                return;

            try
            {
                var db = GetRequiredInstance<DatabaseContext>();
                db.ImportDatabaseAtPath(saveFileDialog.FileName);
                MessageBox.Show("Database imported successfully!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    private void BackupDatabaseToFile()
    {
        var saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Database files (*.db)|*.db";
        saveFileDialog.Title = "Save database";

        if ( saveFileDialog.ShowDialog() == true )
        {
            try
            {
                string saveFilePath = $"{saveFileDialog.FileName}";
                var db = GetRequiredInstance<DatabaseContext>();
                // Checkpoint WAL and close connection
                db.BackupDatabaseAtPath(saveFilePath);

                MessageBox.Show("Database backup created successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error backing up database: {ex.Message}");
            }
        }
    }

    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    private void ToggleConfirmPasswordVisibility()
    {
        IsConfirmPasswordVisible = !IsConfirmPasswordVisible;
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _loginService.InitializeAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to initialize login system.";
            System.Diagnostics.Debug.WriteLine($"Login initialization error: {ex.Message}");
        }
    }

    private async Task ExecutePrimaryActionAsync()
    {
        if ( IsCreateAccountMode )
        {
            await ExecuteCreateAccountAsync();
        }
        else
        {
            await ExecuteLoginAsync();
        }
    }

    private async Task ExecuteLoginAsync()
    {
        ClearMessages();
        IsLoading = true;

        try
        {
            // Validate input
            if ( string.IsNullOrWhiteSpace(Username) )
            {
                ErrorMessage = "Please enter a username.";
                LoginAttempted?.Invoke(this, false);
                return;
            }

            if ( string.IsNullOrWhiteSpace(Password) )
            {
                ErrorMessage = "Please enter a password.";
                LoginAttempted?.Invoke(this, false);
                return;
            }

            // Validate credentials using the login service
            var isValid = await _loginService.ValidateUserAsync(Username, Password);

            if ( isValid )
            {
                SuccessMessage = "Login successful!";
                LoginAttempted?.Invoke(this, true);
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
                LoginAttempted?.Invoke(this, false);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred during login. Please try again.";
            LoginAttempted?.Invoke(this, false);
            System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteCreateAccountAsync()
    {
        ClearMessages();
        IsLoading = true;

        try
        {
            // Validate input
            if ( string.IsNullOrWhiteSpace(Username) )
            {
                ErrorMessage = "Please enter a username.";
                return;
            }

            if ( Username.Length < 3 )
            {
                ErrorMessage = "Username must be at least 3 characters long.";
                return;
            }

            if ( string.IsNullOrWhiteSpace(Password) )
            {
                ErrorMessage = "Please enter a password.";
                return;
            }

            if ( Password.Length < 6 )
            {
                ErrorMessage = "Password must be at least 6 characters long.";
                return;
            }

            if ( string.IsNullOrWhiteSpace(ConfirmPassword) )
            {
                ErrorMessage = "Please confirm your password.";
                return;
            }

            if ( Password != ConfirmPassword )
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            // Check if user already exists
            if ( await _loginService.UserExistsAsync(Username) )
            {
                ErrorMessage = "Username already exists. Please choose a different username.";
                return;
            }

            // Create the user
            var success = await _loginService.CreateUserAsync(Username, Password);

            if ( success )
            {
                SuccessMessage = "Account created successfully! You can now login.";

                // Auto-switch to login mode after successful creation
                await Task.Delay(500); // Show success message for a moment
                IsCreateAccountMode = false;
            }
            else
            {
                ErrorMessage = "Failed to create account. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while creating the account. Please try again.";
            System.Diagnostics.Debug.WriteLine($"Account creation error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ToggleMode()
    {
        IsCreateAccountMode = !IsCreateAccountMode;
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    private void ClearFields()
    {
        Username = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        IsPasswordVisible = false;
        IsConfirmPasswordVisible = false;
    }

    public void Restart()
    {
        ClearMessages();
        IsPasswordVisible = false;
        IsConfirmPasswordVisible = false;
        IsCreateAccountMode = false;
        IsLoading = false;
        HasError = false;
        HasSuccess = false;
        ConfirmPassword = string.Empty;
        IsPasswordVisible = false;
    }
}

// Command implementations remain the same...
public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke() ?? true);
    }

    public async void Execute(object? parameter)
    {
        if ( !CanExecute(parameter) )
            return;

        try
        {
            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

[EnumExtensions]
public enum ColorTheme
{
    DarkTheme,
    LightTheme,
}