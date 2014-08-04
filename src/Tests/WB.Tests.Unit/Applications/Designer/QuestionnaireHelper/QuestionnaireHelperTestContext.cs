using System;
using System.Collections.Generic;
using Main.Core.View;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireHelper
{
    internal class QuestionnaireHelperTestContext
    {
        protected static UI.Designer.Code.QuestionnaireHelper CreateQuestionnaireHelper()
        {
            var userHelperMock = new Mock<IMembershipUserService>();
            var userViewFactoryMock = new Mock<IViewFactory<QuestionnaireListInputModel, QuestionnaireListView>>();
            var user = CreateAdminUser(userHelperMock);

            userHelperMock.Setup(_ => _.WebUser).Returns(user);

            userViewFactoryMock.Setup(_ => _.Load(It.IsAny<QuestionnaireListInputModel>()))
                .Returns(new QuestionnaireListView(1,2,2,
                    new List<QuestionnaireListViewItem>
                    {
                        CreateDeletedQuestionnaire(user.UserId),
                        CreateNotDeletedQuestionnaire(user.UserId)
                    },
                    string.Empty
                    ));

            return new UI.Designer.Code.QuestionnaireHelper(userHelperMock.Object, userViewFactoryMock.Object);
        }

        protected static IMembershipWebUser CreateAdminUser(Mock<IMembershipUserService> userHelperMock)
        {
            var user = new Mock<IMembershipWebUser>();

            var userId = Guid.NewGuid();

            user.Setup(_ => _.IsAdmin).Returns(true);
            user.Setup(_ => _.UserId).Returns(userId);

            return user.Object;
        }

        protected static QuestionnaireListViewItem CreateNotDeletedQuestionnaire(Guid userGuid)
        {
            var listViewItem = new QuestionnaireListViewItem() { IsDeleted = false };
            listViewItem.SharedPersons.Add(userGuid);
            return listViewItem;
        }

        protected static QuestionnaireListViewItem CreateDeletedQuestionnaire(Guid userGuid)
        {
            var questionnaireListViewItem = new QuestionnaireListViewItem() { IsDeleted = true, };
            questionnaireListViewItem.SharedPersons.Add(userGuid);
            return questionnaireListViewItem;
        }
    }
}