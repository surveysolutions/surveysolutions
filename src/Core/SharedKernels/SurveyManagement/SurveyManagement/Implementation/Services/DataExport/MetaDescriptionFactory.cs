using ddidotnet;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    public class MetaDescriptionFactory : IMetaDescriptionFactory
    {
        public IMetaDescription CreateMetaDescription()
        {
            return new MetaDescription();

        }
    }
}