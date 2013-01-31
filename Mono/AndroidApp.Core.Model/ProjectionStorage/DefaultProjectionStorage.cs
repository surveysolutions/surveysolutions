
using Android.Content;
using Newtonsoft.Json;
namespace AndroidApp.Core.Model.ProjectionStorage
{
    public class DefaultProjectionStorage : IProjectionStorage
    {
       // private readonly SQLiteContext _sqLiteContext;
        private readonly ProjectionsDataBaseHelper _databaseHelper;
        public DefaultProjectionStorage(Context context)
		{
            _databaseHelper = new ProjectionsDataBaseHelper(context);

		//	_sqLiteContext = new SQLiteContext(_databaseHelper);

			//_propertyBagConverter = new PropertyBagConverter();
		}
        private string GetJsonData(object payload)
        {
            return JsonConvert.SerializeObject(payload, Formatting.None,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
        }

        private object GetObject(string json)
        {
            return JsonConvert.DeserializeObject(json,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
        }
        #region Implementation of IProjectionStorage

        public void SaveOrUpdateProjection(IProjection projection)
        {
            var data = GetJsonData(projection.SerrializeState());

            var values = new ContentValues();
            values.Put("PublicKey", projection.PublicKey.ToString());
            values.Put("Data", data);

            _databaseHelper.WritableDatabase.Insert("Events", null, values);
        }

        public void RestoreProjection(IProjection projection)
        {
            var cursor = _databaseHelper
                .ReadableDatabase
                .RawQuery(ProjectionQuery.SelectProjectionByGuidQuery(projection.PublicKey), null);
            var dataIndex = cursor.GetColumnIndex("Data");
            while (cursor.MoveToNext())
            {
                var data = GetObject(cursor.GetString(dataIndex));
                projection.RestoreState(data);
                return;
            }
        }

        #endregion
    }
}