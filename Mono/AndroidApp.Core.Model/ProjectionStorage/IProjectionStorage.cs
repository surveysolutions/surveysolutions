namespace AndroidApp.Core.Model.ProjectionStorage
{
    public interface IProjectionStorage
    {
        void SaveOrUpdateProjection(IProjection projection);
        void RestoreProjection(IProjection projection);
    }
}