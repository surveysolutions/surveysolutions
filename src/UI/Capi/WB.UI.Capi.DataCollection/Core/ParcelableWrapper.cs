using Android.OS;
using Java.Interop;
using Newtonsoft.Json;

namespace WB.UI.Capi.DataCollection.Core
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
			this.Value = value;
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
            dest.WriteString(JsonConvert.SerializeObject(this.Value));
        }

        #endregion
    }
}