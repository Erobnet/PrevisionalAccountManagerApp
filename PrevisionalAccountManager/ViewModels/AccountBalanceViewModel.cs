using System.Collections.ObjectModel;
using System.Windows.Input;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Models.DataBaseEntities;
using PrevisionalAccountManager.Services;
using PrevisionalAccountManager.Utils;

namespace PrevisionalAccountManager.ViewModels
{
    public class AccountBalanceRootViewModel : ViewModel, IRootViewModel
    {
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;
        private readonly List<DateRange> _selectedDateRanges;
        public PeriodType[] AvailablePeriodTypes { get; }
        public AmountType[] AmountOperations { get; }

        public AmountType SelectedOperation {
            get;
            set {
                if ( value == field )
                {
                    return;
                }
                field = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CategoryViewModel> Categories { get; protected init; }
        public ICommand AddTransactionCommand { get; }
        public ICommand RemoveTransactionCommand { get; }
        public ICommand DeleteSelectedTransactionCommand { get; }
        public ICommand SearchTransactionCommand { get; }

        public PeriodType SelectedPeriodType {
            get;
            set {
                if ( value == field )
                    return;

                field = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPeriodCountVisible));
            }
        }

        public int PeriodCount {
            get;
            set {
                field = value <= 0
                    ? 1
                    : value;

                OnPropertyChanged();
            }
        }

        public bool IsPeriodCountVisible => SelectedPeriodType != PeriodType.Once;

        public DateTime SelectedDate {
            set {
                field = value;
                NewTransactionViewModelDate = value;
                _selectedDateRanges.Clear();
                _selectedDateRanges.Add(new DateRange(field, field));
                UpdateSelectedDateTransactions(_transactionService.GetTransactionsForDate(field));
            }
        }

        public TransactionViewModel NewTransactionViewModel {
            get;
            init {
                field = value;
                OnPropertyChanged();
            }
        }

        public CategoryViewModel? SelectedCategory {
            get;
            set {
                field = value;
                NewTransactionViewModel.Category = value;
                OnPropertyChanged();
            }
        }

        public string NewCategoryName {
            get;
            set {
                field = value;
                OnPropertyChanged();
            }
        } = "";

        public virtual DateRangeBalanceViewModel SelectedDateBalanceViewModel => _transactionService.CalculateBalanceForDateRange(FilteredTransactions, _selectedDateRanges[0]);

        public ObservableCollectionWithItemNotify<TransactionViewModel> FilteredTransactions {
            get;
            protected init;
        }

        public List<TransactionViewModel> SelectedTransactions {
            get;
        }

        public AccountBalanceRootViewModel() : this(GetRequiredInstance<ITransactionService>(), GetRequiredInstance<ICategoryService>())
        { }

        private AccountBalanceRootViewModel(ITransactionService transactionService, ICategoryService categoryService)
        {
            AmountOperations = AmountTypeExtensions.GetValues();
            _transactionService = transactionService;
            _categoryService = categoryService;
            _selectedDateRanges = new(2);
            FilteredTransactions = new();
            SelectedTransactions = new();
            Categories = new();
            AvailablePeriodTypes = PeriodTypeExtensions.GetValues();
            NewTransactionViewModel = new TransactionViewModel();
            AddTransactionCommand = new RelayCommand(OnAddTransactionClick, CanAddTransaction);
            RemoveTransactionCommand = new RelayCommand<TransactionViewModel>(RemoveTransaction);
            SearchTransactionCommand = new RelayCommand(SearchTransaction, CanSearchTransaction);
            DeleteSelectedTransactionCommand = new RelayCommand(DeleteSelectedTransactions, CanDeleteSelectedTransactions);
            PeriodCount = 1;
            SelectedDate = DateTime.Today;
            SelectedOperation = AmountType.Credit;
            FilteredTransactions.ItemPropertyChanged += (_, e) => transactionService.UpdateTransaction(e.Item);
        }

        public void Restart()
        {
            PeriodCount = 1;
            SelectedTransactions.Clear();
            NewTransactionViewModel.ResetData();
            SelectedDate = DateTime.Today;
            OnPropertyChanged(nameof(NewTransactionViewModel));
            LoadCategories();
        }

        public void ApplyChangesAsync()
        {
            _transactionService.ApplyTransactionChangesAsync();
        }

        private bool CanDeleteSelectedTransactions()
        {
            return SelectedTransactions.IsNotEmpty();
        }

        private void SearchTransaction()
        {
            var transactionSearchInput = new TransactionSearchInput(NewTransactionViewModel.Amount * (int)SelectedOperation, _selectedDateRanges[0], NewTransactionViewModel.Category?.Id, NewTransactionViewModel.Observations);
            var searchResults = _transactionService.GetTransactions(transactionSearchInput);
            UpdateSelectedDateTransactions(searchResults);
        }

        private void DeleteSelectedTransactions()
        {
            _transactionService.RemoveTransaction(SelectedTransactions);
            SelectedTransactions.Clear();
            UpdateTransactionSelectionFromDateRanges(_selectedDateRanges.AsSpan());
            OnPropertyChanged(nameof(SelectedTransactions));
        }

        private CategoryViewModel AddNewCategory()
        {
            var newCategoryModel = _categoryService.AddCategory(NewCategoryName);
            var newCategoryViewModel = new CategoryViewModel(newCategoryModel);

            LoadCategories(); // Refresh the categories list
            SelectedCategory = newCategoryViewModel; // Auto-select the new category
            return newCategoryViewModel;
        }

        private void LoadCategories()
        {
            Categories.Clear();
            var categories = _categoryService.GetAllCategories();
            foreach ( var category in categories )
            {
                Categories.Add(new(category));
            }
            OnPropertyChanged(nameof(Categories));
        }

