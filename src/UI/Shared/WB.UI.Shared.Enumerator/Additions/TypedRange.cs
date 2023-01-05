using Java.Lang;
using Object = Java.Lang.Object;

namespace Com.Google.Archivepatcher.Shared
{
    public partial class TypedRange : Object, Java.Lang.IComparable
    {
        public int CompareTo(Object o)
        {
            if (o == null)
                return 1;
            if (this == o)
                return 0;
            return -1;
        }
    }
}