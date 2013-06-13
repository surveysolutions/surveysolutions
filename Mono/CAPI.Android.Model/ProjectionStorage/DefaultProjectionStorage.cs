using System;
using Android.Content;
using CAPI.Android.Core.Model.ModelUtils;
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
            var data = JsonUtils.GetJsonData(projection);
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
                return JsonUtils.GetObject<T>(cursor.GetString(dataIndex));
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