using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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

    private void FilteredTransactionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if ( DataContext is AccountBalanceRootViewModel viewModel && ReferenceEquals(e.OriginalSource, FilteredTransactionsListView) )
        {
            // Update the ViewModel's selected transactions
            viewModel.OnTransactionListViewSelectionChanged(e.AddedItems.Cast<TransactionViewModel>().ToArray());
        }
    }

    private void CalendarTransactionDateSelection_OnSelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if ( DataContext is AccountBalanceRootViewModel viewModel )
        {
            viewModel.UpdateDateFilter(CalendarTransactionDateSelection.SelectedDates);
        }
    }

    private void Root_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if ( e.OriginalSource is CalendarItem && FilteredTransactionsListView.CaptureMouse() )
        { }
    }

    private void FilteredTransactionsListView_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if ( DataContext is AccountBalanceRootViewModel viewModel )
        {
            viewModel.ApplyChangesAsync();
        }
    }
}