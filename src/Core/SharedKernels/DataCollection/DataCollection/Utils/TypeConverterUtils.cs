using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public static class TypeConverterUtils
    {
        public static TypeConverter SafeSelectTypeConverter<T>()
        {
            /*
             please don't remove check on Guid.
             * it is made because of monodroid bug
             * in monodroid in Release configuration TypeDescriptor.GetConverter(typeof (Guid)) gives MissingMethodException
             */
            if (typeof(T) == typeof(Guid))
                return new GuidConverter();

            return TypeDescriptor.GetConverter(typeof(T));
        }
    }
}
