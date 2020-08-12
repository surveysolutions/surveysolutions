namespace WB.Core.BoundedContexts.Headquarters.Views.Maps
{
    public class UserMap
    {
        public virtual int Id { get; set; }
        public virtual string UserName { get; set; }
        public virtual MapBrowseItem Map { get; set; }
        
        protected bool Equals(UserMap other) => Id == other.Id;
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserMap) obj);
        }

        public override int GetHashCode() => Id;
    }
}
