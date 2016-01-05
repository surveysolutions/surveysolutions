using ddidotnet;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal class MetaDescriptionFactory : IMetaDescriptionFactory
    {
        public IMetaDescription CreateMetaDescription()
        {
            return new MetaDescription();
        }
    }
}