using System;
using Android.Content;
using Newtonsoft.Json;

namespace CAPI.Android.Core.Model.ProjectionStorage
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
            var data = JsonConvert.SerializeObject(payload, Formatting.None,
                                                   new JsonSerializerSettings
                                                       {
                                                           TypeNameHandling = TypeNameHandling.Objects/*,
                                                           Converters =
                                                               new List<JsonConverter>() {new ItemPublicKeyConverter()}*/
                                                       });
            Console.WriteLine(data);
            return data;
        }

        private T GetObject<T>(string json)where  T:class 
        {
            return JsonConvert.DeserializeObject<T>(json,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects/*,
                    Converters =
                        new List<JsonConverter>() { new ItemPublicKeyConverter() }*/
                });
        }
        #region Implementation of IProjectionStorage

        public void SaveOrUpdateProjection<T>(T projection, Guid publicKey)where  T:class 
        {
            var cursor = _databaseHelper
               .ReadableDatabase
               .RawQuery(ProjectionQuery.SelectProjectionExistanceByGuidQuery(publicKey), null);
            var dataIndex = cursor.GetColumnIndex("Count");
            int count=0;
            while (cursor.MoveToNext())
            {
                if (!int.TryParse(cursor.GetString(dataIndex), out count))
                    count = 0;
                break;
                
            }
            var data = GetJsonData(projection);
            var values = new ContentValues();
            values.Put("PublicKey", publicKey.ToString());
            values.Put("Data", data);
            if (count == 0)
                _databaseHelper.WritableDatabase.Insert("Projections", null, values);
            else
                _databaseHelper.WritableDatabase.Update("Projections", values, string.Format("[PublicKey] = '{0}'", publicKey),
                                                        new string[0]);
        }

        public T RestoreProjection<T>(Guid publicKey)where  T:class 
        {
            var cursor = _databaseHelper
                .ReadableDatabase
                .RawQuery(ProjectionQuery.SelectProjectionByGuidQuery(publicKey), null);
            var dataIndex = cursor.GetColumnIndex("Data");
            while (cursor.MoveToNext())
            {
                return GetObject<T>(cursor.GetString(dataIndex));
            }
            return null;
        }

        public void ClearStorage()
        {
            _databaseHelper.WritableDatabase.Delete("Projections", null, null);
        }

        public void ClearProjection(Guid prjectionKey)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}