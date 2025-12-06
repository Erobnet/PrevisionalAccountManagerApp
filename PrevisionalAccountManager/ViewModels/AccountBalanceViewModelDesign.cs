using System.Collections.ObjectModel;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Models.DataBaseEntities;
using PrevisionalAccountManager.Utils;

namespace PrevisionalAccountManager.ViewModels;

public class AccountBalanceRootViewModelDesign : AccountBalanceRootViewModel
{
    public override DateRangeBalanceViewModel SelectedDateBalanceViewModel => new DateRangeBalanceViewModel {
        TotalIncome = new Amount(2500.00m),
        TotalExpenses = new Amount(-1335.89m),
        DateRange = new DateRange(new DateTime(2024, 12, 1), new DateTime(2024, 12, 5))
    };

    public AccountBalanceRootViewModelDesign()
    {
        FilteredTransactions = new ObservableCollectionWithItemNotify<TransactionViewModel>(
            [
                new(new TransactionModel {
                    Id = Guid.NewGuid(),
                    Date = new DateTime(2024, 12, 1),
                    Amount = new Amount(150.50m),
                    Observations = "Courses Carrefour",
                    Category = new CategoryModel { Id = 1, Name = "Alimentation" }
                }),
                new(new TransactionModel {
                    Id = Guid.NewGuid(),
                    Date = new DateTime(2024, 12, 2),
                    Amount = new Amount(-45.90m),
                    Observations = "Essence Total",
                    Category = new CategoryModel { Id = 2, Name = "Transport" }
                }),
                new(new TransactionModel {
                    Id = Guid.NewGuid(),
                    Date = new DateTime(2024, 12, 3),
                    Amount = new Amount(-1200.00m),
                    Observations = "Loyer décembre",
                    Category = new CategoryModel { Id = 3, Name = "Logement" }
                }),
                new(new TransactionModel {
                    Id = Guid.NewGuid(),
                    Date = new DateTime(2024, 12, 4),
                    Amount = new Amount(2500.00m),
                    Observations = "Salaire",
                    Category = new CategoryModel { Id = 4, Name = "Revenu" }
                }),
                new(new TransactionModel {
                    Id = Guid.NewGuid(),
                    Date = new DateTime(2024, 12, 5),
                    Amount = new Amount(-89.99m),
                    Observations = "Restaurant",
                    Category = new CategoryModel { Id = 5, Name = "Sorties" }
                })
            ]
        );

        Categories = new ObservableCollection<CategoryViewModel> {
            new(new CategoryModel { Id = 1, Name = "Alimentation" }),
            new(new CategoryModel { Id = 2, Name = "Transport" }),
            new(new CategoryModel { Id = 3, Name = "Logement" }),
            new(new CategoryModel { Id = 4, Name = "Loisirs" }),
            new(new CategoryModel { Id = 5, Name = "Sorties" })
        };
    }
}