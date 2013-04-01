using System;

namespace CAPI.Android.Core.Model.ProjectionStorage
{
    public interface IProjectionStorage
    {
        void SaveOrUpdateProjection<T>(T projection, Guid publicKey) where  T:class ;
        T RestoreProjection<T>(Guid publicKey) where T : class;
        void ClearStorage();
        void ClearProjection(Guid prjectionKey);
    }
}