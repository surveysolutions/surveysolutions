using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public class UserMapsViewItem
    {
        public string UserName { set; get; }

        public List<string> Maps { get; set; }
    }
}