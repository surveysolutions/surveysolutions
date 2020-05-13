namespace WB.Core.BoundedContexts.Headquarters.Views.Maps
{
    public class UserMap
    {
        public virtual int Id { get; set; }
        public virtual string Map { get; set; }
        public virtual string UserName { get; set; }
        public virtual MapBrowseItem Source { get; set; }
    }
}
