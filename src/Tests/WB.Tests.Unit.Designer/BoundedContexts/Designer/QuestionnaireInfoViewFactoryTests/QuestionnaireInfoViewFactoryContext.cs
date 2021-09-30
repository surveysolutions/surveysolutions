using System;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class QuestionnaireInfoViewFactoryContext
    {
        protected static QuestionnaireDocument CreateQuestionnaireDocument(string questionnaireId, string questionnaireTitle)
        {
            return Create.QuestionnaireDocument(Guid.Parse(questionnaireId), title: questionnaireTitle);
        }

        protected static QuestionnaireInfoViewFactory CreateQuestionnaireInfoViewFactory(
            IDesignerQuestionnaireStorage repository = null,
            DesignerDbContext dbContext = null)
        {
            var doc = new QuestionnaireDocument();

            return
                new QuestionnaireInfoViewFactory(dbContext ?? Create.InMemoryDbContext(),
                    repository ?? Mock.Of<IDesignerQuestionnaireStorage>(x =>
                    x.Get(It.IsAny<QuestionnaireRevision>()) == doc),
                                                Mock.Of<IQuestionnaireCompilationVersionService>(),
                                                Mock.Of<IAttachmentService>(),
                                                Mock.Of<ILoggedInUser>());
        }

        protected static QuestionnaireRevision questionnaireId = Create.QuestionnaireRevision(Id.g1);
    }
}
