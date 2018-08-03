using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public class AssignmentDocumentFromDtoBuilder : IAssignmentDocumentFromDtoBuilder
    {
        private readonly IAnswerToStringConverter answerToStringConverter;
        private readonly IInterviewAnswerSerializer answerSerializer;

        public AssignmentDocumentFromDtoBuilder(IAnswerToStringConverter answerToStringConverter, IInterviewAnswerSerializer answerSerializer)
        {
            this.answerToStringConverter = answerToStringConverter;
            this.answerSerializer = answerSerializer;
        }

        public AssignmentDocument GetAssignmentDocument(AssignmentApiDocument remote, IQuestionnaire questionnaire)
        {
            var local = new AssignmentDocument
            {
                Id = remote.Id,
                QuestionnaireId = remote.QuestionnaireId.ToString(),
                Title = questionnaire.Title,
                Quantity = remote.Quantity,
                CreatedInterviewsCount = 0,
                LocationQuestionId = remote.LocationQuestionId,
                LocationLatitude = remote.LocationLatitude,
                LocationLongitude = remote.LocationLongitude,
                ResponsibleId = remote.ResponsibleId,
                ResponsibleName = remote.ResponsibleName,
                OriginalResponsibleId = remote.ResponsibleId,
                ReceivedDateUtc = remote.CreatedAtUtc,
                ProtectedVariables = remote.ProtectedVariables?.Select(x => new AssignmentDocument.AssignmentProtectedVariable
                {
                    Variable = x,
                    AssignmentId = remote.Id
                }).ToList() ?? new List<AssignmentDocument.AssignmentProtectedVariable>()
            };

            this.FillAnswers(remote, questionnaire, local);

            return local;
        }

        private void FillAnswers(AssignmentApiDocument remote, IQuestionnaire questionnaire, AssignmentDocument local)
        {
            var identifyingQuestionIds = questionnaire.GetPrefilledQuestions().ToHashSet();

            local.LocationLatitude = remote.LocationLatitude;
            local.LocationLongitude = remote.LocationLongitude;
            local.LocationQuestionId = remote.LocationQuestionId;

            var answers = new List<AssignmentDocument.AssignmentAnswer>();
            var identiAnswers = new List<AssignmentDocument.AssignmentAnswer>();

            foreach (var answer in remote.Answers)
            {
                var isIdentifying = identifyingQuestionIds.Contains(answer.Identity.Id);

                var assignmentAnswer = new AssignmentDocument.AssignmentAnswer
                {
                    AssignmentId = remote.Id,
                    Identity = answer.Identity,
                    SerializedAnswer = answer.SerializedAnswer,
                    Question = isIdentifying ? questionnaire.GetQuestionTitle(answer.Identity.Id) : null,
                    IsIdentifying = isIdentifying
                };

                if (isIdentifying && questionnaire.GetQuestionType(answer.Identity.Id) != QuestionType.GpsCoordinates)
                {
                    var abstractAnswer = this.answerSerializer.Deserialize<AbstractAnswer>(answer.SerializedAnswer);
                    var answerAsString = this.answerToStringConverter.Convert(abstractAnswer?.ToString(), answer.Identity.Id, questionnaire);
                    assignmentAnswer.AnswerAsString = answerAsString;

                    identiAnswers.Add(assignmentAnswer);
                }

                answers.Add(assignmentAnswer);
            }

            local.Answers = answers;
            local.IdentifyingAnswers = identiAnswers;
        }
    }
}
