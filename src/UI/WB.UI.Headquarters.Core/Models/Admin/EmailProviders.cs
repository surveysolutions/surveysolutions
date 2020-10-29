using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WB.UI.Headquarters.Models.Admin
{
    public class EmailProviders
    {
        public dynamic Api { get; set; }
        public List<KeyValuePair<string, string>> AwsRegions { get; set; }
    }
}
