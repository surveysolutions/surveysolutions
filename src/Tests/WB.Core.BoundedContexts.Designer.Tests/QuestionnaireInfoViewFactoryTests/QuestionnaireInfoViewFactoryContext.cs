﻿using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewFactoryTests
{
    internal class QuestionnaireInfoViewFactoryContext
    {
        protected static QuestionnaireInfoView CreateQuestionnaireInfoView(string questionnaireId, string questionnaireTitle)
        {
            return new QuestionnaireInfoView() {QuestionnaireId = questionnaireId, Title = questionnaireTitle};
        }

        protected static QuestionnaireInfoViewFactory CreateQuestionnaireInfoViewFactory(
            IQueryableReadSideRepositoryReader<QuestionnaireInfoView> repository = null)
        {
            return
                new QuestionnaireInfoViewFactory(repository ??
                                                 Mock.Of<IQueryableReadSideRepositoryReader<QuestionnaireInfoView>>());
        }
    }
}