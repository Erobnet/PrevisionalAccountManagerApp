using PrevisionalAccountManager.Models;

namespace PrevisionalAccountManager.Services;

public record struct TransactionSearchInput(Amount Amount, DateTime Date, int? CategoryId, string? Observations);