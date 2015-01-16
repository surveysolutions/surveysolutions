extern alias designer;

using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using It = Machine.Specifications.It;

using TemplateImported = designer::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionsAndGroupsCollectionDenormalizerTests
{
    internal class when_handling_TemplateImported_event_for_old_templates : QuestionsAndGroupsCollectionDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionDetailsFactoryMock = new Mock<IQuestionDetailsViewMapper>();
            questionDetailsFactoryMock
                .Setup(x => x.Map(Moq.It.IsAny<IQuestion>(), Moq.It.IsAny<Guid>()))
                .Returns((IQuestion q, Guid p) => new TextDetailsView
                {
                    Id = q.PublicKey,
                    ParentGroupId = p
                });
            questionFactoryMock = new Mock<IQuestionnaireEntityFactory>();

            questionnaire = new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("G1")
                    {
                        PublicKey = g1Id,
                        Children = new List<IComposite>
                        {
                            new TextQuestion{ PublicKey = q1Id, QuestionType = QuestionType.Text},
                            new MultyOptionsQuestion{ PublicKey = q2Id, QuestionType = QuestionType.MultyOption},
                            new Group("R1.1")
                            {
                                PublicKey = g2Id,
                                IsRoster = true,
                                RosterSizeSource = RosterSizeSourceType.Question,
                                RosterSizeQuestionId = q2Id,
                                RosterTitleQuestionId = q3Id,
                                Children = new List<IComposite>
                                {
                                    new TextQuestion{ PublicKey = q3Id, QuestionType = QuestionType.Text},
                                    new Group("R1.1.1")
                                    {
                                        PublicKey = g3Id,
                                        IsRoster = true,
                                        RosterSizeSource = RosterSizeSourceType.FixedTitles,
                                        RosterFixedTitles = new[] { "1", "2", "3" },
                                        Children = new List<IComposite>
                                        {
                                            new TextQuestion{ PublicKey = q4Id, QuestionType = QuestionType.Text},
                                        }
                                    },
                                    new Group("G1.1.2")
                                    {
                                        PublicKey = g4Id,
                                        Children = new List<IComposite>
                                        {
                                            new StaticText(publicKey: st2Id, text: st2Text),
                                            new TextQuestion{ PublicKey = q5Id, QuestionType = QuestionType.Text},
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Group("G2")
                    {
                        PublicKey = g5Id,
                        Children = new List<IComposite>
                        {
                            new StaticText(publicKey: st1Id, text: st1Text),
                            new TextQuestion{ PublicKey = q6Id, QuestionType = QuestionType.Text},
                        }
                    },
                }
            };

            evnt = CreateTemplateImportedEvent(questionnaire);

            denormalizer = CreateQuestionnaireInfoDenormalizer(
                questionDetailsViewMapper: questionDetailsFactoryMock.Object,
                questionnaireEntityFactory: questionFactoryMock.Object);
        };

        Because of = () =>
            newState = denormalizer.Update(null, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_questions_collection_in_result_view = () =>
            newState.Questions.ShouldNotBeNull();

        It should_return_6_items_in_questions_collection = () =>
            newState.Questions.Count.ShouldEqual(6);

        It should_return_items_in_questions_collection_with_specified_ids = () =>
            newState.Questions.Select(x => x.Id).ShouldContainOnly(q1Id, q2Id, q3Id, q4Id, q5Id, q6Id);

        It should_return_1st_group_with_empty_breadcrumbs = () =>
            newState.Groups.Single(x => x.Id == g1Id).ParentGroupsIds.ShouldBeEmpty();

        It should_return_1st_group_with_empty_roster_scope = () =>
            newState.Groups.Single(x => x.Id == g1Id).RosterScopeIds.ShouldBeEmpty();

        It should_return_2nd_group_with_breadcrumbs_with_g1Id = () =>
            newState.Groups.Single(x => x.Id == g2Id).ParentGroupsIds.ShouldContainOnly(g1Id);

        It should_return_2nd_group_with_roster_scope_ids_with_q2Id = () =>
            newState.Groups.Single(x => x.Id == g2Id).RosterScopeIds.ShouldContainOnly(q2Id);

        It should_return_3rd_group_with_breadcrumbs_with_g1Id_g2Id = () =>
            newState.Groups.Single(x => x.Id == g3Id).ParentGroupsIds.ShouldContainOnly(g1Id, g2Id);

        It should_return_3rd_group_with_roster_scope_ids_with_q2Id_g3Id = () =>
            newState.Groups.Single(x => x.Id == g3Id).RosterScopeIds.ShouldContainOnly(q2Id, g3Id);

        It should_return_4th_group_with_breadcrumbs_with_g1Id_g2Id = () =>
            newState.Groups.Single(x => x.Id == g4Id).ParentGroupsIds.ShouldContainOnly(g1Id, g2Id);

        It should_return_4th_group_with_roster_scope_ids_with_q2Id = () =>
            newState.Groups.Single(x => x.Id == g4Id).RosterScopeIds.ShouldContainOnly(q2Id);

        It should_return_5th_group_with_empty_breadcrumbs = () =>
            newState.Groups.Single(x => x.Id == g5Id).ParentGroupsIds.ShouldBeEmpty();

        It should_return_5th_group_with_emptyh_roster_scope_ids_ = () =>
            newState.Groups.Single(x => x.Id == g5Id).RosterScopeIds.ShouldBeEmpty();

        It should_return_1st_group_with_parent_id_equals_empty_guid = () =>
            newState.Groups.Single(x => x.Id == g1Id).ParentGroupId.ShouldEqual(Guid.Empty);

        It should_return_2nd_group_with_parent_id_equals_g1Id = () =>
            newState.Groups.Single(x => x.Id == g2Id).ParentGroupId.ShouldEqual(g1Id);

        It should_return_3rd_group_with_parent_id_equals_g2Id = () =>
            newState.Groups.Single(x => x.Id == g3Id).ParentGroupId.ShouldEqual(g2Id);

        It should_return_4th_group_with_parent_id_equals_g2Id = () =>
            newState.Groups.Single(x => x.Id == g4Id).ParentGroupId.ShouldEqual(g2Id);

        It should_return_5th_group_with_parent_id_equals_empty_guid = () =>
            newState.Groups.Single(x => x.Id == g5Id).ParentGroupId.ShouldEqual(Guid.Empty);

        It should_return_not_null_groups_collection_in_result_sview = () =>
            newState.Groups.ShouldNotBeNull();

        It should_return_5_items_in_groups_collection = () =>
            newState.Groups.Count.ShouldEqual(5);

        It should_return_items_in_groups_collection_with_specified_ids = () =>
            newState.Groups.Select(x => x.Id).ShouldContainOnly(g1Id, g2Id, g3Id, g4Id, g5Id);

        It should_return_question_N1_with_parent_id_equals_g1Id = () =>
            GetQuestion(q1Id).ParentGroupId.ShouldEqual(g1Id);

        It should_return_question_N2_with_parent_id_equals_g1Id = () =>
            GetQuestion(q2Id).ParentGroupId.ShouldEqual(g1Id);

        It should_return_question_N3_with_parent_id_equals_g2Id = () =>
            GetQuestion(q3Id).ParentGroupId.ShouldEqual(g2Id);

        It should_return_question_N4_with_parent_id_equals_g3Id = () =>
            GetQuestion(q4Id).ParentGroupId.ShouldEqual(g3Id);

        It should_return_question_N5_with_parent_id_equals_g4Id = () =>
            GetQuestion(q5Id).ParentGroupId.ShouldEqual(g4Id);

        It should_return_question_N6_with_parent_id_equals_g5Id = () =>
            GetQuestion(q6Id).ParentGroupId.ShouldEqual(g5Id);

        It should_return_question_N1_with_breadcrumbs_with_g1Id = () =>
            GetQuestion(q1Id).ParentGroupsIds.ShouldContainOnly(g1Id);

        It should_return_question_N2_with_breadcrumbs_with_g1Id = () =>
            GetQuestion(q2Id).ParentGroupsIds.ShouldContainOnly(g1Id);

        It should_return_question_N3_with_breadcrumbs_with_g1Id_g2Id = () =>
            GetQuestion(q3Id).ParentGroupsIds.ShouldContainOnly(g1Id, g2Id);

        It should_return_question_N4_with_breadcrumbs_with_g1Id_g2Id_g3Id = () =>
            GetQuestion(q4Id).ParentGroupsIds.ShouldContainOnly(g1Id, g2Id, g3Id);

        It should_return_question_N5_with_breadcrumbs_with_g1Id_g2Id_g4Id = () =>
            GetQuestion(q5Id).ParentGroupsIds.ShouldContainOnly(g1Id, g2Id, g4Id);

        It should_return_question_N6_with_breadcrumbs_with_g1Id = () =>
            GetQuestion(q6Id).ParentGroupsIds.ShouldContainOnly(g5Id);

        It should_return_question_N1_with_empty_roster_scope_ids_ = () =>
            GetQuestion(q1Id).RosterScopeIds.ShouldBeEmpty();

        It should_return_question_N2_with_empty_roster_scope_ids = () =>
            GetQuestion(q2Id).RosterScopeIds.ShouldBeEmpty();

        It should_return_question_N3_with_roster_scope_ids_with_q2Id = () =>
            GetQuestion(q3Id).RosterScopeIds.ShouldContainOnly(q2Id);

        It should_return_question_N4_with_roster_scope_ids_with_q2Id_g3Id = () =>
            GetQuestion(q4Id).RosterScopeIds.ShouldContainOnly(q2Id, g3Id);

        It should_return_question_N5_with_roster_scope_ids_with_q2Id = () =>
            GetQuestion(q5Id).RosterScopeIds.ShouldContainOnly(q2Id);

        It should_return_question_N6_with_empty_roster_scope_ids = () =>
            GetQuestion(q6Id).RosterScopeIds.ShouldBeEmpty();

        It should_return_not_null_static_text_collection_in_result_view = () =>
            newState.StaticTexts.ShouldNotBeNull();

        It should_return_2_items_in_static_text_collection = () =>
           newState.StaticTexts.Count.ShouldEqual(2);

        It should_return_items_in_static_text_collection_with_specified_ids = () =>
            newState.StaticTexts.Select(x => x.Id).ShouldContainOnly(st1Id, st2Id);

        It should_return_static_text_N1_with_parent_id_equals_g5Id = () =>
            GetStaticText(st1Id).ParentGroupId.ShouldEqual(g5Id);

        It should_return_static_text_N1_with_breadcrumbs_with_g5Id = () =>
            GetStaticText(st1Id).ParentGroupsIds.ShouldContainOnly(g5Id);

        It should_return_static_text_N1_with_empty_roster_scope_ids = () =>
           GetStaticText(st1Id).RosterScopeIds.ShouldBeEmpty();

        It should_return_static_text_N1_with_specified_text = () =>
          GetStaticText(st1Id).Text.ShouldEqual(st1Text);

        It should_return_static_text_N2_with_parent_id_equals_g4Id = () =>
            GetStaticText(st2Id).ParentGroupId.ShouldEqual(g4Id);

        It should_return_static_text_N2_with_breadcrumbs_with_g1Id_g2Id_g4Id = () =>
            GetStaticText(st2Id).ParentGroupsIds.ShouldContainOnly(g1Id, g2Id, g4Id);

        It should_return_static_text_N2_with_roster_scope_ids_with_q2Id = () =>
            GetStaticText(st2Id).RosterScopeIds.ShouldContainOnly(q2Id);

        It should_return_static_text_N2_with_specified_text = () =>
          GetStaticText(st2Id).Text.ShouldEqual(st2Text);

        private static QuestionDetailsView GetQuestion(Guid questionId)
        {
            return newState.Questions.Single(x => x.Id == questionId);
        }

        private static StaticTextDetailsView GetStaticText(Guid entityId)
        {
            return newState.StaticTexts.Single(x => x.Id == entityId);
        }

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<TemplateImported> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionDetailsViewMapper> questionDetailsFactoryMock = null;
        private static Mock<IQuestionnaireEntityFactory> questionFactoryMock;
        private static QuestionnaireDocument questionnaire;

        private static Guid g1Id = Guid.Parse("11111111111111111111111111111111");
        private static Guid g2Id = Guid.Parse("22222222222222222222222222222222");
        private static Guid g3Id = Guid.Parse("33333333333333333333333333333333");
        private static Guid g4Id = Guid.Parse("44444444444444444444444444444444");
        private static Guid g5Id = Guid.Parse("55555555555555555555555555555555");

        private static Guid q1Id = Guid.Parse("66666666666666666666666666666666");
        private static Guid q2Id = Guid.Parse("77777777777777777777777777777777");
        private static Guid q3Id = Guid.Parse("88888888888888888888888888888888");
        private static Guid q4Id = Guid.Parse("99999999999999999999999999999999");
        private static Guid q5Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid q6Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        private static Guid st1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid st2Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

        private static string st1Text = "static text 1";
        private static string st2Text = "static text 2";
    }
}