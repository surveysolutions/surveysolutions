using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.ViewModels;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModel
{
    internal class when_setting_answer_to_roster_title_question : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterTitleQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            var rosterGroup = new Group() { PublicKey = rosterGroupId, IsRoster = true, RosterSizeQuestionId = rosterSizeQuestionId, RosterTitleQuestionId = rosterTitleQuestionId };
            rosterGroup.Children.Add(new NumericQuestion()
            {
                PublicKey = rosterTitleQuestionId,
                QuestionType = QuestionType.Numeric
            });

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                rosterGroup);

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    {
                        new InterviewItemId(rosterGroupId, new decimal[0]),
                        new[] { new RosterSynchronizationDto(rosterGroupId, new decimal[0], 0, null, null) }
                    }
                });
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure, interviewSynchronizationDto);
        };

        Because of = () =>
            interviewViewModel.SetAnswer(new InterviewItemId(rosterTitleQuestionId, new decimal[] { 0 }), rosterTitle);

        It should_roster_screen_title_rosterTitle = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[new InterviewItemId(rosterGroupId, new decimal[] { 0 })]).ScreenName.ShouldEqual(rosterTitle);

        private static WB.Core.BoundedContexts.Capi.Views.InterviewDetails.InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static Guid rosterGroupId;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static string rosterTitle = "roster title";
    }
}
