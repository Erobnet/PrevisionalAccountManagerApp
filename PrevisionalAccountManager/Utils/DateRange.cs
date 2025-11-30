namespace PrevisionalAccountManager.Utils;

public struct DateRange(DateTime start, DateTime end) : IEquatable<DateRange>
{
    public DateTime Start { get; set; } = start;
    public DateTime End { get; set; } = end;

    public bool IsSingleDay => DayCount == 1;
    public int DayCount => (End - Start).Days + 1;

    public override string ToString()
    {
        return Start == End ? Start.ToShortDateString() : $"{Start.ToShortDateString()} - {End.ToShortDateString()}";
    }

    public bool Equals(DateRange other)
    {
        return Start.Equals(other.Start) && End.Equals(other.End);
    }

    public override bool Equals(object? obj)
    {
        return obj is DateRange other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public static bool operator ==(DateRange left, DateRange right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DateRange left, DateRange right)
    {
        return !left.Equals(right);
    }
}