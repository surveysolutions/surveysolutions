using ddidotnet;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Ddi.Impl
{
    internal class MetaDescriptionFactory : IMetaDescriptionFactory
    {
        public IMetadataWriter CreateMetaDescription()
        {
            return new MetadataWriter(new MetaDescription());
        }
    }
}