using ddidotnet;

namespace WB.Services.Export.Ddi.Implementation
{
    internal class MetaDescriptionFactory : IMetaDescriptionFactory
    {
        public IMetadataWriter CreateMetaDescription()
        {
            return new MetadataWriter(new MetaDescription());
        }
    }
}
