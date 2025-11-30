using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Utils;

namespace PrevisionalAccountManager.Services;

public record struct TransactionSearchInput(Amount Amount, DateRange Date, int? CategoryId, string? Observations);