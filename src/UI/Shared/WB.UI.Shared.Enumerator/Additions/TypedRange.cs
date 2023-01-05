using Java.Lang;
using Object = Java.Lang.Object;

namespace Com.Google.Archivepatcher.Shared
{
    public partial class TypedRange : Object, Java.Lang.IComparable
    {
        /*
        int IComparable.CompareTo(object obj)
        {
            return 0;
        }
        */

        public int CompareTo(Object o)
        {
            throw new System.NotImplementedException();
        }
    }
}