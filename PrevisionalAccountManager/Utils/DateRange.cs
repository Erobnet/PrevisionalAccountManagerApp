namespace PrevisionalAccountManager.Utils;

public struct DateRange(DateTime start, DateTime end)
{
    public DateTime Start { get; set; } = start;
    public DateTime End { get; set; } = end;

    public bool IsSingleDay => DayCount == 1;
    public int DayCount => (End - Start).Days + 1;

    public override string ToString()
    {
        return Start == End ? Start.ToShortDateString() : $"{Start.ToShortDateString()} - {End.ToShortDateString()}";
    }
}