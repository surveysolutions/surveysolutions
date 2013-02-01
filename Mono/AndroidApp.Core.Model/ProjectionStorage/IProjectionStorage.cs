using System;

namespace AndroidApp.Core.Model.ProjectionStorage
{
    public interface IProjectionStorage
    {
        void SaveOrUpdateProjection(object projection, Guid publicKey);
        object RestoreProjection(Guid publicKey);
        void ClearStorage();
        void ClearProjection(Guid prjectionKey);
    }
}