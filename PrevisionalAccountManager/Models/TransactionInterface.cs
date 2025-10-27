using PrevisionalAccountManager.Models.DataBaseEntities;

namespace PrevisionalAccountManager.Models
{
    public interface ITransactionModel
    {
        Guid Id { get; set; }
        Amount Amount { get; set; }
        string Observations { get; set; }
        DateTime Date { get; set; }
        TransactionModel Model { get; }
        
    }
}
