using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Utils;

namespace PrevisionalAccountManager.ViewModels
{
    public struct DateRangeBalanceViewModel
    {
        public DateRange DateRange { get; set; }
        public Amount StartingBalance { get; init; }
        public Amount TotalIncome { get; set; }
        public Amount TotalExpenses { get; set; }
        public Amount EndingBalance => StartingBalance + TotalIncome + TotalExpenses;
    }
}