        private void OnAddTransactionClick()
        {
            if ( NewTransactionViewModel.Category == null && !string.IsNullOrWhiteSpace(NewCategoryName) )
            {
                NewTransactionViewModel.Category = AddNewCategory();
            }

            var transaction = new TransactionModel(NewTransactionViewModel.Model);
            transaction.Amount *= (int)SelectedOperation;
            var transactions = new List<TransactionModel>(PeriodCount) {
                transaction
            };
            int periodCount = PeriodCount - 1;
            if ( SelectedPeriodType > PeriodType.Once )
            {
                if ( SelectedPeriodType >= PeriodType.Monthly )
                {
                    for ( int i = 0; i < periodCount; i++ )
                    {
                        transaction = new(transaction);
                        transaction.Date = SelectedPeriodType switch {
                            PeriodType.Monthly => transaction.Date.AddMonths(1),
                            PeriodType.BiMonthly => transaction.Date.AddMonths(2),
                            PeriodType.Quarter => transaction.Date.AddMonths(3),
                            PeriodType.Half => transaction.Date.AddMonths(6),
                            PeriodType.Yearly => transaction.Date.AddYears(1),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        transactions.Add(transaction);
                    }
                }
                else
                {
                    TimeSpan periodDuration = SelectedPeriodType switch {
                        PeriodType.Daily => TimeSpan.FromDays(1),
                        PeriodType.Weekly => TimeSpan.FromDays(7),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    for ( int i = 0; i < periodCount; i++ )
                    {
                        transaction = new(transaction);
                        transaction.Date = transaction.Date.Add(periodDuration);
                        transactions.Add(transaction);
                    }
                }
            }
            _transactionService.AddTransaction(transactions);

            PeriodCount = 1;
            NewTransactionViewModel.ResetData();
            NewTransactionViewModelDate = DateTime.Today;
            UpdateTransactionSelectionFromDateRanges(_selectedDateRanges.AsSpan());
            OnPropertyChanged(nameof(NewTransactionViewModel));
        }

        public DateTime? NewTransactionViewModelDate {
            get => NewTransactionViewModel.Date != default ? NewTransactionViewModel.Date : null;
            set {
                NewTransactionViewModel.Date = value ?? default;
                OnPropertyChanged();
            }
        }

        private bool CanAddTransaction()
        {
            return NewTransactionViewModel.Amount != 0
                   && NewTransactionViewModel.Date != default
                   && (NewTransactionViewModel.Category != null || !string.IsNullOrWhiteSpace(NewCategoryName) || !string.IsNullOrWhiteSpace(NewTransactionViewModel.Observations));
        }

        private bool CanSearchTransaction()
        {
            return !NewTransactionViewModel.Model.IsDataDefault();
        }

        private void RemoveTransaction(TransactionViewModel transactionViewModel)
        {
            _transactionService.RemoveTransaction(transactionViewModel.Model);
            OnPropertyChanged(nameof(SelectedDateBalanceViewModel));
            OnPropertyChanged(nameof(FilteredTransactions));
        }

        public void UpdateDateFilter(IReadOnlyList<DateTime> selectedDates)
        {
            if ( selectedDates.Count == 1 )
            {
                SelectedDate = selectedDates[0];
                return;
            }

            // Remove duplicates and sort
            var sortedDates = selectedDates
                .Select(d => d.Date) // Ensure we only work with dates (no time component)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            if ( !sortedDates.IsNotEmpty() )
                return;

            _selectedDateRanges.Clear();
            DateTime rangeStart = sortedDates[0];
            DateTime rangeEnd = rangeStart;
            int transactionIndex = 1;
            for ( ; transactionIndex < sortedDates.Count; transactionIndex++ )
            {
                // Check if current date is consecutive to the previous one
                if ( sortedDates[transactionIndex] == rangeEnd.AddDays(1) )
                {
                    // Extend current range
                    rangeEnd = sortedDates[transactionIndex];
                }
                else
                {
                    // Current date is not consecutive, close current range and start new one
                    _selectedDateRanges.Add(new(rangeStart, rangeEnd));
                    rangeStart = sortedDates[transactionIndex];
                    rangeEnd = rangeStart;
                }
            }

            // Add the last range
            _selectedDateRanges.Add(new(rangeStart, rangeEnd));
            UpdateTransactionSelectionFromDateRanges(_selectedDateRanges.AsSpan(), transactionIndex - 1);
        }

        private void UpdateTransactionSelectionFromDateRanges(ReadOnlySpan<DateRange> selectedDateRanges, int transactionCount = 8)
        {
            var listModel = new List<TransactionModel>(transactionCount);
            for ( var index = 0; index < selectedDateRanges.Length; index++ )
            {
                listModel = _transactionService.GetTransactionsForDateRange(selectedDateRanges[index], listModel);
            }
            UpdateSelectedDateTransactions(listModel);
        }

        private void UpdateSelectedDateTransactions(IReadOnlyList<TransactionModel> listModel)
        {
            AddSelectedTransactions(listModel.Select(t => new TransactionViewModel(t)).ToList());
            RefreshTransactionByDate();
        }

        private void RefreshTransactionByDate()
        {
            OnPropertyChanged(nameof(SelectedDateBalanceViewModel));
        }


        public void OnTransactionListViewSelectionChanged(IReadOnlyList<TransactionViewModel> selectedItems)
        {
            SelectedTransactions.Clear();
            SelectedTransactions.AddRange(selectedItems);
        }

        private void AddSelectedTransactions(IReadOnlyList<TransactionViewModel> selectedItems)
        {
            FilteredTransactions.Clear();
            FilteredTransactions.AddRange(selectedItems);
            OnPropertyChanged(nameof(FilteredTransactions));
        }
    }

}