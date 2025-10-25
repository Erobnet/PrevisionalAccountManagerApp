using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Hash128 : IEquatable<Hash128>
{
    public readonly ulong High;
    public readonly ulong Low;
    
    public Hash128(ulong high, ulong low)
    {
        High = high;
        Low = low;
    }
    
    public Hash128(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 16)
            throw new ArgumentException("Need at least 16 bytes");
        
        High = BitConverter.ToUInt64(bytes.Slice(0, 8));
        Low = BitConverter.ToUInt64(bytes.Slice(8, 8));
    }
    
    public Hash128(byte[] bytes) : this(bytes.AsSpan()) { }
    
    public bool Equals(Hash128 other) => High == other.High && Low == other.Low;
    
    public override bool Equals(object? obj) => obj is Hash128 other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(High, Low);
    
    public static bool operator ==(Hash128 left, Hash128 right) => left.Equals(right);
    
    public static bool operator !=(Hash128 left, Hash128 right) => !left.Equals(right);
    
    public override string ToString() => $"{High:X16}{Low:X16}";
    
    public byte[] ToByteArray()
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(High).CopyTo(bytes, 0);
        BitConverter.GetBytes(Low).CopyTo(bytes, 8);
        return bytes;
    }
    
    public static Hash128 Zero => new(0, 0);
    
    // XOR operation for combining hashes
    public static Hash128 operator ^(Hash128 left, Hash128 right)
    {
        return new Hash128(left.High ^ right.High, left.Low ^ right.Low);
    }
}
