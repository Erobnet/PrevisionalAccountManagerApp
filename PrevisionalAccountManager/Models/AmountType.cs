using NetEscapades.EnumGenerators;

namespace PrevisionalAccountManager.Models;

[EnumExtensions]
public enum AmountType { Credit = 1, Debit = -1 }