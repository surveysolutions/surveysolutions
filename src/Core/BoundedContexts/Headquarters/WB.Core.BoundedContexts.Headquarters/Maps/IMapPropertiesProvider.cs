namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public interface IMapPropertiesProvider
    {
        MapProperties GetMapPropertiesFromFile(string pathToFile);
    }
}