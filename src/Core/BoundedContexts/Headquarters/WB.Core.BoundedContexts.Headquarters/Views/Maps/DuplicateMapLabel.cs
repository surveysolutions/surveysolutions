namespace WB.Core.BoundedContexts.Headquarters.Views.Maps;

public class DuplicateMapLabel
{
    public virtual int Id { get; set; }
    public virtual string Label { get; set; }
    public virtual int Count { get; set; }
    public virtual MapBrowseItem Map { get; set; }
        
    protected bool Equals(DuplicateMapLabel other) => Id == other.Id;
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DuplicateMapLabel) obj);
    }

    public override int GetHashCode() => Id;

}