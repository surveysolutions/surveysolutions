namespace Main.Core
{
    #warning: TLK: use IRavenDBSettings in DocumentStoreProvider instead of direct parameters pass
    public interface IRavenDBSettings
    {
        bool IsStoreEmbedded { get; }

        string EmbeddedStorePath { get; }

        string ServerStorePath { get; }
    }
}