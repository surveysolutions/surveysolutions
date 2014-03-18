using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.ViewFactories
{
    internal class SurveyViewFactory : ISurveyViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<SurveyLineView> surveyLineViewRepositoryReader;
        private readonly IQueryableReadSideRepositoryReader<SurveyDetailsView> surveyDetailsViewRepositoryReader;

        public SurveyViewFactory(IQueryableReadSideRepositoryReader<SurveyLineView> surveyLineViewRepositoryReader,
            IQueryableReadSideRepositoryReader<SurveyDetailsView> surveyDetailsViewRepositoryReader)
        {
            this.surveyLineViewRepositoryReader = surveyLineViewRepositoryReader;
            this.surveyDetailsViewRepositoryReader = surveyDetailsViewRepositoryReader;
        }

        public SurveyLineView[] GetAllLineViews()
        {
            return this.surveyLineViewRepositoryReader.QueryAll(view => true).ToArray();
        }

        public SurveyDetailsView GetDetailsView(string surveyId)
        {
            return this.surveyDetailsViewRepositoryReader.GetById(surveyId);
        }
    }
}