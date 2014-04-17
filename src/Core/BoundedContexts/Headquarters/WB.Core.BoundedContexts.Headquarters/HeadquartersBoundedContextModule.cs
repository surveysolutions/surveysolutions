using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation;
using WB.Core.Infrastructure.FunctionalDenormalization;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IVersionedQuestionnaireReader>().To<VersionedQustionnaireDocumentViewFactory>();
            this.Kernel.RegisterDenormalizer<InterviewsFeedDenormalizer>();
        }
    }
}
