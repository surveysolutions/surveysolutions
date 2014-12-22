using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Raven.Client.Embedded;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Tests.Integration.SurveyManagement.RavenIndexes.SupervisorReportsSurveysAndStatusesGroupByTeamMemberTests
{
    [Subject(typeof(QuestionnaireBrowseItemsGroupByQuestionnaireIdIndex))]
    internal class when_applying_index_to_5_misc_questionnaire_browse_items : RavenIndexesTestContext
    {
        Establish context = () =>
        {
            documentStore = CreateDocumentStore(questionnaireBrowseItems: new List<QuestionnaireBrowseItem>
            {
                new QuestionnaireBrowseItem{ QuestionnaireId = questionnaireAId, Version = 1, Title = "Another title"},
                new QuestionnaireBrowseItem{ QuestionnaireId = questionnaireAId, Version = 2, Title = "Another title"},
                new QuestionnaireBrowseItem{ QuestionnaireId = questionnaireAId, Version = 4, Title = questionnaireATitle},
                new QuestionnaireBrowseItem{ QuestionnaireId = questionnaireBId, Version = 3, Title = questionnaireBTitle}
            });
        };

        Because of = () =>
            resultItems = QueryUsingIndex<QuestionnaireAndVersionsItem>(documentStore, typeof(QuestionnaireBrowseItemsGroupByQuestionnaireIdIndex));

        It should_return_2_lines_of_items = () =>
            resultItems.Length.ShouldEqual(2);

        It should_contains_only_questionnaireAId_and_questionnaireBId_ids = () =>
            resultItems.Select(x => x.QuestionnaireId).ShouldContainOnly(new[] { questionnaireAId, questionnaireBId });

        It should_set_questionnaireATitle_in_record_with_id_equals_questionnaireAId = () =>
            resultItems.Single(x => x.QuestionnaireId == questionnaireAId).Title.ShouldEqual(questionnaireATitle);

        It should_set_1_2_and_4_as_versions_for_record_with_id_equals_questionnaireAId = () =>
            resultItems.Single(x => x.QuestionnaireId == questionnaireAId).Versions.ShouldContainOnly(new long[] { 1, 2, 4 });

        It should_set_questionnaireBTitle_in_record_with_id_equals_questionnaireBId = () =>
           resultItems.Single(x => x.QuestionnaireId == questionnaireBId).Title.ShouldEqual(questionnaireBTitle);

        It should_set_3_as_versions_for_record_with_id_equals_questionnaireBId = () =>
            resultItems.Single(x => x.QuestionnaireId == questionnaireBId).Versions.ShouldContainOnly(new long[] { 3 });

        Cleanup stuff = () =>
        {
            documentStore.Dispose();
            documentStore = null;
        };

        private static Guid questionnaireAId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionnaireBId = Guid.Parse("22222222222222222222222222222222");
        private static string questionnaireATitle = "Questionnaire A Title";
        private static string questionnaireBTitle = "Questionnaire B Title";
        private static EmbeddableDocumentStore documentStore;
        private static QuestionnaireAndVersionsItem[] resultItems;
    }
}