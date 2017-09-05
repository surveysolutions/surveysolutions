using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewSynchronizationDtoFactoryTests
{
    internal class when_build_from_interview_with_static_texts : InterviewSynchronizationDtoFactoryTestContext
    {
        Establish context = () =>
        {
            interviewData = CreateInterviewData();
            interviewData.Levels.Add("#1", new InterviewLevel
            {
                RosterVector = RosterVector.Empty,
                StaticTexts = new Dictionary<Guid, InterviewStaticText>()
                {
                    {disabledStaticText1Id.Id, new InterviewStaticText {Id = disabledStaticText1Id.Id, IsEnabled = false, IsInvalid = false}},
                    {validStaticText1Id.Id, new InterviewStaticText {Id = validStaticText1Id.Id, IsInvalid = false, IsEnabled = true}},
                    {
                        invalidStaticText1Id.Id,
                        new InterviewStaticText
                        {
                            Id = invalidStaticText1Id.Id,
                            IsInvalid = true,
                            IsEnabled = true,
                            FailedValidationConditions = new List<FailedValidationCondition>()
                        }
                    }
                }
            });
            interviewData.Levels.Add("#2", new InterviewLevel
            {
                RosterVector = new RosterVector(new decimal[] {1, 2, 3}),
                StaticTexts = new Dictionary<Guid, InterviewStaticText>()
                {
                    {disabledStaticText2Id.Id, new InterviewStaticText {Id = disabledStaticText2Id.Id, IsEnabled = false, IsInvalid = false}},
                    {validStaticText2Id.Id, new InterviewStaticText {Id = validStaticText2Id.Id, IsEnabled = true, IsInvalid = false}},
                    {
                        invalidStaticText2Id.Id,
                        new InterviewStaticText
                        {
                            Id = invalidStaticText2Id.Id,
                            IsEnabled = true,
                            IsInvalid = true,
                            FailedValidationConditions = new List<FailedValidationCondition>()
                        }
                    }
                }

            });

            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter();

            interviewSynchronizationDtoFactory = CreateInterviewSynchronizationDtoFactory(questionnaireDocument);
        };

        Because of = () =>
            result = interviewSynchronizationDtoFactory.BuildFrom(interviewData, "comment", null, null);

        It should_return_specified_disabled_static_texts = () =>
        {
            result.DisabledStaticTexts.Count.ShouldEqual(2);
            result.DisabledStaticTexts.ShouldEachConformTo(sttid => sttid == disabledStaticText1Id || sttid == disabledStaticText2Id);
        };

        It should_return_specified_valid_static_texts = () =>
        {
            result.ValidStaticTexts.Count.ShouldEqual(0);
        };

        It should_return_specified_invalid_static_texts = () =>
        {
            result.InvalidStaticTexts.Count.ShouldEqual(2);
            result.InvalidStaticTexts.ShouldEachConformTo(sttid => sttid.Key == invalidStaticText1Id || sttid.Key == invalidStaticText2Id);
        };

        private static InterviewSynchronizationDtoFactory interviewSynchronizationDtoFactory;
        private static InterviewData interviewData;
        private static InterviewSynchronizationDto result;

        private static readonly Identity disabledStaticText1Id = Create.Identity(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);
        private static readonly Identity disabledStaticText2Id = Create.Identity(Guid.Parse("22222222222222222222222222222222"), new RosterVector(new decimal[]{1, 2, 3}));

        private static readonly Identity validStaticText1Id = Create.Identity(Guid.Parse("33333333333333333333333333333333"), RosterVector.Empty);
        private static readonly Identity validStaticText2Id = Create.Identity(Guid.Parse("44444444444444444444444444444444"), new RosterVector(new decimal[] { 1, 2, 3 }));

        private static readonly Identity invalidStaticText1Id = Create.Identity(Guid.Parse("55555555555555555555555555555555"), RosterVector.Empty);
        private static readonly Identity invalidStaticText2Id = Create.Identity(Guid.Parse("66666666666666666666666666666666"), new RosterVector(new decimal[] { 1, 2, 3 }));
    }
}
