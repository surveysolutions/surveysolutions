using System;

namespace AndroidApp.Core.Model.ProjectionStorage
{
    public interface IProjection
    {
        Guid PublicKey { get; }
       // object SerrializeState();
        void Recicle();
      //  void RestoreState(object state);
    }
}