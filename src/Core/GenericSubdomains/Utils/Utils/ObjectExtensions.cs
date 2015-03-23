using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class ObjectExtensions
    {
        public static string AsCompositeKey(object key, object value)
        {
            return string.Format("{0}${1}", key, value);
        }
    }
}
