using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Maps
{
    public class MapBrowseItem
    {
        public MapBrowseItem()
        {
            this.Users = new HashSet<UserMap>();
        }
        public virtual string Id { get; set; }

        public virtual long Size { get; set; }
        public virtual string FileName { get; set; }
        public virtual DateTime? ImportDate { get; set; }
        public virtual Guid? UploadedBy { get; set; }
        public virtual double XMaxVal { set; get; }
        public virtual double YMaxVal { set; get; }
        public virtual double XMinVal { set; get; }
        public virtual double YMinVal { set; get; }
        public virtual int Wkid { set; get; }

        public virtual double MaxScale { set; get; }
        public virtual double MinScale { set; get; }
        public virtual ICollection<UserMap> Users { get; set; }
        public virtual string ShapeType { get; set; }
        public virtual int? ShapesCount { get; set; }
    }
}
