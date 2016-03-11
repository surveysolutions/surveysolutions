﻿using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Applications.QuestionnaireHelper
{
    internal class QuestionnaireHelperTestContext
    {
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

        protected static QuestionnaireListView CreateQuestionnaireListView(IMembershipWebUser user)
        {
            var questionnaireListView = new QuestionnaireListView(1, 2, 2,
                new List<QuestionnaireListViewItem>
                {
                    CreateDeletedQuestionnaire(user.UserId),
                    CreateNotDeletedQuestionnaire(user.UserId)
                },
                string.Empty
                );
            return questionnaireListView;
        }
    }
}