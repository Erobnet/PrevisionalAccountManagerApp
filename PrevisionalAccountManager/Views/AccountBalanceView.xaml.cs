using System.Windows.Controls;
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
            viewModel.UpdateDateFilter(CalendarTransactionDateSelection.SelectedDates);
        }
    }
}