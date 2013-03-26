using Android.OS;
using Java.Interop;
using Newtonsoft.Json;

namespace CAPI.Android.Core
{
    public class ParcelableWrapper : Java.Lang.Object, IParcelable
    {
        [ExportField("CREATOR")]
        static CAPIParcelableCreator InitializeCreator()
        {
            return new CAPIParcelableCreator(/*typeof(T)*/);
        }

        public ParcelableWrapper(object value)
		{
			Value = value;
		}

        public object Value { get; private set; }
        #region Implementation of IParcelable

        public int DescribeContents()
        {
            return 0;
        }

        public void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
          /*  var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            T payload = JsonConvert.DeserializeObject<T>(s, settings);*/
            dest.WriteString(JsonConvert.SerializeObject(Value));
        }

        #endregion
    }
}