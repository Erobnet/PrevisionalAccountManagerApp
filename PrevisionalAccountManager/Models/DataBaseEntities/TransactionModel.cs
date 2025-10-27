using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NetEscapades.EnumGenerators;

namespace PrevisionalAccountManager.Models.DataBaseEntities
{
    [PrimaryKey(nameof(Id))]
    public class TransactionModel : IEquatable<TransactionModel>, ITransactionModel
    {
        private static readonly TransactionModel _DefaultTransactionModel = new();

        [Key]
        public Guid Id { get; set; } = Guid.Empty;
        public Amount Amount { get; set; }
        [MaxLength(500)]
        public string Observations { get; set; } = "";
        public DateTime Date { get; set; }
        public int OwnerUserId { get; set; }
        public int? CategoryId { get; set; }

        // Navigation property
        [ForeignKey(nameof(CategoryId))]
        public CategoryModel? Category { get; set; }

        [JsonIgnore]
        public TransactionModel Model => this;

        public TransactionModel() { }

        public TransactionModel(TransactionModel data)
        {
            SetData(data);
        }

        public TransactionModel SetData(TransactionModel data)
        {
            Id = data.Id;
            Amount = data.Amount;
            Observations = data.Observations;
            Date = data.Date;
            Category = data.Category;
            CategoryId = data.CategoryId;
            return this;
        }



        public bool IsDataDefault()
        {
            return Observations.Equals(_DefaultTransactionModel.Observations)
                   && Amount.Equals(_DefaultTransactionModel.Amount)
                   && Date.Equals(_DefaultTransactionModel.Date)
                   && CategoryId == _DefaultTransactionModel.CategoryId
                ;
            ;
        }

        public override string ToString()
        {
            return $"{Observations} - {Amount} - {Date} - {Category?.Name ??
                                                           (CategoryId.HasValue
                                                               ? CategoryId.ToString()
                                                               : "No Category")}";
        }

        public bool Equals(TransactionModel? other)
        {
            return other is not null && Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
        {
            return obj is TransactionModel other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(TransactionModel left, TransactionModel right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TransactionModel left, TransactionModel right)
        {
            return !left.Equals(right);
        }
    }

    [EnumExtensions]
    public enum PeriodType
    {
        Once,
        Daily,
        Weekly,
        Monthly,
        BiMonthly,
        Quarter,
        Half,
        Yearly
    }
}