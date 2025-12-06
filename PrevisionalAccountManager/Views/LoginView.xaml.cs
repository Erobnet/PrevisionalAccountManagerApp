using System.Windows;
using System.Windows.Controls;
using PrevisionalAccountManager.ViewModels;

namespace PrevisionalAccountManager.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();

            // Handle password binding for PasswordBoxes
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            ConfirmPasswordBox.PasswordChanged += ConfirmPasswordBox_PasswordChanged;

            // Handle text changes for TextBoxes to sync with PasswordBoxes
            PasswordTextBox.TextChanged += PasswordTextBox_TextChanged;
            ConfirmPasswordTextBox.TextChanged += ConfirmPasswordTextBox_TextChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if ( DataContext is LoginRootViewModel viewModel && sender is PasswordBox passwordBox )
            {
                viewModel.Password = passwordBox.Password;
                // Sync with TextBox
                if ( PasswordTextBox.Text != passwordBox.Password )
                {
                    PasswordTextBox.Text = passwordBox.Password;
                }
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if ( DataContext is LoginRootViewModel viewModel && sender is PasswordBox passwordBox )
            {
                viewModel.ConfirmPassword = passwordBox.Password;
                // Sync with TextBox
                if ( ConfirmPasswordTextBox.Text != passwordBox.Password )
                {
                    ConfirmPasswordTextBox.Text = passwordBox.Password;
                }
            }
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ( sender is TextBox textBox )
            {
                // Sync with PasswordBox
                if ( PasswordBox.Password != textBox.Text )
                {
                    PasswordBox.Password = textBox.Text;
                }
            }
        }

        private void ConfirmPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ( sender is TextBox textBox )
            {
                // Sync with PasswordBox
                if ( ConfirmPasswordBox.Password != textBox.Text )
                {
                    ConfirmPasswordBox.Password = textBox.Text;
                }
            }
        }
    }
}