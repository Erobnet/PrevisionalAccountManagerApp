using Microsoft.EntityFrameworkCore;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Models.DataBaseEntities;
using PrevisionalAccountManager.Utils;
using PrevisionalAccountManager.ViewModels;

namespace PrevisionalAccountManager.Services
{
    public interface ITransactionService
    {
        IReadOnlyList<TransactionModel> GetAllTransactions();
        Task<IReadOnlyList<TransactionModel>> GetAllTransactionsAsync();
        void AddTransaction(ITransactionModel entity);
        void AddTransaction(IReadOnlyList<TransactionModel> entities);
        Task AddTransactionAsync(ITransactionModel entity);
        void RemoveTransaction(ITransactionModel transactionViewModel);

        void RemoveTransaction<TTransactionModel>(IReadOnlyList<TTransactionModel> transactionModels)
            where TTransactionModel : ITransactionModel;

        Task RemoveTransactionAsync(ITransactionModel transactionViewModel);
        List<TransactionModel> GetTransactionsForDate(DateTime date);
        Task<List<TransactionModel>> GetTransactionsForDateAsync(DateTime date);
        List<TransactionModel> GetTransactionsForDateRange(DateRange range, List<TransactionModel>? transactions = null);
        IReadOnlyList<TransactionModel> GetTransactions(TransactionSearchInput searchInput);
        Task<List<TransactionModel>> GetTransactionsForDateRangeAsync(DateRange range);

        DateRangeBalanceViewModel CalculateBalanceForDateRange<TTransactionModel>(IReadOnlyList<TTransactionModel> selectionList, DateRange date, Amount startingBalance = default)
            where TTransactionModel : ITransactionModel;
    }

    public class TransactionService(DatabaseContext ctx, ILoginService loginService) : IDisposable, ITransactionService
    {
        public IReadOnlyList<TransactionModel> GetAllTransactions()
        {
            return ctx.Transactions
                .GetCommonTransactionQuery(loginService)
                .ToList();
        }

        public async Task<IReadOnlyList<TransactionModel>> GetAllTransactionsAsync()
        {
            return await ctx.Transactions
                .GetCommonTransactionQuery(loginService)
                .ToListAsync();
        }

        public List<TransactionModel> GetTransactionsForDate(DateTime date)
        {
            return GetTransactionForDateImplem(date)
                .ToList();
        }

        public Task<List<TransactionModel>> GetTransactionsForDateAsync(DateTime date)
        {
            return GetTransactionForDateImplem(date)
                .ToListAsync();
        }

        private IQueryable<TransactionModel> GetTransactionForDateImplem(DateTime date)
        {
            return ctx.Transactions.GetTransactionInDateRangeQuery(new DateRange(date, date), loginService);
        }

        public List<TransactionModel> GetTransactionsForDateRange(DateRange range, List<TransactionModel>? transactions = null)
        {
            transactions ??= [];
            transactions.AddRange(ctx.Transactions.GetTransactionInDateRangeQuery(range, loginService));
            return transactions;
        }

        public IReadOnlyList<TransactionModel> GetTransactions(TransactionSearchInput searchInput)
        {
            if ( searchInput == default )
                return Array.Empty<TransactionModel>();

            return ctx.Transactions
                .Where(t => (string.IsNullOrWhiteSpace(searchInput.Observations) || t.Observations.Contains(searchInput.Observations))
                            && (searchInput.Amount == 0 || t.Amount == searchInput.Amount)
                            && (searchInput.Date == default || (searchInput.Date.IsSingleDay ? t.Date <= searchInput.Date.Start : t.Date >= searchInput.Date.Start && t.Date <= searchInput.Date.End))
                            && (!searchInput.CategoryId.HasValue || searchInput.CategoryId == t.CategoryId)
                )
                .GetCommonTransactionQuery(loginService)
                .ToArray();
        }

        public Task<List<TransactionModel>> GetTransactionsForDateRangeAsync(DateRange range)
        {
            return ctx.Transactions
                .GetTransactionInDateRangeQuery(range, loginService)
                .ToListAsync();
        }

