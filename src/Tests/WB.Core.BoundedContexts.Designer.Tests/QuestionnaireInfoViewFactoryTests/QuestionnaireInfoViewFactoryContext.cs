using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
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
            IReadSideRepositoryReader<QuestionnaireDocument> documentReader = null, 
            IQueryableReadSideRepositoryReader<QuestionnaireInfoView> repository = null, 
            IReadSideRepositoryReader<QuestionnaireSharedPersons> sharedWith = null,
            IReadSideRepositoryReader<AccountDocument> accountsDocumentReader = null)
        {
            return
                new QuestionnaireInfoViewFactory(repository ?? Mock.Of<IQueryableReadSideRepositoryReader<QuestionnaireInfoView>>(),
                                                sharedWith ?? Mock.Of<IReadSideRepositoryReader<QuestionnaireSharedPersons>>(),
                                                documentReader ?? Mock.Of<IReadSideRepositoryReader<QuestionnaireDocument>>(x => x.GetById(It.IsAny<string>()) == new QuestionnaireDocument()),
                                                accountsDocumentReader ?? Mock.Of<IReadSideRepositoryReader<AccountDocument>>());
        }
    }
}