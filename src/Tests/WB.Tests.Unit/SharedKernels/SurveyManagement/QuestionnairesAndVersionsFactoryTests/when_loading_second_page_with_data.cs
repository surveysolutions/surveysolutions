using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnairesAndVersionsFactoryTests
{
    internal class when_loading_second_page_with_data : QuestionnairesAndVersionsFactoryTestContext
    {
        Establish context = () =>
        {
            indexAccessorMock = new Mock<IReadSideRepositoryIndexAccessor>();

            indexAccessorMock
                .Setup(x => x.Query<QuestionnaireAndVersionsItem>(typeof (QuestionnaireBrowseItemsGroupByQuestionnaireIdIndex).Name))
                .Returns(new List<QuestionnaireAndVersionsItem>
                {
                    new QuestionnaireAndVersionsItem { QuestionnaireId = questionnaireAId, Title = "title A", Versions = new long[]{ 1 }},
                    new QuestionnaireAndVersionsItem { QuestionnaireId = questionnaireBId, Title = "title B", Versions = new long[]{ 1, 3, 4 }},
                    new QuestionnaireAndVersionsItem { QuestionnaireId = questionnaireCId, Title = "title C", Versions = new long[]{ 48 }},
                    new QuestionnaireAndVersionsItem { QuestionnaireId = questionnaireDId, Title = "title D", Versions = new long[]{ 2, 3, 8 }},
                    new QuestionnaireAndVersionsItem { QuestionnaireId = questionnaireEId, Title = "title E", Versions = new long[]{ 1 }}
                }.AsQueryable());

            factory = CreateQuestionnairesAndVersionsFactory(indexAccessorMock.Object);

            input = CreateQuestionnairesAndVersionsInputModel(page: 2, pageSize: 2);
        };

        Because of = () => view = factory.Load(input);

        It should_returns_2_records_in_Items_field = () => 
            view.Items.Count().ShouldEqual(2);

        It should_contains_questionnaires_C_and_D_only = () =>
           view.Items.Select(x=> x.QuestionnaireId).ShouldContainOnly(new []{ questionnaireCId, questionnaireDId});

        It should_contains_questionnaire_C_with_Tille__title_C__ = () =>
          view.Items.Single(x => x.QuestionnaireId == questionnaireCId).Title.ShouldEqual("title C");

        It should_contains_questionnaire_C_with_Version__48__ = () =>
          view.Items.Single(x => x.QuestionnaireId == questionnaireCId).Versions.ShouldContainOnly(new long[]{ 48 });

        It should_contains_questionnaire_D_with_Version__2_3_8___ = () =>
            view.Items.Single(x => x.QuestionnaireId == questionnaireDId).Versions.ShouldContainOnly(new long[] { 2, 3, 8 });

        It should_contains_questionnaire_D_with_Title__title_D___ = () =>
         view.Items.Single(x => x.QuestionnaireId == questionnaireDId).Title.ShouldEqual("title D");

        It should_returns_5_TotalCount_field = () =>
           view.TotalCount.ShouldEqual(5);

        private static QuestionnairesAndVersionsFactory factory;
        private static QuestionnaireBrowseInputModel input;
        private static QuestionnaireAndVersionsView view;
        private static Guid questionnaireAId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionnaireBId = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionnaireCId = Guid.Parse("33333333333333333333333333333333");
        private static Guid questionnaireDId = Guid.Parse("44444444444444444444444444444444");
        private static Guid questionnaireEId = Guid.Parse("55555555555555555555555555555555");
        private static Mock<IReadSideRepositoryIndexAccessor> indexAccessorMock;
    }
}
