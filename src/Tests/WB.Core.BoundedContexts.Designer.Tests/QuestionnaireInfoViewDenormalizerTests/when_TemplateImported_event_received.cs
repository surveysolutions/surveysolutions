using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_TemplateImported_event_received : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            denormalizer = CreateDenormalizer();
        };

        Because of = () =>
            viewState = denormalizer.Create(
                CreatePublishableEvent(new TemplateImported()
                {
                    Source =
                        new QuestionnaireDocument()
                        {
                            PublicKey = new Guid(questionnaireId),
                            Title = questionnaireTitle,
                            Children = new List<IComposite>()
                            {
                                new Group()
                                {
                                    PublicKey = Guid.Parse(chapter1Id),
                                    Title = chapter1Title,
                                    Children = new List<IComposite>()
                                    {
                                        new Group()
                                        {
                                            Children = new List<IComposite>()
                                            {
                                                new Group()
                                                {
                                                    IsRoster = true
                                                }
                                            }
                                        }
                                    }
                                },
                                new Group()
                                {
                                    PublicKey = Guid.Parse(chapter2Id),
                                    Title = chapter2Title,
                                    Children = new List<IComposite>()
                                    {
                                        new TextQuestion()
                                        {

                                        }
                                    }
                                }
                            }
                        }
                }, new Guid(questionnaireId)));

        It should_questionnnaireInfoView_QuestionnaireId_be_equal_to_questionnaireId = () =>
            viewState.QuestionnaireId.ShouldEqual(questionnaireId);

        It should_questionnnaireInfoView_Title_be_equal_to_questionnaireTitle = () =>
            viewState.Title.ShouldEqual(questionnaireTitle);

        It should_questionnnaireInfoView_Chapters_not_be_null = () =>
            viewState.Chapters.ShouldNotBeNull();

        It should_questionnnaireInfoView_Chapters_have_2_chapters = () =>
            viewState.Chapters.Count.ShouldEqual(2);

        It should_questionnnaireInfoView_first_chapter_id_be_equal_chapter1Id = () =>
            viewState.Chapters[0].ChapterId.ShouldEqual(chapter1Id);

        It should_questionnnaireInfoView_second_chapter_id_be_equal_chapter2Id = () =>
            viewState.Chapters[1].ChapterId.ShouldEqual(chapter2Id);

        It should_questionnnaireInfoView_first_chapter_title_be_equal_chapter1Title = () =>
            viewState.Chapters[0].Title.ShouldEqual(chapter1Title);

        It should_questionnnaireInfoView_second_chapter_title_be_equal_chapter2Title = () =>
            viewState.Chapters[1].Title.ShouldEqual(chapter2Title);

        It should_questionnnaireInfoView_GroupsCount_be_equal_3 = () =>
            viewState.GroupsCount.ShouldEqual(3);

        It should_questionnnaireInfoView_QuestionsCount_be_equal_1 = () =>
            viewState.QuestionsCount.ShouldEqual(1);

        It should_questionnnaireInfoView_RostersCount_be_equal_1 = () =>
            viewState.RostersCount.ShouldEqual(1);

        private static string questionnaireId = "33333333333333333333333333333333";
        private static string questionnaireTitle = "questionnaire title";
        private static string chapter1Id = "22222222222222222222222222222222";
        private static string chapter2Id = "11111111111111111111111111111111";
        private static string chapter1Title = "chapter 1 title";
        private static string chapter2Title = "chapter 2 title";

        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
