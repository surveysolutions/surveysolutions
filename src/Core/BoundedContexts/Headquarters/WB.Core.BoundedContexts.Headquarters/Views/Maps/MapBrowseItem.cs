using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Maps
{
    public class MapBrowseItem
    {
        public virtual string Id { get; set; }

        public virtual long Size { get; set; }
        public virtual string FileName { get; set; }
        public virtual DateTime? ImportDate { get; set; }
    }
}
