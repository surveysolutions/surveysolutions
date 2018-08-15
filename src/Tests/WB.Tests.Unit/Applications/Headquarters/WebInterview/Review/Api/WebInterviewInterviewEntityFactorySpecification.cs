using System;
using System.Collections.Generic;
using AutoMapper;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    [TestOf(typeof(WebInterviewInterviewEntityFactory))]
    public abstract class WebInterviewInterviewEntityFactorySpecification
    {
        protected static readonly Identity SectionA = Id.IdentityA;
        protected static readonly Identity SecA_InterviewerQuestion = Id.Identity1;
        protected static readonly Identity SecA_SupervisorQuestion = Id.Identity2;
        protected static readonly Identity SecA_Roster = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAA11111111111111111"), 1);
        protected static readonly Identity SecA_Roster_InterviewerQuestion = Create.Identity(Id.g3, 1);
        protected static readonly Identity SecA_Roster_SupervisorQuestion = Create.Identity(Id.g4, 1);

        protected StatefulInterview CurrentInterview;

        private Mock<IAuthorizedUser> authorizedUserMock;
        private QuestionnaireDocument document;
        protected IQuestionnaire questionnaire;

        protected WebInterviewInterviewEntityFactory Subject { get; set; }

        protected virtual bool IsReviewMode { get; set; }

        [OneTimeSetUp]
        public void Prepare()
        {
            document = GetDocument();
            questionnaire = Create.Entity.PlainQuestionnaire(document);

            var autoMapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebInterviewAutoMapProfile());
            });

            authorizedUserMock = new Mock<IAuthorizedUser>();

            Subject = new HqWebInterviewInterviewEntityFactory(autoMapperConfig.CreateMapper(),
                authorizedUserMock.Object,
                new EnumeratorGroupGroupStateCalculationStrategy(), 
                new SupervisorGroupStateCalculationStrategy());
        }

        [SetUp]
        public void Setup()
        {
            CurrentInterview = Create.AggregateRoot.StatefulInterview(Guid.NewGuid(), questionnaire: document);
            Because();
        }

        protected virtual void Because()
        {
        }

        protected abstract QuestionnaireDocument GetDocument();

        protected void AnswerTextQuestions(params Identity[] ids)
        {
            foreach (var id in ids)
                CurrentInterview.Apply(Create.Event.TextQuestionAnswered(id.Id, id.RosterVector, id.ToString()));
        }

        protected void MarkQuestionAsInvalid(params Identity[] ids)
        {
            foreach (var identity in ids)
            {
                var failedConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
                {
                    {
                        identity,
                        new List<FailedValidationCondition> {new FailedValidationCondition(0)}
                    }
                };

                CurrentInterview.Apply(Create.Event.AnswersDeclaredInvalid(failedConditions));
            }
        }

        protected InterviewGroupOrRosterInstance GetGroupDetails(Identity id, bool asReviewer)
        {
            if (asReviewer)
                AsReviewer();
            else
                AsInterviewer();

            var entity = Subject.GetEntityDetails(id.ToString(), CurrentInterview, questionnaire, IsReviewMode);

            return entity as InterviewGroupOrRosterInstance;
        }

        protected void AsInterviewer()
        {
            IsReviewMode = false;
            authorizedUserMock.Setup(au => au.IsSupervisor).Returns(false);
        }

        protected void AsReviewer()
        {
            IsReviewMode = true;
            authorizedUserMock.Setup(au => au.IsSupervisor).Returns(true);
        }
    }
}
