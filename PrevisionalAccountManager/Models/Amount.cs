using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace PrevisionalAccountManager.Models;

public struct Amount : IEquatable<Amount>, IFormattable
{
    public double Value;

    public override bool Equals(object? obj)
    {
        return obj is Amount amount && Equals(amount);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public bool Equals(Amount other)
    {
        return Value.Equals(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString(CultureInfo.CurrentCulture);
    }

    public string ToString(IFormatProvider? format)
    {
        return Value.ToString(format);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return Value.ToString(format, formatProvider);
    }

    public static bool operator ==(Amount left, Amount right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Amount left, Amount right)
    {
        return !left.Equals(right);
    }

    public static Amount AdditiveIdentity => new Amount { Value = 0 };
    public static Amount MultiplicativeIdentity => new Amount { Value = 1 };
    public static implicit operator Amount(double value) => new() { Value = value };
    public static implicit operator double(Amount amount) => amount.Value;

    public static Amount operator +(Amount left, Amount right) => new Amount { Value = left.Value + right.Value };
    public static Amount operator -(Amount left, Amount right) => new Amount { Value = left.Value - right.Value };
    public static Amount operator *(Amount left, Amount right) => new Amount { Value = left.Value * right.Value };
    public static Amount operator /(Amount left, Amount right) => new Amount { Value = left.Value / right.Value };
    public static Amount operator ++(Amount value) => new() { Value = value.Value + 1 };
    public static Amount operator --(Amount value) => new() { Value = value.Value - 1 };
    public static Amount operator -(Amount value) => new() { Value = -value.Value };
    public static Amount operator +(Amount value) => value;
    public static Amount operator %(Amount left, Amount right) => new Amount { Value = left.Value % right.Value };
    public static bool operator <(Amount left, Amount right) => left.Value < right.Value;
    public static bool operator <=(Amount left, Amount right) => left.Value <= right.Value;
    public static bool operator >(Amount left, Amount right) => left.Value > right.Value;
    public static bool operator >=(Amount left, Amount right) => left.Value >= right.Value;

    public static Amount Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => new Amount { Value = double.Parse(s, provider) };
    public static Amount Parse(string s, IFormatProvider? provider) => new Amount { Value = double.Parse(s, provider) };

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Amount result)
    {
        if ( double.TryParse(s, provider, out double value) )
        {
            result = new Amount { Value = value };
            return true;
        }
        result = default;
        return false;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Amount result)
    {
        if ( double.TryParse(s, provider, out double value) )
        {
            result = new Amount { Value = value };
            return true;
        }
        result = default;
        return false;
    }
}