        public void AddTransaction(ITransactionModel entity)
        {
            AddTransactionImplementation(entity);
            ctx.SaveChanges();
        }

        public async Task AddTransactionAsync(ITransactionModel entity)
        {
            AddTransactionImplementation(entity);
            await ctx.SaveChangesAsync();
        }

        public void AddTransaction(IReadOnlyList<TransactionModel> entities)
        {
            for ( var index = 0; index < entities.Count; index++ )
            {
                var entity = entities[index];
                SetOwnership(entity);
                SetRelationStateUnchanged(entity);
            }
            ctx.Transactions.AddRange(entities);
            ctx.SaveChanges();
        }

        private void SetOwnership(TransactionModel entity)
        {
            entity.OwnerUserId = loginService.CurrentUserId!.Value;
        }

        private void AddTransactionImplementation(ITransactionModel entity)
        {
            SetOwnership(entity.Model);
            SetRelationStateUnchanged(entity.Model);
            ctx.Transactions.Add(entity.Model);
        }

        private void SetRelationStateUnchanged(TransactionModel entity)
        {
            if ( entity.Category != null && entity.Category.Id != 0 )
            {
                ctx.Entry(entity.Category).State = EntityState.Unchanged;
            }
        }

        public void RemoveTransaction(ITransactionModel transactionViewModel)
        {
            var entity = ctx.Transactions.Find(transactionViewModel.Id);
            if ( entity is not null )
            {
                ctx.Transactions.Remove(entity);
                ctx.SaveChanges();
            }
        }

        public void RemoveTransaction<TTransactionModel>(IReadOnlyList<TTransactionModel> transactionModels)
            where TTransactionModel : ITransactionModel
        {
            for ( var index = 0; index < transactionModels.Count; index++ )
            {
                TTransactionModel? model = transactionModels[index];
                var entity = ctx.Transactions.Find(model.Id);
                if ( entity is not null )
                {
                    ctx.Transactions.Remove(entity);
                }
            }
            ctx.SaveChanges();
        }

        public async Task RemoveTransactionAsync(ITransactionModel transactionViewModel)
        {
            var entity = await ctx.Transactions.FindAsync(transactionViewModel.Id);
            if ( entity is not null )
            {
                ctx.Transactions.Remove(entity);
                await ctx.SaveChangesAsync();
            }
        }

        public DateRangeBalanceViewModel CalculateBalanceForDateRange<TTransactionModel>(IReadOnlyList<TTransactionModel> selectionList, DateRange date, Amount startingBalance = default)
            where TTransactionModel : ITransactionModel
        {
            var dayBalance = new DateRangeBalanceViewModel {
                DateRange = date,
                StartingBalance = startingBalance,
            };

            foreach ( var transactionViewModel in selectionList )
            {
                if ( transactionViewModel.Amount < 0 )
                {
                    dayBalance.TotalExpenses += transactionViewModel.Amount;
                }
                else
                {
                    dayBalance.TotalIncome += transactionViewModel.Amount;
                }
            }

            return dayBalance;
        }

        public void Dispose()
        {
            ctx?.Dispose();
        }
    }

    internal static class TransactionQueryExtensions
    {
        extension(IQueryable<TransactionModel> transactionModels)
        {
            internal IQueryable<TransactionModel> GetTransactionInDateRangeQuery(DateRange range, ILoginService loginService)
            {
                return (range.IsSingleDay
                        ? transactionModels.Where(t => t.Date.Date == range.Start.Date)
                        : transactionModels.Where(t => t.Date.Date >= range.Start.Date && t.Date.Date <= range.End.Date))
                    .GetCommonTransactionQuery(loginService);
            }

            internal IQueryable<TransactionModel> GetCommonTransactionQuery(ILoginService loginService)
            {
                return transactionModels
                    .Where(t => t.OwnerUserId == loginService.CurrentUserId)
                    .OrderByDescending(t => t.Date);
            }
        }


    }
}