using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_invalid_static_text : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new Group(nestedGroupTitle)
                {
                    PublicKey = nestedGroupId,
                    Children =
                        new List<IComposite>()
                        {
                            Create.Entity.StaticText(publicKey: staticTextId, text: staticText, 
                            validationConditions: new List<ValidationCondition>()
                            {
                                Create.Entity.ValidationCondition("n1 != 1"),
                                Create.Entity.ValidationCondition("n1 != 2"),
                            })
                        }.ToReadOnlyCollection()
                });

            interview = CreateInterviewData(interviewId);
            interview.Levels["#"].StaticTexts.Add(staticTextId, new InterviewStaticText(staticTextId)
            {
                IsInvalid = true,
                FailedValidationConditions = new List<FailedValidationCondition>()
                {
                    Create.Entity.FailedValidationCondition(0),
                    Create.Entity.FailedValidationCondition(1),
                }
            });


            user = Mock.Of<UserDocument>();

            merger = CreateMerger(questionnaire);
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, user.GetUseLight(), null, null);

        It should_exists = () =>
            GetStaticText().ShouldNotBeNull();

        It should_static_text_has_text = () =>
            GetStaticText().Text.ShouldEqual(staticText);
        
        It should_static_text_be_valid = () =>
            GetStaticText().IsValid.ShouldBeFalse();
        
        It should_static_text_contains_2_invalid_messages = () =>
            GetStaticText().FailedValidationMessages.Count.ShouldEqual(2);
        
        private static InterviewGroupView GetNestedGroup()
        {
            return mergeResult.Groups.Find(g => g.Id == nestedGroupId);
        }

        private static InterviewStaticTextView GetStaticText()
        {
            return GetNestedGroup().Entities.OfType<InterviewStaticTextView>().FirstOrDefault(q => q.Id == staticTextId);
        }

        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocument questionnaire;
        private static UserDocument user;
        private static Guid nestedGroupId = Guid.Parse("11111111111111111111111111111111");
        private static Guid staticTextId = Guid.Parse("55555555555555555555555555555555");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static string nestedGroupTitle = "nested Group";
        private static string staticText = "static text";
    }
}
