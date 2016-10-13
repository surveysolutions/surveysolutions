using System;
using Main.Core.Documents;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
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
            return
                new QuestionnaireInfoViewFactory(sharedWith ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireSharedPersons>>(),
                                                repository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(x => x.GetById(It.IsAny<string>()) == new QuestionnaireDocument()),
                                                accountsDocumentReader ?? Mock.Of<IPlainStorageAccessor<User>>(),
                                                Mock.Of<IAttachmentService>(), Mock.Of<IMembershipUserService>(x=>x.WebUser == Substitute.For<IMembershipWebUser>()));
        }
    }
}