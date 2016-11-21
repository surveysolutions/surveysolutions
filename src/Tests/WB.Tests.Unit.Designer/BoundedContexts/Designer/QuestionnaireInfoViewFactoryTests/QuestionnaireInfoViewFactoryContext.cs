using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.Implementation;
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
            IPlainStorageAccessor<User> accountsDocumentReader = null,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage = null)
        {
            var doc = new QuestionnaireDocument();
            var mockedListStorage = new Mock<IPlainStorageAccessor<QuestionnaireListViewItem>>();
            mockedListStorage.SetReturnsDefault(Create.QuestionnaireListViewItem());

            return
                new QuestionnaireInfoViewFactory(repository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(x => x.GetById(It.IsAny<string>()) == doc),
                                                questionnaireListViewItemStorage ?? mockedListStorage.Object, 
                                                accountsDocumentReader ?? Mock.Of<IPlainStorageAccessor<User>>(),
                                                Mock.Of<IAttachmentService>(), Mock.Of<IMembershipUserService>(x=>x.WebUser == Substitute.For<IMembershipWebUser>()));
        }
    }
}