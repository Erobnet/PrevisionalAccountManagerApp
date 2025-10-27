using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.ViewModels;

namespace PrevisionalAccountManager.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class AccountBalanceView
{
    public AccountBalanceView()
    {
        InitializeComponent();
    }

    private void TransactionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if ( DataContext is AccountBalanceViewModel viewModel )
        {
            // Update the ViewModel's selected transactions
            viewModel.OnTransactionListViewSelectionChanged(e.AddedItems.Cast<TransactionViewModel>().ToArray());
        }
    }

    private void CalendarTransactionDateSelection_OnSelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if ( DataContext is AccountBalanceViewModel viewModel )
        {
            // Update the ViewModel's selected transactions
            viewModel.UpdateDateFilter(CalendarTransactionDateSelection.SelectedDates);
        }
    }
}