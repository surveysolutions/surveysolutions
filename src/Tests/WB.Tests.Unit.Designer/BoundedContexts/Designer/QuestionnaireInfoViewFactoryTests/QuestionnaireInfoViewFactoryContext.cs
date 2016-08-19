using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class QuestionnaireInfoViewFactoryContext
    {
        protected static QuestionnaireInfoView CreateQuestionnaireInfoView(string questionnaireId, string questionnaireTitle)
        {
            return new QuestionnaireInfoView() {QuestionnaireId = questionnaireId, Title = questionnaireTitle};
        }

        protected static QuestionnaireInfoViewFactory CreateQuestionnaireInfoViewFactory(
            IReadSideKeyValueStorage<QuestionnaireDocument> documentReader = null,
            IReadSideKeyValueStorage<QuestionnaireInfoView> repository = null,
            IReadSideKeyValueStorage<QuestionnaireSharedPersons> sharedWith = null,
            IReadSideRepositoryReader<AccountDocument> accountsDocumentReader = null)
        {
            return
                new QuestionnaireInfoViewFactory(repository ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireInfoView>>(),
                                                sharedWith ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireSharedPersons>>(),
                                                documentReader ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(x => x.GetById(It.IsAny<string>()) == new QuestionnaireDocument()),
                                                accountsDocumentReader ?? Mock.Of<IReadSideRepositoryReader<AccountDocument>>(),
                                                Mock.Of<IAttachmentService>(), Mock.Of<IMembershipUserService>());
        }
    }
}