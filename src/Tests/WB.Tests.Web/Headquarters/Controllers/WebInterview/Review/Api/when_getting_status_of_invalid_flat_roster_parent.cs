using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    public class when_getting_status_of_invalid_flat_roster_parent : WebInterviewInterviewEntityFactorySpecification
    {
        public InterviewGroupOrRosterInstance entity;
        
        protected override QuestionnaireDocument GetDocument()
        {
            return Create.Entity.QuestionnaireDocumentWithOneChapter(Id.Identity1.Id,
               Create.Entity.FixedRoster(Id.g10, 
                    children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(Id.IdentityA_0.Id, validationConditions:new List<ValidationCondition>()
                            { new ValidationCondition("false", "error")})
                    },
                    fixedTitles: new []
                    {
                        Create.Entity.FixedTitle(0, "Test"),
                        Create.Entity.FixedTitle(1, "Test2")
                    },
                    displayMode: RosterDisplayMode.Flat)
            );
        }

        protected override void Because()
        {
            // act
            CurrentInterview.Apply(Create.Event.TextQuestionAnswered(Id.IdentityA_0.Id, Id.IdentityA_0.RosterVector, Id.g1.ToString()));

            var failedConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
            {
                {
                    Id.IdentityA_0,
                    new List<FailedValidationCondition> {new FailedValidationCondition(0)}
                }
            };

            CurrentInterview.Apply(Create.Event.AnswersDeclaredInvalid(failedConditions));
            
            this.entity = Subject.GetEntityDetails(Id.Identity1.ToString(), CurrentInterview, questionnaire, true)
                as InterviewGroupOrRosterInstance;
        }

        [Test]
        public void should_parent_get_StartedInvalid_status_and_invalid()
        {
            Assert.That(entity.Status, Is.EqualTo(GroupStatus.StartedInvalid));
            Assert.That(entity.Validity.IsValid, Is.False);
        }

    }
}
