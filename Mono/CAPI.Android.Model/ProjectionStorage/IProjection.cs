using System;

namespace CAPI.Android.Core.Model.ProjectionStorage
{
    public interface IProjection
    {
        Guid PublicKey { get; }
       // object SerrializeState();
        void Recicle();
      //  void RestoreState(object state);
    }
}