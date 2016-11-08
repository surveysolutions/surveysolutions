using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Shared.Web.Membership;
using It = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    [Subject(typeof(QuestionnaireInfoViewFactory))]
    internal class QuestionnaireInfoViewFactoryContext
    {
        protected static QuestionnaireDocument CreateQuestionnaireDocument(string questionnaireId, string questionnaireTitle)
        {
            return Create.QuestionnaireDocument(Guid.Parse(questionnaireId), title : questionnaireTitle);
        }

        protected static QuestionnaireInfoViewFactory CreateQuestionnaireInfoViewFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> repository = null,
            IPlainKeyValueStorage<QuestionnaireSharedPersons> sharedWith = null,
            IPlainStorageAccessor<User> accountsDocumentReader = null)
        {
            var doc = new QuestionnaireDocument();
            return
                new QuestionnaireInfoViewFactory(sharedWith ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireSharedPersons>>(),
                                                repository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(x => x.GetById(It.IsAny<string>()) == doc),
                                                accountsDocumentReader ?? Mock.Of<IPlainStorageAccessor<User>>(),
                                                Mock.Of<IAttachmentService>(), Mock.Of<IMembershipUserService>(x=>x.WebUser == Substitute.For<IMembershipWebUser>()));
        }
    }
}