using Android.OS;
using Newtonsoft.Json;

namespace CAPI.Android.Core
{
    public class CAPIParcelableCreator : Java.Lang.Object, IParcelableCreator
    {
        public CAPIParcelableCreator(/*Type type*/)
        {
           // ValueType = type;
        }
       // public Type ValueType { get; private set; }
        public Java.Lang.Object CreateFromParcel(Parcel source)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            object payload = JsonConvert.DeserializeObject(source.ReadString()/*, ValueType*/, settings);
            return new ParcelableWrapper(payload);
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return new Java.Lang.Object[size];
        }
    }
}