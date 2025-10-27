namespace PrevisionalAccountManager.Models.DataBaseEntities;

public class TypesStableHashInfoModel : IEquatable<TypesStableHashInfoModel>
{
    public int Id { get; init; }
    public string TypesStableHash = "";
    public bool Equals(TypesStableHashInfoModel? other)
    {
        if ( other is null )
        {
            return false;
        }
        if ( ReferenceEquals(this, other) )
        {
            return true;
        }
        return Equals(TypesStableHash, other.TypesStableHash);
    }

    public override bool Equals(object? obj)
    {
        if ( obj is null )
        {
            return false;
        }
        if ( ReferenceEquals(this, obj) )
        {
            return true;
        }
        if ( obj.GetType() != GetType() )
        {
            return false;
        }
        return Equals((TypesStableHashInfoModel)obj);
    }

    public override int GetHashCode()
    {
        return TypesStableHash.GetHashCode();
    }

    public static bool operator ==(TypesStableHashInfoModel? left, TypesStableHashInfoModel? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TypesStableHashInfoModel? left, TypesStableHashInfoModel? right)
    {
        return !Equals(left, right);
    }
}