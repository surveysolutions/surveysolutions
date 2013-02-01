
using System;
using System.Collections.Generic;
using Android.Content;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
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
            var data = JsonConvert.SerializeObject(payload, Formatting.None,
                                                   new JsonSerializerSettings
                                                       {
                                                           TypeNameHandling = TypeNameHandling.Objects,ContractResolver = new CriteriaContractResolver()/*,
                                                           Converters =
                                                               new List<JsonConverter>() {new ItemPublicKeyConverter()}*/
                                                       });
            Console.WriteLine(data);
            return data;
        }

        private object GetObject(string json)
        {
            return JsonConvert.DeserializeObject(json,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    ContractResolver = new CriteriaContractResolver()/*,
                    Converters =
                        new List<JsonConverter>() { new ItemPublicKeyConverter() }*/
                });
        }
        #region Implementation of IProjectionStorage

        public void SaveOrUpdateProjection(object projection, Guid publicKey)
        {
            var data = GetJsonData(projection);
            var values = new ContentValues();
            values.Put("PublicKey", publicKey.ToString());
            values.Put("Data", data);

            _databaseHelper.WritableDatabase.Insert("Projections", null, values);
        }

        public object RestoreProjection(Guid publicKey)
        {
            var cursor = _databaseHelper
                .ReadableDatabase
                .RawQuery(ProjectionQuery.SelectProjectionByGuidQuery(publicKey), null);
            var dataIndex = cursor.GetColumnIndex("Data");
            while (cursor.MoveToNext())
            {
                return GetObject(cursor.GetString(dataIndex));
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